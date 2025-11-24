using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Models.BaseModels;

namespace TGC.MonoGame.TP.Models
{

    internal class Menu
    {
        private const float SCALE = 0.1f;
        private const float YAW = MathHelper.PiOver2;
        private const float PITCH = 0;
        private const float ROLL = MathHelper.PiOver4;

        private readonly Model _model;
        private readonly Matrix _rotationScaleMatrix;
        private readonly List<RectangleButton> _buttons;
        private readonly SpriteBatch _spriteBatch;
        private MouseState _previousMouse;
        private MouseState _currentMouse;
        public Menu(List<RectangleButton> buttons, SpriteBatch spriteBatch)
        {
            _model = Nave_1.GetModel();

            // Inicializa la lista de botones
            _buttons = buttons;
            _spriteBatch = spriteBatch;

            _rotationScaleMatrix = Matrix.CreateScale(SCALE) * Matrix.CreateFromYawPitchRoll(YAW, PITCH, ROLL);
        }

        public void Update()
        {
            _previousMouse = _currentMouse;
            _currentMouse = Mouse.GetState();
            foreach (var button in _buttons)
            {
                button.Update(_previousMouse, _currentMouse);
            }
        }

        public void Draw(Matrix viewProjection, Vector3 lightPosition, Vector3 cameraPosition)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var world = meshWorld * _rotationScaleMatrix;
                var transposeInvertWorld = Matrix.Transpose(Matrix.Invert(world));

                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.Parameters["ViewProjection"].SetValue(viewProjection);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["InverseTransposeWorld"].SetValue(transposeInvertWorld);
                    effect.Parameters["lightPosition"].SetValue(lightPosition);
                    effect.Parameters["eyePosition"]?.SetValue(cameraPosition);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                    }
                }
                mesh.Draw();
            }

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            foreach (var button in _buttons)
            {
                button.Draw();
            }

            _spriteBatch.End();
        }
    }
}