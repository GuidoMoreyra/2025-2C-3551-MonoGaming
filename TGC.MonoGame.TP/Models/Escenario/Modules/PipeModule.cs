using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Escenario.Contract;
using TGC.MonoGame.TP.Models.Player;

namespace TGC.MonoGame.TP.Models.Escenario.Modules;

internal class PipeModule : IModule
{
    private const TipoDeModulo tipo = TipoDeModulo.Pipe;
    private const float MODEL_ROTATION = -90f;
    private const float SCALE = 0.2f;
    private const float SPEED = 0.1f;

    private readonly Matrix _rotationScaleMatrix;
    private readonly Model _model;
    private readonly Vector2 _textureSpeed;
    public bool IsOn { get; set; }
    private Matrix _worldMatrix;

    public PipeModule()
    {
        _model = Pipe.GetModel();

        _textureSpeed = new Vector2(SPEED, SPEED);

        _rotationScaleMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(MODEL_ROTATION)) * Matrix.CreateScale(SCALE);
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        _worldMatrix = _rotationScaleMatrix * worldMatrix;
    }

    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime, GraphicsDevice _graphicsDevice)
    {
        foreach (var mesh in _model.Meshes)
        {
            var meshWorld = mesh.ParentBone.Transform;
            var world = meshWorld * _worldMatrix;

            foreach (var meshPart in mesh.MeshParts)
            {
                var effect = meshPart.Effect;

                effect.CurrentTechnique = effect.Techniques["BasicColorDrawing"];
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["ViewProjection"].SetValue(viewProjection);

                effect.Parameters["Time"]?.SetValue(elapsedTime);
                effect.Parameters["Speed"]?.SetValue(_textureSpeed);
            }
            mesh.Draw();
        }
    }

    public void DrawBloom(Matrix viewProjection)
    {
        foreach (var mesh in _model.Meshes)
        {
            var meshWorld = mesh.ParentBone.Transform;
            var world = meshWorld * _worldMatrix;

            foreach (var meshPart in mesh.MeshParts)
            {
                var effect = meshPart.Effect;
                effect.CurrentTechnique = effect.Techniques["Bloom"];
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["ViewProjection"].SetValue(viewProjection);

            }
            mesh.Draw();
        }
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
    }

    public TipoDeModulo GetTipoDeModulo()
    {
        return tipo;
    }

}