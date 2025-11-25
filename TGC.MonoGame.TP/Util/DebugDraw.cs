using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class DebugDraw
{
    public static GraphicsDevice GraphicsDevice { get; set; }

    public static void DrawOBB(OrientedBoundingBox obb, Matrix viewProjection, Color color)
    {
        // Creamos un BasicEffect local
        using (BasicEffect effect = new BasicEffect(GraphicsDevice))
        {
            effect.VertexColorEnabled = true;
            effect.View = Matrix.Identity;
            effect.Projection = Matrix.Identity;
            effect.World = viewProjection;

            // Calculamos las 8 esquinas de la OBB
            Vector3 hx = obb.AxisX * obb.HalfExtents.X;
            Vector3 hy = obb.AxisY * obb.HalfExtents.Y;
            Vector3 hz = obb.AxisZ * obb.HalfExtents.Z;

            Vector3[] corners = new Vector3[8];
            corners[0] = obb.Center + hx + hy + hz;
            corners[1] = obb.Center + hx + hy - hz;
            corners[2] = obb.Center + hx - hy + hz;
            corners[3] = obb.Center + hx - hy - hz;
            corners[4] = obb.Center - hx + hy + hz;
            corners[5] = obb.Center - hx + hy - hz;
            corners[6] = obb.Center - hx - hy + hz;
            corners[7] = obb.Center - hx - hy - hz;

            // Vertex con color
            VertexPositionColor[] verts = new VertexPositionColor[8];
            for (int i = 0; i < 8; i++)
                verts[i] = new VertexPositionColor(corners[i], color);

            // Indices para lineas
            int[] indices = new int[]
            {
                0,1, 0,2, 0,4,
                1,3, 1,5,
                2,3, 2,6,
                3,7,
                4,5, 4,6,
                5,7,
                6,7
            };

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList,
                    verts,
                    0,
                    8,
                    indices,
                    0,
                    indices.Length / 2
                );
            }
        }
    }
}
