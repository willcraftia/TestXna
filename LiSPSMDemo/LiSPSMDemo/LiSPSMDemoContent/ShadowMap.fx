float4x4 World;
float4x4 LightViewProjection;

struct VSOutput
{
    float4 Position     : POSITION0;
    float4 PositionWVP  : TEXCOORD0;
};

VSOutput VS(float4 position : POSITION0)
{
    VSOutput output;

    output.Position = mul(position, mul(World, LightViewProjection));
    output.PositionWVP = output.Position;

    return output;
}

float4 BasicPS(VSOutput input) : COLOR0
{
    float depth = input.PositionWVP.z / input.PositionWVP.w;
    return float4(depth, 0, 0, 0);
}

float4 VariancePS(VSOutput input) : COLOR0
{
    float depth = input.PositionWVP.z / input.PositionWVP.w;
    return float4(depth, depth * depth, 0, 0);
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
