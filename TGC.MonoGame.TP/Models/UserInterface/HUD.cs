using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models
{
    internal class HUD
    {
        private readonly SpriteFont _font;
        private readonly SpriteBatch _spriteBatch;
        private readonly Vector2 _puntosPosition;
        private readonly Vector2 _multplicadorPosition;
        private int puntos;
        private int multiplicador;
        public HUD(ContentManager content, SpriteBatch spriteBatch)
        {
            puntos = 0;
            multiplicador = 1;

            _font = content.Load<SpriteFont>(MonoGaming.ContentFolderSpriteFonts + "GameFontBig");

            Vector2 puntosSize = _font.MeasureString("Puntos: ");
            _puntosPosition = Vector2.Zero;
            _multplicadorPosition = new Vector2(0, puntosSize.Y + 5);

            _spriteBatch = spriteBatch;
        }

        public void Update(int puntos, int multiplicador)
        {
            this.puntos = puntos;
            this.multiplicador = multiplicador;
        }

        public void Draw()
        {
            _spriteBatch.Begin();

            _spriteBatch.DrawString(_font, "Puntos: " + puntos, _puntosPosition, Color.LightCyan);

            _spriteBatch.DrawString(_font, "Mult: " + multiplicador, _multplicadorPosition, Color.LightCyan);

            _spriteBatch.End();
        }
    }
}