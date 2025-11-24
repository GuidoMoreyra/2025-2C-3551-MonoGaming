using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models
{

    internal class Menu
    {
        private const float SCALE = 0.1f;

        Model _model;
        List<RectangleButton> _buttons;
        MouseState _previousMouse;
        MouseState _currentMouse;
        SpriteBatch spriteBatch;
        public Menu(List<RectangleButton> buttons, SpriteBatch spriteBatch)
        {
            _model = Nave_1.GetModel();

            // Inicializa la lista de botones
            _buttons = buttons;
            this.spriteBatch = spriteBatch;
        }

        public void Update(GameTime gameTime)
        {
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
            foreach (var button in _buttons)
            {
                button.Update(_previousMouse, _currentMouse);
            }
        }

        public void Draw(Matrix viewProjection, Vector3 LightPosition, Vector3 CameraPosition)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var scaleMatrix = Matrix.CreateScale(SCALE);
                var world = meshWorld * scaleMatrix * Matrix.CreateFromYawPitchRoll(MathHelper.PiOver2,0,MathHelper.PiOver4);

                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.Parameters["ViewProjection"].SetValue(viewProjection);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
                    effect.Parameters["lightPosition"].SetValue(LightPosition);
                    effect.Parameters["eyePosition"]?.SetValue(CameraPosition);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                    }
                }
                mesh.Draw();
            }

            spriteBatch.Begin(blendState: BlendState.AlphaBlend); // ¡Importante activar AlphaBlend!
            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch);
            }

            spriteBatch.End();
        }
    }
}