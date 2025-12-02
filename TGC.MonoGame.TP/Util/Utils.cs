using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Util;

internal class Utils
{
    private static readonly Random random = new();

    public static float GenerateNumber(float x)
    {
        return (float)((random.NextDouble() * 2 - 1) * x);
    }

    public static BoundingBox CalculateBoundingBox(Model model)
    {
        Vector3 min = new(float.MaxValue);
        Vector3 max = new(float.MinValue);

        foreach (var mesh in model.Meshes)
        {
            var meshTransform = mesh.ParentBone.Transform;
            foreach (var meshPart in mesh.MeshParts)
            {
                var vertexData = new VertexPositionNormalTexture[meshPart.NumVertices];
                meshPart.VertexBuffer.GetData(vertexData);

                foreach (var vertex in vertexData)
                {
                    var transformed = Vector3.Transform(vertex.Position, meshTransform);
                    min = Vector3.Min(min, transformed);
                    max = Vector3.Max(max, transformed);
                }
            }
        }

        return new BoundingBox(min, max);
    }
}