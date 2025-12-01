using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Models.UserInterface
{
    internal class RectangleButton
    {
        private readonly string _text;
        private readonly Rectangle _rectangle;
        private readonly SpriteFont _font;
        private readonly Texture2D _texture;
        private readonly SpriteBatch _spriteBatch;
        private readonly Action _onClick;

        public RectangleButton(ContentManager content, string text, Rectangle rectangle, SpriteBatch spriteBatch, Action onClick)
        {
            _texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Buttons/RectangleButton");
            _font = content.Load<SpriteFont>(MonoGaming.ContentFolderSpriteFonts + "GameFont");
            _text = text;
            _rectangle = rectangle;
            _spriteBatch = spriteBatch;
            _onClick = onClick;
        }

        public void Update(MouseState previousMouse, MouseState currentMouse)
        {
            if (_rectangle.Contains(currentMouse.Position))
            {
                if (currentMouse.LeftButton == ButtonState.Pressed &&
                    previousMouse.LeftButton == ButtonState.Released)
                {
                    // Si hay una acción asignada, la ejecutamos
                    _onClick?.Invoke();
                }
            }
        }

        public void Draw()
        {
            _spriteBatch.Draw(_texture, _rectangle, Color.White * 0.8f); // Fondo con 80% opacidad

            // Dibuja el texto centrado
            Vector2 textSize = _font.MeasureString(_text);
            Vector2 textPosition = new(
                _rectangle.X + (_rectangle.Width / 2) - (textSize.X / 2),
                _rectangle.Y + (_rectangle.Height / 2) - (textSize.Y / 2)
            );

            _spriteBatch.DrawString(_font, _text, textPosition, Color.Black);
        }
    }
}