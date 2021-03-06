//=============================================================================
// Definitions
//-----------------------------------------------------------------------------
#define MAX_LEVEL_COUNT 15
// (debug) to color by a height.
#define HEIGHT_INDEX_COUNT 8
// (debug) to color by a height.
#define HEIGHT_COLOR_BLEND_RATIO 0.8f

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

float LevelCount;
float2 MorphConsts[MAX_LEVEL_COUNT];

// x = (textureWidth - 1.0f) / textureWidth
// y = (textureHeight - 1.0f) / textureHeight
float2 SamplerWorldToTextureScale;

float2 HeightMapSize;
float2 TwoHeightMapSize;
// x = 1 / textureWidth
// y = 1 / textureHeight
float2 HeightMapTexelSize;
float2 TwoHeightMapTexelSize;

// [g_gridDim.y] on the original code.
float HalfPatchGridSize;
// [g_gridDim.z] on the original code.
float TwoOverPatchGridSize;

// (debug) to color by a height.
float4 HeightColors[] =
{
    { 0,       0,       0.5f,    1 },
    { 0,       0,       1,       1 },
    { 0,       0.5f,    1,       1 },
    { 0.9411f, 0.9411f, 0.2509f, 1 },
    { 0.1254f, 0.6274f, 0,       1 },
    { 0.8784f, 0.8784f, 0,       1 },
//    { 0.2509f, 0.2509f, 0.2509f, 1 },
    { 0.5,     0.5f,    0.5f,    1 },
    { 1,       1,       1,       1 }
};
// (debug) to color by a height.
float Heights[] =
{
    -1.0000f,
    -0.2500f,
    0.0000f,
    0.0625f,
    0.1250f,
    0.3750f,
    0.7500f,
    1.0000f
};

// (debug) to turn on/off a light.
bool LightEnabled;

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
    // (debug) to color by a height.
    float Height : TEXCOORD2;
};

//=============================================================================
// Vertex shader helper
//-----------------------------------------------------------------------------
float2 CalculateGlobalUV(float4 vertex)
{
    return vertex.xz / TerrainScale.xz;
}

float2 MorphVertex(float4 position, float2 vertex, float4 quadScale, float morphLerpK)
{
    float2 fracPart = frac(position.xz * float2(HalfPatchGridSize, HalfPatchGridSize));
    fracPart *= float2(TwoOverPatchGridSize, TwoOverPatchGridSize);
    fracPart *= quadScale.xz;
    return vertex - fracPart * morphLerpK;
}

float SampleHeightMap(float2 uv)
{
    // A manual bilinear interpolation.
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

float4 CalculateNormal(float2 texCoord)
{
    float n = SampleHeightMap(texCoord + float2(0, -HeightMapTexelSize.y));
    float s = SampleHeightMap(texCoord + float2(0,  HeightMapTexelSize.y));
    float e = SampleHeightMap(texCoord + float2(-HeightMapTexelSize.x, 0));
    float w = SampleHeightMap(texCoord + float2( HeightMapTexelSize.x, 0));

    float3 sn = float3(0, 1, (s - n) * TerrainScale.y);
    float3 ew = float3(1, 0, (e - w) * TerrainScale.y);
    sn = normalize(sn);
    ew = normalize(ew);

    return float4(normalize(cross(sn, ew)), 1);
}

//=============================================================================
// Vertex shader
//-----------------------------------------------------------------------------
// instanceParam0:
//      x: QuadOffset.x
//      y: QuadOffset.z
//      z: QuadScale (x and z shared)
//      w: Level
VS_OUTPUT VS(
    VS_INPUT input,
    float4 instanceParam0 : TEXCOORD1)
{
    VS_OUTPUT output;

    // calculate base vertex position.
    float4 quadOffset = float4(instanceParam0.x, 0, instanceParam0.y, 0);
    float4 quadScale = float4(instanceParam0.z, 0, instanceParam0.z, 0);
    int level = floor(instanceParam0.w);

    // terrain relative vertex.
    float4 vertex = input.Position * quadScale + quadOffset;
    float2 preGlobalUV = CalculateGlobalUV(vertex);
    vertex.y = SampleHeightMap(preGlobalUV);
    vertex.y *= TerrainScale.y;

    // the distance between the world vertex and the eye position.
    float eyeDistance = distance(vertex.xyz + TerrainOffset, EyePosition);
    float morphLerpK = 1 - saturate(MorphConsts[level].x - eyeDistance * MorphConsts[level].y);

    // morph xz.
    vertex.xz = MorphVertex(input.Position, vertex.xz, quadScale, morphLerpK);

    // get a height with morphed xz.
    float2 globalUV = CalculateGlobalUV(vertex);
    float h = SampleHeightMap(globalUV);
    vertex.y = h;
    vertex.y *= TerrainScale.y;
    // to world vertex
    vertex.xyz += TerrainOffset;
    vertex.w = 1;

    float4x4 viewProjection = mul(View, Projection);
    output.Position = mul(vertex, viewProjection);
    output.TexCoord = globalUV;
    output.EyeDirection = normalize(EyePosition - vertex.xyz);

    output.Normal = CalculateNormal(globalUV);

    // (debug) to decide a height color.
    output.Height = h;

    return output;
}

//=============================================================================
// Pixel shader helper
//-----------------------------------------------------------------------------
float3 CalculateLight(float3 E, float3 N)
{
    float3 diffuse = AmbientLightColor;

    float3 L = -LightDirection;
    float3 H = normalize(E + L);
    float dt = max(0, dot(L, N));
    diffuse += DiffuseLightColor * dt;
    return diffuse;
}

//=============================================================================
// Pixel shader
//-----------------------------------------------------------------------------
float4 WhiteSolidPS(VS_OUTPUT input) : COLOR0
{
    // Always white.
    float4 color = float4(1, 1, 1, 1);

    if (LightEnabled)
    {
        float3 normal = input.Normal.xyz;
        float3 E = normalize(input.EyeDirection);
        float3 N = normalize(input.Normal.xyz);
        float3 light = CalculateLight(E, N);
        color.rgb *= light;
    }

    return color;
}

float4 HeightColorPS(VS_OUTPUT input) : COLOR0
{
    float4 color = float4(0, 0, 0, 1);

    float weight;
    float4 range;

    // The height color blending.
    // index = 0
    range = (Heights[1] - Heights[0]) * HEIGHT_COLOR_BLEND_RATIO;
    weight = saturate(1 - abs(input.Height - Heights[0]) / range);
    color.rgb += HeightColors[0] * weight;
    // index = [1, HEIGHT_INDEX_COUNT - 2]
    for (int i = 1; i < HEIGHT_INDEX_COUNT - 1; i++)
    {
        range = (Heights[i + 1] - Heights[i - 1]) * 0.5f * HEIGHT_COLOR_BLEND_RATIO;
        weight = saturate(1 - abs(input.Height - Heights[i]) / range);
        color.rgb += HeightColors[i] * weight;
    }
    // index = HEIGHT_INDEX_COUNT - 1
    range = (Heights[HEIGHT_INDEX_COUNT - 1] - Heights[HEIGHT_INDEX_COUNT - 2]) * HEIGHT_COLOR_BLEND_RATIO;
    weight = saturate(1 - abs(input.Height - Heights[HEIGHT_INDEX_COUNT - 1]) / range);
    color.rgb += HeightColors[HEIGHT_INDEX_COUNT - 1] * weight;

    if (LightEnabled)
    {
        float3 normal = input.Normal.xyz;
        float3 E = normalize(input.EyeDirection);
        float3 N = normalize(input.Normal.xyz);
        float3 light = CalculateLight(E, N);
        color.rgb *= light;
    }

    return color;
}

float4 WireframePS(VS_OUTPUT input) : COLOR0
{
    return float4(0, 0, 0, 1);
}

//=============================================================================
// Technique
//-----------------------------------------------------------------------------
technique WhiteSolid
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 WhiteSolidPS();
    }
}

technique HeightColor
{
    pass P0
    {
        FillMode = SOLID;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 HeightColorPS();
    }
}

technique Wireframe
{
    pass P0
    {
        FillMode = WIREFRAME;
        CullMode = CCW;
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 WireframePS();
    }
}
