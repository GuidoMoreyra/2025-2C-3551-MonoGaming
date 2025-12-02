using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.BaseModels
{
    internal class BaseModelEscudo
    {
        private static Model _model = null;

        public static void InitializeModel(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _model = content.Load<Model>(MonoGaming.ContentFolder3D + "Esfera/Esfera");

            var effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "MovingTextureOpacityShader");

            // Clonar el shader para cada meshPart
            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect.Clone();
                    meshPart.Effect.Parameters["Texture"]?.SetValue(GeneradorTextura.CreateDiagonalStripeTexture(
                        graphicsDevice,
                        256,
                        Color.LightBlue,
                        Color.LightYellow,
                        8
                    ));
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