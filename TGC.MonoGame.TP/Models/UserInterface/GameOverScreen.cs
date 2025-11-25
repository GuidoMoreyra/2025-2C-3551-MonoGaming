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
        private Effect _effect;
        private SpriteFont _font;
        List<RectangleButton> _pauseButtons;
        MouseState _previousMouse;
        MouseState _currentMouse;
        SpriteBatch spriteBatch;
        int puntos;
        Vector2 centroPantalla;
        public GameOverScreen(ContentManager content, List<RectangleButton> buttons, SpriteBatch spriteBatch, int puntos, Vector2 centroPantalla)
        {
            this.puntos = puntos;
            this.centroPantalla = centroPantalla;
            _effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShader").Clone();
            _font = content.Load<SpriteFont>(MonoGaming.ContentFolderSpriteFonts + "GameFont");

            // Inicializa la lista de botones
            _pauseButtons = buttons;
            this.spriteBatch = spriteBatch;
        }

        public void setPuntos(int puntos)
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

        public void Draw(GraphicsDevice graphicsDevice)
        {
            _effect.Parameters["View"].SetValue(Matrix.Identity);
            _effect.Parameters["Projection"].SetValue(Matrix.Identity);
            _effect.Parameters["World"].SetValue(Matrix.Identity);
            _effect.Parameters["DiffuseColor"].SetValue(new Vector4(0, 0, 0, 0.3f));

            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.BlendState = BlendState.AlphaBlend;

            // 4. Dibujar el Quad
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

            // 5. Restaurar matrices y Z-Buffer para la escena 3D
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            spriteBatch.Begin(blendState: BlendState.AlphaBlend); // ¡Importante activar AlphaBlend!

            Vector2 puntosPosicion = new Vector2(centroPantalla.X - (_font.MeasureString("Puntos: ").X / 2), centroPantalla.Y / 2);
            spriteBatch.DrawString(_font, "Puntos: " + puntos, puntosPosicion, Color.Red);

            foreach (var button in _pauseButtons)
            {
                button.Draw(spriteBatch);
            }

            spriteBatch.End();
        }
    }
}