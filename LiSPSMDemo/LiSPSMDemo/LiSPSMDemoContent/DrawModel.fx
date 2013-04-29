float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 LightViewProjection;
float3   LightDirection;
float    DepthBias;
float4   AmbientColor;

texture Texture     : register(t0);
texture ShadowMap   : register(t1);

sampler TextureSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
};

sampler ShadowMapSampler : register(s1)= sampler_state
{
    Texture = <ShadowMap>;
};

struct VSInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float3 Normal   : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float4 WorldPos : TEXCOORD2;
};

VSOutput VS(VSInput input)
{
    VSOutput output;

    float4x4 WorldViewProj = mul(mul(World, View), Projection);

    output.Position = mul(input.Position, WorldViewProj);
    output.Normal =  normalize(mul(float4(input.Normal, 0), World)).xyz;
    output.TexCoord = input.TexCoord;

    output.WorldPos = mul(input.Position, World);

    return output;
}

float4 BasicPS(VSOutput input) : COLOR0
{ 
    float4 diffuseColor = tex2D(TextureSampler, input.TexCoord);

    float diffuseIntensity = saturate(dot(-LightDirection, input.Normal));
    float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;

    float4 lightingPosition = mul(input.WorldPos, LightViewProjection);

    float2 shadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2( 0.5, 0.5 );
    shadowTexCoord.y = 1.0f - shadowTexCoord.y;

    float shadowdepth = tex2D(ShadowMapSampler, shadowTexCoord).x;

    float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias;

    if (shadowdepth < ourdepth)
    {
        diffuse *= float4(0.5,0.5,0.5,0);
    };

    return diffuse;
}

float TestVSM(float4 position, float2 shadowTexCoord)
{
    float2 moments = tex2D(ShadowMapSampler, shadowTexCoord).xy;

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

    float4 lightingPosition = mul(input.WorldPos, LightViewProjection);

    float2 shadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2( 0.5, 0.5 );
    shadowTexCoord.y = 1.0f - shadowTexCoord.y;

    float shadow = TestVSM(lightingPosition, shadowTexCoord);

    // 最も影な部分を 0.5 にするための調整。
    shadow *= 0.5f;
    shadow += 0.5f;

    diffuse *= shadow;

    return diffuse;
}

technique Basic
{
    pass Default
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 BasicPS();
    }
}

technique Variance
{
    pass Default
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 VariancePS();
    }
}
