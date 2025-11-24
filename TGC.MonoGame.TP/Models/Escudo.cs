
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models.Modules
{
    internal class Escudo
    {
        private Matrix _worldMatrix;
        private Model _model;
        private float scale = 5f;

        public float Alpha { get; set; } = 0.3f; // inicializado en blanco, opaco

        public Escudo(ContentManager content, Matrix worldMatrix)
        {
            _model = content.Load<Model>(MonoGaming.ContentFolder3D + "Esfera/Esfera");
            var effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShader").Clone();

            // Clonar el shader para cada meshPart
            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }

            _worldMatrix = worldMatrix;
        }

       public void Draw(Matrix viewProjection, Matrix mundo, GraphicsDevice graphicsDevice)
        {
        graphicsDevice.BlendState = BlendState.NonPremultiplied;
        graphicsDevice.DepthStencilState = new DepthStencilState()
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false
        };

            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.Parameters["World"].SetValue(Matrix.CreateScale(scale) * mundo);
                    effect.Parameters["ViewProjection"].SetValue(viewProjection);
                    effect.Parameters["DiffuseColor"]?.SetValue(new Vector4(Color.White.ToVector3(), Alpha));
                }
                mesh.Draw();
            }

            // Restaurar estados para no afectar otros objetos
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }


    }
}
