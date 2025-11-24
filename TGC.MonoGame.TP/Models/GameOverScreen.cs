using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models
{
    internal class GameOverScreen
    {
        private readonly Effect _effect;
        private readonly SpriteFont _font;
        private readonly List<RectangleButton> _pauseButtons;
        private readonly SpriteBatch _spriteBatch;
        private readonly Vector2 _centroPantalla;
        private readonly GraphicsDevice _graphicsDevice;
        private MouseState _previousMouse;
        private MouseState _currentMouse;
        private int puntos;
        
        public GameOverScreen(ContentManager content, List<RectangleButton> buttons, SpriteBatch spriteBatch, Vector2 centroPantalla, GraphicsDevice graphicsDevice)
        {
            puntos = 0;
            _centroPantalla = centroPantalla;
            _font = content.Load<SpriteFont>(MonoGaming.ContentFolderSpriteFonts + "GameFont");

            _effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShader").Clone();
            _effect.Parameters["ViewProjection"].SetValue(Matrix.Identity);
            _effect.Parameters["World"].SetValue(Matrix.Identity);
            _effect.Parameters["DiffuseColor"].SetValue(new Vector4(0, 0, 0, 0.3f));

            // Inicializa la lista de botones
            _pauseButtons = buttons;
            _spriteBatch = spriteBatch;
            _graphicsDevice = graphicsDevice;
        }

        public void SetPuntos(int puntos)
        {
            this.puntos = puntos;
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

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend);

            Vector2 puntosPosicion = new(_centroPantalla.X - (_font.MeasureString("Puntos: ").X / 2), _centroPantalla.Y / 2);
            _spriteBatch.DrawString(_font, "Puntos: " + puntos, puntosPosicion, Color.Red);

            foreach (var button in _pauseButtons)
            {
                button.Draw();
            }

            _spriteBatch.End();

            _graphicsDevice.BlendState = BlendState.Opaque;
        }
    }
}