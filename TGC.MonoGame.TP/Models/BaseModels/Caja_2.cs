using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models.BaseModels
{
    internal class Caja_2
    {
        private static Model _model = null;

        public static void InitializeModel(ContentManager content)
        {
            _model = content.Load<Model>(MonoGaming.ContentFolder3D + "Caja_2/Caja_2");

            var texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Caja_2/Caja2_Diana");

            var effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "TextureOpacityShader");

            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var meshEffect = effect.Clone();
                    meshPart.Effect = meshEffect;

                    meshPart.Effect.Parameters["Texture"]?.SetValue(texture);
                    meshPart.Effect.Parameters["Opacity"]?.SetValue(1.0f);
                }
            }
        }

        public static Model GetModel()
        {
            if (_model == null)
            {
                throw new InvalidOperationException("No se inicializo el modelo");
            }
            return _model;
        }
    }
}