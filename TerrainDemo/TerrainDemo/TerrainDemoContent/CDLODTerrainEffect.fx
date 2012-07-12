//=============================================================================
// Variables
//-----------------------------------------------------------------------------
float4x4 View;
float4x4 Projection;
float3 EyePosition;

float3 AmbientLightColor;
float3 LightDirection;
float3 DiffuseLightColor;

float3 TerrainOffset;
float3 TerrainScale;

// x = (textureWidth - 1.0f) / textureWidth
// y = (textureHeight - 1.0f) / textureHeight
float2 SamplerWorldToTextureScale;
// HeightMapTexelSize = 1 / heightMapSize
float HeightMapTexelSize;

texture HeightMap;
sampler HeightMapSampler = sampler_state
{
    Texture = <HeightMap>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

texture NormalMap;
sampler NormalMapSampler = sampler_state
{
    Texture = <NormalMap>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

//=============================================================================
// Structure
//-----------------------------------------------------------------------------
struct VS_INPUT
{
    float4 Position : POSITION0;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 EyeDirection : TEXCOORD1;
// デバッグ用。
    float4 Color : COLOR;
};

//=============================================================================
// Vertex shader helper
//-----------------------------------------------------------------------------
float2 CalculateGlobalUV(float4 vertex)
{
    float2 globalUV = (vertex.xz - TerrainOffset.xz) / TerrainScale.xz;
    globalUV *= SamplerWorldToTextureScale;
    globalUV += float2(HeightMapTexelSize, HeightMapTexelSize) * 0.5;
    return globalUV;
}

float2 MorphVertex(float4 position, float2 vertex, float4 quadScale, float morphLerpK)
{
    float2 fracPart = frac(position.xz) * quadScale.xz;
    return vertex - fracPart * morphLerpK;
}

float SampleHeightMap(float2 uv)
{
    return tex2Dlod(HeightMapSampler, float4(uv, 0, 0)).x;
/*
    const float2 texelSize   = float2(HeightMapTexelSize, HeightMapTexelSize);
    const float2 textureSize = float2(1, 1) / texelSize;

    uv = uv.xy * textureSize - float2(0.5, 0.5);
    float2 uvf = floor( uv.xy );
    float2 f = uv - uvf;
    uv = (uvf + float2(0.5, 0.5)) * texelSize;

    float t00 = tex2Dlod( HeightMapSampler, float4( uv.x, uv.y, 0, 0 ) ).x;
    float t10 = tex2Dlod( HeightMapSampler, float4( uv.x + texelSize.x, uv.y, 0, 0 ) ).x;

    float tA = lerp( t00, t10, f.x );

    float t01 = tex2Dlod( HeightMapSampler, float4( uv.x, uv.y + texelSize.y, 0, 0 ) ).x;
    float t11 = tex2Dlod( HeightMapSampler, float4( uv.x + texelSize.x, uv.y + texelSize.y, 0, 0 ) ).x;

    float tB = lerp( t01, t11, f.x );

    return lerp( tA, tB, f.y );*/
}

//=============================================================================
// Vertex shader
//-----------------------------------------------------------------------------
// instanceParam0:
//      x: QuadOffset.x
//      y: QuadOffset.z
//      z: QuadScale (x, z 共通)
//      w: Level
// instanceParam1: morphConsts
VS_OUTPUT VS(
    VS_INPUT input,
    float4 instanceParam0 : TEXCOORD1,
    float4 instanceParam1 : TEXCOORD2)
{
    VS_OUTPUT output;

    // ベースとなる頂点座標の算出。
    float4 quadOffset = float4(instanceParam0.x, 0, instanceParam0.y, 0);
    float4 quadScale = float4(instanceParam0.z, 0, instanceParam0.z, 0);
    int level = floor(instanceParam0.w);

    float4 vertex = input.Position * quadScale + quadOffset;
    float2 preGlobalUV = CalculateGlobalUV(vertex);
    vertex.y = SampleHeightMap(preGlobalUV);
    vertex.y *= TerrainScale.y;
    vertex.y += TerrainOffset.y;

    float eyeDistance = distance(vertex.xyz, EyePosition);
    float morphLerpK = 1 - saturate(instanceParam1.x - eyeDistance * instanceParam1.y);

    // xz をモーフィング。
    vertex.xz = MorphVertex(input.Position, vertex.xz, quadScale, morphLerpK);

    // モーフィング結果で高さを再取得。
    float2 globalUV = CalculateGlobalUV(vertex);
    vertex.y = SampleHeightMap(globalUV);
    vertex.y *= TerrainScale.y;
    vertex.y += TerrainOffset.y;
    vertex.w = 1;

    float4x4 viewProjection = mul(View, Projection);
    output.Position = mul(vertex, viewProjection);
    output.TexCoord = globalUV;
    output.EyeDirection = normalize(EyePosition - vertex.xyz);

// デバッグ。
    output.Color = float4(0, 0, 0, 1);
    level %= 3;
    if (0 == level)
        output.Color.r = 1;
    else if (1 == level)
        output.Color.g = 1;
    else if (2 == level)
        output.Color.b = 1;

    return output;
}

//=============================================================================
// Pixel shader helper
//-----------------------------------------------------------------------------
float CalculateDiffuseStrength(float3 normal, float3 lightDir)
{
   return saturate(-dot( normal, lightDir ));
}

float CalculateSpecularStrength(float3 normal, float3 lightDir, float3 eyeDir)
{
   float3 diff = saturate(dot(normal, -lightDir));
   float3 reflect = normalize(2 * diff * normal + lightDir);

   return saturate(dot(reflect, eyeDir));
}

float CalculateDirectionalLight(float3 normal, float3 lightDir, float3 eyeDir, float specularPow, float specularMul)
{
   float3 light0 = normalize(lightDir);

   return CalculateDiffuseStrength(normal, light0) + specularMul * pow(CalculateSpecularStrength(normal, light0, eyeDir), specularPow);
}

float3 CalculateNormal(float2 texCoord)
{
    float n = tex2D(NormalMapSampler, texCoord + float2(0, -HeightMapTexelSize)).x;
    float s = tex2D(NormalMapSampler, texCoord + float2(0,  HeightMapTexelSize)).x;
    float e = tex2D(NormalMapSampler, texCoord + float2(-HeightMapTexelSize, 0)).x;
    float w = tex2D(NormalMapSampler, texCoord + float2( HeightMapTexelSize, 0)).x;

    float twoTexel = HeightMapTexelSize * 2;

    float3 ew = normalize(float3(twoTexel, e - w, 0));
    float3 ns = normalize(float3(s - n, twoTexel, 0));

    return normalize(cross(ew, ns));
}

//=============================================================================
// Pixel shader
//-----------------------------------------------------------------------------
float4 PS(VS_OUTPUT input) : COLOR0
{
/*    float3 normal = tex2D(NormalMapSampler, input.TexCoord);
    normal.xz = normal.xz * float2(2, 2) - float2(1, 1);
    normal.y = sqrt(1 - normal.x * normal.x - normal.z * normal.z);

    float directionalLight = CalculateDirectionalLight(normal, normalize(LightDirection), normalize(input.EyeDirection), 16, 0);
    float4 color = float4(AmbientLightColor + DiffuseLightColor * directionalLight, 1);
    return color;
*/

// デバッグ。
/*    float3 normal = CalculateNormal(input.TexCoord);
    float directionalLight = CalculateDirectionalLight(normal, normalize(LightDirection), normalize(input.EyeDirection), 16, 0);
    float4 color = float4(AmbientLightColor + DiffuseLightColor * directionalLight, 1);
    return color;*/

// デバッグ。
/*    float3 normal = CalculateNormal(input.TexCoord);
    float intensity = cross(normalize(LightDirection), normal);
    float4 color = input.Color;
    color.rgb *= 0.5;
    color.rgb += intensity * 0.5;
    return color;*/

// デバッグ。
    return input.Color;
}

//=============================================================================
// Technique
//-----------------------------------------------------------------------------
technique Default
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
