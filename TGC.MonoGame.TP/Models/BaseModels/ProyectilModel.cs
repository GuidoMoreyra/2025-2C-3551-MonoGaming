using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models.BaseModels
{
    internal class ProyectilModel
    {
        // Dimensiones predeterminadas para un cubo
        private const float D = 0.5f;

        public static BoundingBox GetBoundingBox()
        {
            return new BoundingBox(new Vector3(-D, -D, -D), new Vector3(D, D, D));
        }

        public static VertexPositionColor[] GetVertices(Color color)
        {
            return
            [
            // Front face
            new VertexPositionColor(new Vector3(-D, D, D), color),       // 0 - Top Left Front
            new VertexPositionColor(new Vector3(D, D, D), color),        // 1 - Top Right Front
            new VertexPositionColor(new Vector3(-D, -D, D), color),      // 2 - Bottom Left Front
            new VertexPositionColor(new Vector3(D, -D, D), color),       // 3 - Bottom Right Front
            
            // Back face
            new VertexPositionColor(new Vector3(-D, D, -D), color),      // 4 - Top Left Back
            new VertexPositionColor(new Vector3(D, D, -D), color),       // 5 - Top Right Back
            new VertexPositionColor(new Vector3(-D, -D, -D), color),     // 6 - Bottom Left Back
            new VertexPositionColor(new Vector3(D, -D, -D), color),      // 7 - Bottom Right Back
            ];
        }
        public static short[] GetIndices()
        {
            return
            [
                // Frente (0, 1, 2, 3)
                0,1,2,
                1,3,2, 

                // Atrás (4, 5, 6, 7)
                5,4,7,
                4,6,7,

                // Izquierda (4, 0, 6, 2)
                4,0,6,
                0,2,6,

                // Derecha (1, 5, 3, 7)
                1,5,3,
                5,7,3,

                // Arriba (4, 5, 0, 1)
                4,5,0,
                5,1,0,

                // Abajo (2, 3, 6, 7)
                2,3,6,
                3,7,6
            ];
        }
    }
}