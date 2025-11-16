using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


//Clase para ver el wireframe de una bounding sphere
public static class BoundingSphereRenderer
{
    // Puedes ajustar el número de segmentos para una esfera más o menos suave
    private const int Segments = 16;
    private static VertexPositionColor[] verts;
    private static BasicEffect effect;
    private static GraphicsDevice graphicsDevice;

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        BoundingSphereRenderer.graphicsDevice = graphicsDevice;
        // 1. Inicializar el BasicEffect para dibujar líneas
        effect = new BasicEffect(graphicsDevice);
        effect.VertexColorEnabled = true; // Habilitar colores de vértice

        // 2. Generar la geometría para 3 círculos (XY, XZ, YZ)
        verts = new VertexPositionColor[Segments * 3 * 2]; // 3 círculos, Segments líneas por círculo, 2 vértices por línea
        GenerateSphereGeometry();
    }

    private static void GenerateSphereGeometry()
    {
        // Esto solo crea las coordenadas de los círculos en el espacio modelo (radio 1.0)
        // La matriz de mundo manejará el posicionamiento y el escalado
        int index = 0;
        float step = MathHelper.TwoPi / Segments;

        // Círculo XY
        for (int i = 0; i < Segments; i++)
        {
            float angle1 = i * step;
            float angle2 = (i + 1) * step;

            verts[index++] = new VertexPositionColor(new Vector3((float)Math.Cos(angle1), (float)Math.Sin(angle1), 0), Color.White);
            verts[index++] = new VertexPositionColor(new Vector3((float)Math.Cos(angle2), (float)Math.Sin(angle2), 0), Color.White);
        }

        // Círculo XZ (Rota 90 grados alrededor del eje X)
        for (int i = 0; i < Segments; i++)
        {
            float angle1 = i * step;
            float angle2 = (i + 1) * step;

            verts[index++] = new VertexPositionColor(new Vector3((float)Math.Cos(angle1), 0, (float)Math.Sin(angle1)), Color.White);
            verts[index++] = new VertexPositionColor(new Vector3((float)Math.Cos(angle2), 0, (float)Math.Sin(angle2)), Color.White);
        }

        // Círculo YZ (Rota 90 grados alrededor del eje Y)
        for (int i = 0; i < Segments; i++)
        {
            float angle1 = i * step;
            float angle2 = (i + 1) * step;

            verts[index++] = new VertexPositionColor(new Vector3(0, (float)Math.Cos(angle1), (float)Math.Sin(angle1)), Color.White);
            verts[index++] = new VertexPositionColor(new Vector3(0, (float)Math.Cos(angle2), (float)Math.Sin(angle2)), Color.White);
        }
    }
    public static void Draw(BoundingSphere sphere, Matrix view, Matrix projection, Color color)
    {
        // 1. Crear la Matriz de Mundo:
        // La geometría se generó en el origen con radio 1.0. 
        // Ahora la escalamos por el radio y la trasladamos al centro de la esfera.
        Matrix worldMatrix = Matrix.CreateScale(sphere.Radius) * Matrix.CreateTranslation(sphere.Center);

        // 2. Configurar el BasicEffect:
        effect.World = worldMatrix;
        effect.View = view;
        effect.Projection = projection;

        effect.DiffuseColor = color.ToVector3(); // Usar el color difuso del BasicEffect

        // 3. Dibujar la geometría:
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawUserPrimitives(
                PrimitiveType.LineList, // Usar lista de líneas
                verts,                   // Array de vértices
                0,                       // Offset
                Segments * 3             // Número de primitivas (líneas)
            );
        }
    }
}