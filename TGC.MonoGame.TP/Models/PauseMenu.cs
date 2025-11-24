using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models
{
    internal class PauseMenu
    {
        private readonly Effect _effect;
        private readonly List<RectangleButton> _pauseButtons;
        private readonly SpriteBatch _spriteBatch;
        private readonly GraphicsDevice _graphicsDevice;
        private MouseState _previousMouse;
        private MouseState _currentMouse;
        public PauseMenu(ContentManager content, List<RectangleButton> buttons, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            _effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShader").Clone();
            _effect.Parameters["ViewProjection"].SetValue(Matrix.Identity);
            _effect.Parameters["World"].SetValue(Matrix.Identity);
            _effect.Parameters["DiffuseColor"].SetValue(new Vector4(0, 0, 0, 0.3f));

            // Inicializa la lista de botones
            _pauseButtons = buttons;
            _spriteBatch = spriteBatch;
            _graphicsDevice = graphicsDevice;
        }

        public void Update()
        {
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
            foreach (var button in _pauseButtons)
            {
                button.Update(_previousMouse, _currentMouse);
            }
        }

        public void Draw()
        {
            _graphicsDevice.BlendState = BlendState.AlphaBlend;

            Quad.Draw(_effect, _graphicsDevice);

           _graphicsDevice.DepthStencilState = DepthStencilState.Default;

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            foreach (var button in _pauseButtons)
            {
                button.Draw();
            }

            _spriteBatch.End();

            _graphicsDevice.BlendState = BlendState.Opaque;
        }
    }
}