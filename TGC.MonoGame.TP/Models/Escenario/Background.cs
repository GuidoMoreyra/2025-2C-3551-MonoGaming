using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models
{
    internal class Background
    {
        private Effect _effect;
        private Texture2D _texture;
        public Background(ContentManager content)
        {
            _effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShaderTexture").Clone();
            _texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Background/Starfield");
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            _effect.Parameters["View"].SetValue(Matrix.Identity);
            _effect.Parameters["Projection"].SetValue(Matrix.Identity);
            _effect.Parameters["World"].SetValue(Matrix.Identity);
            _effect.Parameters["Texture"].SetValue(_texture);

            graphicsDevice.DepthStencilState = DepthStencilState.None;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    Quad.GetVertices(),
                    0,
                    4,
                    Quad.GetIndices(),
                    0, 2
                );
            }

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}