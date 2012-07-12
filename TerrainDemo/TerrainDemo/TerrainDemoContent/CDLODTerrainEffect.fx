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

float2 HeightMapSize;
// x = 1 / textureWidth
// y = 1 / textureHeight
float2 HeightMapTexelSize;

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
    float4 Normal : NORMAL0;
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
    globalUV += HeightMapTexelSize * 0.5;
    return globalUV;
}

float2 MorphVertex(float4 position, float2 vertex, float4 quadScale, float morphLerpK)
{
    float2 fracPart = frac(position.xz) * quadScale.xz;
    return vertex - fracPart * morphLerpK;
}

float SampleHeightMap(float2 uv)
{
//    return tex2Dlod(HeightMapSampler, float4(uv, 0, 0)).x;

// 単純に tex2Dlod で値を取りたいが、
// 例えば leafNodeSize = 8 かつパッチ サイズ 32 などを行った場合、
// TextureFilter = POINT の影響により段階的な値の取得となってしまう。
// XNA 4.0 では SurfaceFormat.Single で TextureFilter = Linear は不可能？
// HiDef ですらできないのだが。
// ゆえに、コード上での bilinear filtering。
    uv = uv.xy * HeightMapSize - float2(0.5, 0.5);
    float2 uvf = floor( uv.xy );
    float2 f = uv - uvf;
    uv = (uvf + float2(0.5, 0.5)) * HeightMapTexelSize;

    float t00 = tex2Dlod( HeightMapSampler, float4( uv.x, uv.y, 0, 0 ) ).x;
    float t10 = tex2Dlod( HeightMapSampler, float4( uv.x + HeightMapTexelSize.x, uv.y, 0, 0 ) ).x;

    float tA = lerp( t00, t10, f.x );

    float t01 = tex2Dlod( HeightMapSampler, float4( uv.x, uv.y + HeightMapTexelSize.y, 0, 0 ) ).x;
    float t11 = tex2Dlod( HeightMapSampler, float4( uv.x + HeightMapTexelSize.x, uv.y + HeightMapTexelSize.y, 0, 0 ) ).x;

    float tB = lerp( t01, t11, f.x );

    return lerp( tA, tB, f.y );
}

float4 VSCalculateNormal(float2 texCoord)
{
// From http://graphics.ethz.ch/teaching/gamelab11/course_material/lecture06/XNA_Shaders_Terrain.pdf
/*    float n = tex2Dlod(HeightMapSampler, float4( texCoord + float2(0, -HeightMapTexelSize.x), 0, 1) ).x;
    float s = tex2Dlod(HeightMapSampler, float4( texCoord + float2(0,  HeightMapTexelSize.x), 0, 1) ).x;
    float e = tex2Dlod(HeightMapSampler, float4( texCoord + float2(-HeightMapTexelSize.y, 0), 0, 1) ).x;
    float w = tex2Dlod(HeightMapSampler, float4( texCoord + float2( HeightMapTexelSize.y, 0), 0, 1) ).x;*/

    float n = SampleHeightMap(texCoord + float2(0, -HeightMapTexelSize.x));
    float s = SampleHeightMap(texCoord + float2(0,  HeightMapTexelSize.x));
    float e = SampleHeightMap(texCoord + float2(-HeightMapTexelSize.y, 0));
    float w = SampleHeightMap(texCoord + float2( HeightMapTexelSize.y, 0));

    float2 twoTexel = HeightMapTexelSize * 2;
    float2 divisor = float2(1, 1) / twoTexel;

    float3 sn = float3(0, (s - n) * TerrainScale.y, -twoTexel.y);
    float3 ew = float3(-twoTexel.x, (e - w) * TerrainScale.y, 0);
    sn *= divisor.y;
    ew *= divisor.x;
    sn = normalize(sn);
    ew = normalize(ew);

    float4 normal = float4(normalize(cross(sn, ew)), 1);
    normal.x = -normal.x;

    return normal;
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

    output.Normal = VSCalculateNormal(globalUV);

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
    return color;*/

// デバッグ。
/*    float3 normal = input.Normal.xyz;
    float directionalLight = CalculateDirectionalLight(normal, normalize(LightDirection), normalize(input.EyeDirection), 16, 0);
    float4 color = float4(AmbientLightColor + input.Color * DiffuseLightColor * directionalLight, 1);
    return color;*/

// デバッグ。
//    return input.Color;
    return float4(0, 0, 0, 1);
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
