#define MAX_SPLIT_COUNT 3

float4x4 World;
float4x4 View;
float4x4 Projection;
float4   AmbientColor;
float    DepthBias;
int      SplitCount;
float3   LightDirection;
float3   ShadowColor;
float    SplitDistances[MAX_SPLIT_COUNT + 1];
float4x4 LightViewProjections[MAX_SPLIT_COUNT];

texture Texture : register(t0);
sampler TextureSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
};

texture ShadowMap0 : register(t1);
#if MAX_SPLIT_COUNT > 1
texture ShadowMap1 : register(t2);
#endif
#if MAX_SPLIT_COUNT > 2
texture ShadowMap2 : register(t3);
#endif

sampler ShadowMapSampler[MAX_SPLIT_COUNT] : register(s1) =
{
    sampler_state
    {
        Texture = <ShadowMap0>;
    },
#if MAX_SPLIT_COUNT > 1
    sampler_state
    {
        Texture = <ShadowMap1>;
    },
#endif
#if MAX_SPLIT_COUNT > 2
    sampler_state
    {
        Texture = <ShadowMap2>;
    },
#endif
};

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position   : POSITION0;
    float3 Normal     : TEXCOORD0;
    float2 TexCoord   : TEXCOORD1;
    float4 PositionWS : TEXCOORD2;
    float4 PositionVS : TEXCOORD3;
};

VSOutput VS(VSInput input)
{
    VSOutput output;

    float4 positionWS = mul(input.Position, World);
    float4 positionVS = mul(positionWS, View);

    output.Position = mul(positionVS, Projection);
    output.Normal =  normalize(mul(float4(input.Normal, 0), World)).xyz;
    output.TexCoord = input.TexCoord;

    output.PositionWS = positionWS;
    output.PositionVS = positionVS;

    return output;
}

float4 BasicPS(VSOutput input) : COLOR0
{ 
    float4 diffuseColor = tex2D(TextureSampler, input.TexCoord);

    float diffuseIntensity = saturate(dot(-LightDirection, input.Normal));
    float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;

    float distance = abs(input.PositionVS.z);

    float shadow = 0;

    [unroll]
    for (int i = 0; i < SplitCount; i++)
    {
        float depthLS = 0;
        float depthShadowMap = 0;

        if (SplitDistances[i] <= distance && distance < SplitDistances[i + 1])
        {
            float4 positionLS = mul(input.PositionWS, LightViewProjections[i]);
            depthLS = (positionLS.z / positionLS.w) - DepthBias;

            float2 shadowMapTexCoord = 0.5 * positionLS.xy / positionLS.w + float2(0.5, 0.5);
            shadowMapTexCoord.y = 1 - shadowMapTexCoord.y;

            depthShadowMap = tex2D(ShadowMapSampler[i], shadowMapTexCoord).x;

            shadow = (depthShadowMap < depthLS);

            break;
        }
    }

    float3 blendShadowColor = lerp(float3(1, 1, 1), ShadowColor, shadow);

    diffuse.xyz *= blendShadowColor;

    return diffuse;
}

float TestVSM(float4 position, float2 moments)
{
    float Ex = moments.x;
    float E_x2 = moments.y;
    float Vx = E_x2 - Ex * Ex;
    Vx = min(1, max(0, Vx + 0.00001f));
    float t = position.z / position.w - DepthBias;
    float tMinusM = t - Ex;
    float p = Vx / (Vx + tMinusM * tMinusM);

    // チェビシェフの不等式により t > Ex で p が有効。
    // t <= Ex では p = 1、つまり、影がない。
    return saturate(max(p, t <= Ex));
}

float4 VariancePS(VSOutput input) : COLOR0
{ 
    float4 diffuseColor = tex2D(TextureSampler, input.TexCoord);

    float diffuseIntensity = saturate(dot(-LightDirection, input.Normal));
    float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;

    float distance = abs(input.PositionVS.z);

    float shadow = 0;

    [unroll]
    for (int i = 0; i < SplitCount; i++)
    {
        if (SplitDistances[i] <= distance && distance < SplitDistances[i + 1])
        {
            float4 positionLS = mul(input.PositionWS, LightViewProjections[i]);

            float2 shadowMapTexCoord = 0.5 * positionLS.xy / positionLS.w + float2( 0.5, 0.5 );
            shadowMapTexCoord.y = 1 - shadowMapTexCoord.y;

            // Sample() では gradient-based operation に関する警告が発生。
            // これは SampleLevel() で LOD を明示することで解決可能。
            float2 moments = tex2D(ShadowMapSampler[i], shadowMapTexCoord).xy;
            float test = TestVSM(positionLS, moments);

            shadow = (1 - test);

            break;
        }
    }

    float3 blendShadowColor = lerp(float3(1, 1, 1), ShadowColor, shadow);

    diffuse.xyz *= blendShadowColor;

    return diffuse;
}

technique Basic
{
    pass Default
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 BasicPS();
    }
}

technique Variance
{
    pass Default
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 VariancePS();
    }
}
