#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// ====================================================================
// MATRICES UNIFORMES
// ====================================================================

float4x4 World;
float4x4 ViewProjection;

// ====================================================================
// PARÁMETROS DE TEXTURA Y OPACIDAD
// ====================================================================

// Textura base del modelo
uniform Texture2D Texture; 

// Valor de opacidad (Alfa) pasado desde C# [0.0 (Invisible) a 1.0 (Opaco)]
// Este valor debe ser calculado en el método Draw de C# basado en la distancia a la cámara.
float Opacity; 

// Sampler para muestrear la textura
sampler s = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// ====================================================================
// ESTRUCTURAS DE SHADER
// ====================================================================

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0; // Coordenadas de textura
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD1;
};

// ====================================================================
// SHADER DE VÉRTICE (VS)
// ====================================================================

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    
    // Transformación Model -> World
    float4 worldPosition = mul(input.Position, World);
    
    // Transformación World -> Projection (Clip Space)
    output.Position = mul(worldPosition, ViewProjection);

    // Pasar coordenadas de textura al Pixel Shader
    output.TexCoord = input.TexCoord;

    return output;
}

// ====================================================================
// SHADER DE PÍXEL (PS) para renderizado normal
// ====================================================================

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Muestrear el color de la textura
    float4 textureColor = tex2D(s, input.TexCoord);

    // Retornar el color de la textura, pero usando el parámetro Opacity como el canal Alfa.
    // Esto hace que el modelo se vuelva transparente según el valor de Opacity.
    return float4(textureColor.rgb, Opacity);
}

// ====================================================================
// SHADER DE PÍXEL (PS) para el Bloom
// ====================================================================

// El Bloom debe ser totalmente opaco (1.0f) y negro (0,0,0) para que
// no afecte el color base del Bloom, que se calcula en una pasada posterior.
float4 BloomPS(VertexShaderOutput input) : COLOR
{
    return float4(0.0f, 0.0f, 0.0f, 1.0f);
}

// ====================================================================
// TÉCNICAS
// ====================================================================

// Técnica principal para dibujar el modelo con textura y opacidad
technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

// Técnica para dibujar el mapa de Bloom (normalmente, solo objetos brillantes o negro total)
technique Bloom
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BloomPS();
    }
};