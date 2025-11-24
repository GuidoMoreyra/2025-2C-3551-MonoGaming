using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models.BaseModels
{
    internal class Pasillo_Asteroide
    {
        private static Model _model = null;
        public static void InitializeModel(ContentManager content)
        {
            _model = content.Load<Model>(MonoGaming.ContentFolder3D + "Pasillo_Astetoroide/AteroidHallWay");
            var texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Asteroide_1/Asteroide_Tex");
            var effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShaderTexture");

            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var meshEffect = effect.Clone();
                    meshPart.Effect = meshEffect;
                    meshPart.Effect.Parameters["Texture"].SetValue(texture);

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