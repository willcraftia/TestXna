//=============================================================================
// Definitions
//-----------------------------------------------------------------------------
#define MAX_LEVEL_COUNT 15
// (debug) to color by a height.
#define MAX_HEIGHT_COLOR_COUNT 8

//=============================================================================
// Variables
//-----------------------------------------------------------------------------
//float4x4 View;
float4x4 Projection;
float3 EyePosition;

bool LightEnabled;
float3 AmbientLightColor;
float3 LightDirection;
float3 DiffuseLightColor;

float FogEnabled;
float FogStart;
float FogEnd;
float3 FogColor;

//float3 TerrainOffset;
// eye position in terrain space.
float3 TerrainEyePosition;
float4x4 TerrainView;
float3 TerrainScale;
float3 InverseTerrainScale;

float LevelCount;
float2 MorphConsts[MAX_LEVEL_COUNT];

float2 HeightMapSize;
float2 TwoHeightMapSize;
// x = 1 / textureWidth
// y = 1 / textureHeight
float2 HeightMapTexelSize;
float2 TwoHeightMapTexelSize;
float HeightMapOverlapSize;

// [g_gridDim.y] on the original code.
float HalfPatchGridSize;
// [g_gridDim.z] on the original code.
float TwoOverPatchGridSize;

// (debug) for height color..
float HeightColorCount = MAX_HEIGHT_COLOR_COUNT;
float4 HeightColors[MAX_HEIGHT_COLOR_COUNT];
float HeightColorPositions[MAX_HEIGHT_COLOR_COUNT];

texture HeightMap;
sampler HeightMapSampler  = sampler_state
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
    float FogFactor : TEXCOORD2;
    // (debug) to color by a height.
    float Height : TEXCOORD3;
};

//=============================================================================
// Vertex shader helper
//-----------------------------------------------------------------------------
float2 CalculateGlobalUV(float4 vertex)
{
/*  REFERENCE:

    float2 globalUV = vertex.xz / TerrainScale.xz;
    float2 actualSize = HeightMapSize - 2 * HeightMapOverlapSize;
    float2 worldToTexCoord = (actualSize - 1) * HeightMapTexelSize;
    globalUV *= worldToTexCoord;
    globalUV += (HeightMapOverlapSize + 0.5) * HeightMapTexelSize;

    therefore:
*/

    float2 globalUV = vertex.xz * InverseTerrainScale.xz;
    float2 overlapTexelSize = HeightMapOverlapSize * HeightMapTexelSize;

    // REFERENCE:
    //    float2 actualSize = HeightMapSize - 2 * HeightMapOverlapSize;
    //    float2 worldToTexCoord = (actualSize - 1) * HeightMapTexelSize;
    // therefore:
    float2 worldToTexCoord = 1 - 2 * overlapTexelSize - HeightMapTexelSize;
    globalUV *= worldToTexCoord;

    // REFERENCE:
    //    globalUV += HeightMapTexelSize * HeightMapOverlapSize + HeightMapTexelSize * 0.5;
    // therefore:
    globalUV += overlapTexelSize + 0.5 * HeightMapTexelSize;

    return globalUV;
}

float2 MorphVertex(float4 position, float2 vertex, float4 quadScale, float morphLerpK)
{
    float2 fracPart = frac(position.xz * HalfPatchGridSize);
    fracPart *= TwoOverPatchGridSize;
    fracPart *= quadScale.xz;
    return vertex - fracPart * morphLerpK;
}

float SampleHeightMap(float2 uv)
{
    // A manual bilinear interpolation.
    uv = uv.xy * HeightMapSize - 0.5;
    float2 uvf = floor( uv.xy );
    float2 f = uv - uvf;
    uv = (uvf + 0.5) * HeightMapTexelSize;

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
    // From http://graphics.ethz.ch/teaching/gamelab11/course_material/lecture06/XNA_Shaders_Terrain.pdf
    float n = SampleHeightMap(texCoord + float2(0, -HeightMapTexelSize.x));
    float s = SampleHeightMap(texCoord + float2(0,  HeightMapTexelSize.x));
    float e = SampleHeightMap(texCoord + float2(-HeightMapTexelSize.y, 0));
    float w = SampleHeightMap(texCoord + float2( HeightMapTexelSize.y, 0));

    float3 sn = float3(0, (s - n) * TerrainScale.y, -TwoHeightMapTexelSize.y);
    float3 ew = float3(-TwoHeightMapTexelSize.x, (e - w) * TerrainScale.y, 0);
    sn *= TwoHeightMapSize.y;
    ew *= TwoHeightMapSize.x;
    sn = normalize(sn);
    ew = normalize(ew);

    float4 normal = float4(normalize(cross(sn, ew)), 1);
    normal.x = -normal.x;

    return normal;
}

float CalculateFogFactor(float d)
{
    return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogEnabled;
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
    // REFERENCE:
    //    float eyeDistance = distance(vertex.xyz + TerrainOffset, EyePosition);
    // therefore:
    float eyeDistance = distance(vertex.xyz, TerrainEyePosition);
    float morphLerpK = 1 - saturate(MorphConsts[level].x - eyeDistance * MorphConsts[level].y);

    // morph xz.
    vertex.xz = MorphVertex(input.Position, vertex.xz, quadScale, morphLerpK);

    // get the height value on morphed xz.
    float2 globalUV = CalculateGlobalUV(vertex);
    float h = SampleHeightMap(globalUV);
    vertex.y = h;
    vertex.y *= TerrainScale.y;
    vertex.w = 1;

    // calculate the eye direction in terrain space.
    output.EyeDirection = normalize(TerrainEyePosition - vertex.xyz);

    // Calculate the final position with view matrix in terrain space.
    float4x4 viewProjection = mul(TerrainView, Projection);
    output.Position = mul(vertex, viewProjection);
    output.TexCoord = globalUV;
    output.Normal = CalculateNormal(globalUV);

    output.FogFactor = CalculateFogFactor(eyeDistance);

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

    color.rgb = lerp(color.rgb, FogColor, input.FogFactor);

    return color;
}

float4 HeightColorPS(VS_OUTPUT input) : COLOR0
{
    float4 color = float4(0, 0, 0, 1);

    float h = input.Height;

    int index;
    for (index = 0; index < HeightColorCount; index++)
    {
        if (h < HeightColorPositions[index]) break;
    }

    int maxIndex = HeightColorCount - 1;
    int index0 = clamp(index - 1, 0, maxIndex);
    int index1 = clamp(index    , 0, maxIndex);

    if (index0 == index1)
    {
        color = HeightColors[index1];
    }
    else
    {
        float amount = (h - HeightColorPositions[index0]) / (HeightColorPositions[index1] - HeightColorPositions[index0]);
        float4 color0 = HeightColors[index0];
        float4 color1 = HeightColors[index1];
        color.r = lerp(color0.r, color1.r, amount);
        color.g = lerp(color0.g, color1.g, amount);
        color.b = lerp(color0.b, color1.b, amount);
    }

    if (LightEnabled)
    {
        float3 normal = input.Normal.xyz;
        float3 E = normalize(input.EyeDirection);
        float3 N = normalize(input.Normal.xyz);
        float3 light = CalculateLight(E, N);
        color.rgb *= light;
    }

    color.rgb = lerp(color.rgb, FogColor, input.FogFactor);

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
