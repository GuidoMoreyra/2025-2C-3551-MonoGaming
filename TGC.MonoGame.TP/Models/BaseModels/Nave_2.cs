using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models.BaseModels
{
    internal class Nave_2
    {
        private static Model _model = null;
        public static Model GetModel(ContentManager content)
        {
            if (_model == null)
            {
                _model = content.Load<Model>(MonoGaming.ContentFolder3D + "Nave_2/Nave_2");
                var texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Nave_2/Nave2_Tex");
                var effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "FresnelShader");

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

            return _model;
        }
    }
}