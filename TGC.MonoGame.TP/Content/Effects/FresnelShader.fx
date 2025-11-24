#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

float4x4 World;
float4x4 ViewProjection;

float3 eyePosition; // Camera position

// Texturas
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

float Time = 0;

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Normal : NORMAL;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD1;
    float4 Normal : TEXCOORD2;
    float4 WorldPosition : TEXCOORD3;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
    // Model space to World space
    output.WorldPosition = mul(input.Position, World);
	// world space to Projection space
    output.Position = mul(output.WorldPosition, ViewProjection);

    output.TexCoord = input.TexCoord;
    output.Normal = input.Normal;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 fresnelColor = float3(1.0f, 0.576f, 0.0f);

    float3 normal = normalize(input.Normal.xyz);

    float4 texelColor = tex2D(s,input.TexCoord);


    float fresnel = dot(normal, normalize(eyePosition - input.WorldPosition.xyz));
    fresnel = saturate(1 - fresnel);

    float3 finalFresnel = fresnel * fresnelColor;

    float3 finalColor = texelColor.rgb + finalFresnel;

    return float4(finalColor, 1.0f);
}

//Fragment shader para que el bloom se aplique bien  (devuelve negro para que no se vea a traves del objeto)
float4 BloomPS(VertexShaderOutput input) : COLOR
{
    float3 fresnelColor = float3(1.0f, 0.576f, 0.0f);

    float3 normal = normalize(input.Normal.xyz);

    float4 texelColor = tex2D(s,input.TexCoord);


    float fresnel = dot(normal, normalize(eyePosition - input.WorldPosition.xyz));
    fresnel = saturate(1 - fresnel);

    float3 finalFresnel = fresnel * fresnelColor;

    float3 finalColor = texelColor.rgb + finalFresnel;

    float distanceToTargetColor = distance(finalColor, float3(1.0f, 0.576f, 0.0f));
    
    float filter = step(distanceToTargetColor, 0.30);
    
    return float4(finalColor * filter, 1.0f);
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
