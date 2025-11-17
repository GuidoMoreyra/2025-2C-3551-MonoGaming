using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models.BaseModels
{
    internal class Nave_1
    {
        private static Model _model = null;
        public static Model GetModel(ContentManager content)
        {
            if (_model == null)
            {
                _model = content.Load<Model>(MonoGaming.ContentFolder3D + "Nave_1/Nave_1");
                var texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Nave_1/Nave1_Diff");
                var effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BlinnPhong");
                var normalMap = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Nave_1/Nave1_Normal");

                foreach (var mesh in _model.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        var meshEffect = effect.Clone();
                        meshPart.Effect = meshEffect;
                        meshPart.Effect.Parameters["Texture"].SetValue(texture);
                        meshPart.Effect.Parameters["NormalTexture"].SetValue(normalMap);
                        meshPart.Effect.Parameters["ambientColor"].SetValue(MonoGaming.LightAmbientColor.ToVector3());
                        meshPart.Effect.Parameters["diffuseColor"].SetValue(MonoGaming.LightDiffuseColor.ToVector3());
                        meshPart.Effect.Parameters["specularColor"]?.SetValue(MonoGaming.LightSpecularColor.ToVector3());
                        meshPart.Effect.Parameters["KAmbient"].SetValue(0.1f);
                        meshPart.Effect.Parameters["KDiffuse"].SetValue(1.0f);
                        meshPart.Effect.Parameters["KSpecular"]?.SetValue(0.4f);
                        meshPart.Effect.Parameters["shininess"]?.SetValue(32.0f);

                    }
                }

            }

            return _model;
        }
    }
}