#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 ViewProjection;

// Textura
uniform Texture2D Texture;

// Sampler
sampler s = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Tiempo para animar la textura
float Time = 0;

// Velocidad de desplazamiento
float2 Speed = float2(0.5, 0.5); // velocidad en U y V

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(worldPosition, ViewProjection);

    // Desplaza la textura según el tiempo y la velocidad
    float2 offset = Time * Speed;
    output.TexCoord = input.TexCoord + offset;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return tex2D(s, input.TexCoord);
}

float4 BloomPS(VertexShaderOutput input) : COLOR
{
    return float4(0.0f,0.0f,0.0f, 1.0f);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

technique Bloom
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BloomPS();
    }
};
