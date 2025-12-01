using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Escenario
{
    internal class Background
    {
        private readonly Effect _effect;
        private readonly Texture2D _texture;
        private readonly GraphicsDevice _graphicsDevice;
        public Background(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Background/Starfield");

            _effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShaderTexture").Clone();

            _effect.Parameters["ViewProjection"].SetValue(Matrix.Identity);
            _effect.Parameters["World"].SetValue(Matrix.Identity);
            _effect.Parameters["Texture"].SetValue(_texture);

            _graphicsDevice = graphicsDevice;

        }

        public void Draw()
        {
            Quad.Draw(_effect, _graphicsDevice);
        }
    }
}