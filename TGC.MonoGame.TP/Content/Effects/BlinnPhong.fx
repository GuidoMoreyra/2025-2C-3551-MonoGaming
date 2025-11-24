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

float4x4 InverseTransposeWorld;

float3 ambientColor; // Light's Ambient Color
float3 diffuseColor; // Light's Diffuse Color
float3 specularColor; // Light's Specular Color
float KAmbient; 
float KDiffuse; 
float KSpecular;
float shininess; 
float3 lightPosition;
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

//Textura para Normals
texture NormalTexture;
sampler2D normalSampler = sampler_state
{
    Texture = (NormalTexture);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

float3 getNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
    float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

    float3 Q1 = ddx(worldPosition);
    float3 Q2 = ddy(worldPosition);
    float2 st1 = ddx(textureCoordinates);
    float2 st2 = ddy(textureCoordinates);

    worldNormal = normalize(worldNormal.xyz);
    float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
    float3 B = -normalize(cross(worldNormal, T));
    float3x3 TBN = float3x3(T, B, worldNormal);

    return normalize(mul(tangentNormal, TBN));
}

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

    output.Normal = mul(float4(normalize(input.Normal.xyz), 1.0), InverseTransposeWorld);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Base vectors
    float3 lightDirection = normalize(lightPosition - input.WorldPosition.xyz);
    float3 viewDirection = normalize(eyePosition - input.WorldPosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
    float3 normal = getNormalFromMap(input.TexCoord, input.WorldPosition.xyz, normalize(input.Normal.xyz));

	// Get the texture texel
    float4 texelColor = tex2D(s,input.TexCoord);
    
	// Calculate the diffuse light
    float NdotL = saturate(dot(normal, lightDirection));
    float halfLambert = NdotL * 0.5 + 0.5;          // lleva el rango a [0.5,1]
    float3 diffuseLight = KDiffuse * diffuseColor * halfLambert;

	// Calculate the specular light
    float NdotH = dot(normal, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * specularColor * pow(saturate(NdotH), shininess);
    
    // Final calculation
    float4 finalColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);
    return finalColor;
}

float4 BloomPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(s, input.TexCoord);
    
    //El targetColor es el azul de la cabina
    float distanceToTargetColor = distance(color.rgb, float3(0.11f, 0.192f, 0.333f));
    
    float filter = step(distanceToTargetColor, 0.15);
    
    return float4(color.rgb * filter, 1.0f);
}

technique MainTechnique
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
