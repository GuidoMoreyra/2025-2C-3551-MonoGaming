
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Modules.Contract;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Modules;

internal class PipeModule_2 : IModule
{
    private const TipoDeModulo tipo = TipoDeModulo.Pipe2;
    private const float MODEL_ROTATION = -90f;
    private const float SCALE = 0.2f;

    public bool IsOn { get; set; }
    public List<DestroyableBox> obstaclesD{ get; set; }
    private readonly Matrix _rotationScaleMatrix;
    private readonly Model _model;
    private Matrix _worldMatrix;

    //Medidas del Modulo

    public PipeModule_2()
    {
        _model = Pipe.GetModel();

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

                effect.Parameters["Time"]?.SetValue(elapsedTime * 1);
                effect.Parameters["Speed"]?.SetValue(new Vector2(0.1f, 0.1f));
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

    public TipoDeModulo GetTipoDeModulo(){
        return tipo;
    }

}