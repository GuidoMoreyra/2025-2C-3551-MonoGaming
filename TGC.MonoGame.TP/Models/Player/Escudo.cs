
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;

namespace TGC.MonoGame.TP.Models.Player
{
    internal class Escudo
    {
        private const float SCALE = 5.0f;
        private const float OPACITY = 0.3f;
        private const float SPEED = 0.2f;

        private readonly Model _model;
        private readonly Matrix _scaleMatrix;
        private readonly GraphicsDevice _graphicsDevice;

        public Escudo(GraphicsDevice graphicsDevice)
        {
            _scaleMatrix = Matrix.CreateScale(SCALE);

            _model = BaseModelEscudo.GetModel();

            _graphicsDevice = graphicsDevice;
        }

        public void Draw(Matrix viewProjection, Matrix mundo, GameTime gameTime)
        {
            _graphicsDevice.BlendState = BlendState.NonPremultiplied;
            _graphicsDevice.DepthStencilState = DepthStencilState.None;

            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.Parameters["World"].SetValue(_scaleMatrix * mundo);
                    effect.Parameters["ViewProjection"].SetValue(viewProjection);
                    effect.Parameters["Opacity"]?.SetValue(OPACITY);

                    effect.Parameters["Time"]?.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
                    effect.Parameters["Speed"]?.SetValue(SPEED);
                }
                mesh.Draw();
            }

            // Restaurar estados para no afectar otros objetos
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
