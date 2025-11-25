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
        BoundingBox mergedBox = new();
        bool first = true;

        foreach (var mesh in model.Meshes)
        {
            Matrix meshTransform = mesh.ParentBone.Transform;

            Vector3 min = mesh.BoundingSphere.Center - new Vector3(mesh.BoundingSphere.Radius);
            Vector3 max = mesh.BoundingSphere.Center + new Vector3(mesh.BoundingSphere.Radius);
            BoundingBox meshBox = new(min, max);

            if (first)
            {
                mergedBox = meshBox;
                first = false;
            }
            else
            {
                mergedBox = BoundingBox.CreateMerged(mergedBox, meshBox);
            }
        }
        return mergedBox;
    }
}