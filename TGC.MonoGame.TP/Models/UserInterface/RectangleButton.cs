using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models
{
    internal class RectangleButton
    {
        private Rectangle _rectangle;
        private string _text;
        private SpriteFont _font;
        private Texture2D _texture;

        private Color _hoverColor = Color.Yellow; // Color al pasar el mouse

        public Action OnClick;

        public RectangleButton(ContentManager content, string text, Rectangle rectangle)
        {
            _texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Buttons/RectangleButton");
            _font = content.Load<SpriteFont>(MonoGaming.ContentFolderSpriteFonts + "GameFont");
            _text = text;
            _rectangle = rectangle;
        }

        public void Update(MouseState previousMouse, MouseState currentMouse)
        {
            // 1. Comprobar si el mouse está sobre el botón
            if (_rectangle.Contains(currentMouse.Position))
            {


                // 2. Comprobar si se hizo clic (presionado Y soltado sobre el botón)
                if (currentMouse.LeftButton == ButtonState.Pressed &&
                    previousMouse.LeftButton == ButtonState.Released)
                {
                    // Si hay una acción asignada, la ejecutamos
                    OnClick?.Invoke();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _rectangle, Color.White * 0.8f); // Fondo con 80% opacidad

            // Dibuja el texto centrado
            Vector2 textSize = _font.MeasureString(_text);
            Vector2 textPosition = new Vector2(
                _rectangle.X + (_rectangle.Width / 2) - (textSize.X / 2),
                _rectangle.Y + (_rectangle.Height / 2) - (textSize.Y / 2)
            );

            spriteBatch.DrawString(_font, _text, textPosition, Color.Black);
        }
    }
}