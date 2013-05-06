#define MAX_RADIUS 7
#define MAX_KERNEL_SIZE (MAX_RADIUS * 2 + 1)

float KernelSize;
// xy = offset
// z  = weight
float3 Kernels[MAX_KERNEL_SIZE];

texture Texture : register(t0);
sampler TextureSampler : register(s0) = sampler_state
{
    Texture = <Texture>;
};

float4 PS(float4 color    : COLOR0,
          float2 texCoord : TEXCOORD0) : SV_Target0
{
    float4 c = 0;
    for (int i = 0; i < KernelSize; i++)
    {
        float3 kernel = Kernels[i];
        float2 offset = kernel.xy;
        float weight = kernel.z;
        c += tex2D(TextureSampler, texCoord + offset) * weight;
    }
    return c;
}

technique Default
{
    pass Default
    {
        PixelShader = compile ps_2_0 PS();
    }
}
