using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models
{
    internal class HUD
    {
        private SpriteFont _font;
        private int puntos = 0;
        private int multiplicador = 1;
        public HUD(ContentManager content)
        {
            _font = content.Load<SpriteFont>(MonoGaming.ContentFolderSpriteFonts + "GameFontBig");
        }

        public void Update(int puntos, int multiplicador)
        {
            this.puntos = puntos;
            this.multiplicador = multiplicador;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Dibuja el texto centrado
            Vector2 puntosSize = _font.MeasureString("Puntos: ");
            Vector2 puntosPosition = Vector2.Zero;

            spriteBatch.DrawString(_font, "Puntos: " + puntos, puntosPosition, Color.LightCyan);

            Vector2 multplicadorPosition = new Vector2(0, puntosSize.Y + 5);

            spriteBatch.DrawString(_font, "MULT  : " + multiplicador, multplicadorPosition, Color.LightCyan);

            spriteBatch.End();
        }
    }
}