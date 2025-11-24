using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Modules.Contract;
using TGC.MonoGame.TP.Models.Obstacles;

namespace TGC.MonoGame.TP.Models.Modules;

internal class CargoModule : IModule
{
    private const float AJUSTE_TRASLACION_Y = 6.5f;
    private const float AJUSTE_TRASLACION_Z = 18.5f;
    private const float SCALE = 0.04f;
    private const float MODEL_ROTATION = -90f;

    private readonly Matrix _rotationScaleMatrix;
    private readonly Model _model;
    private readonly List<CargoShip> _obstacles = [];
    private Matrix _worldMatrix;

    public CargoModule()
    {
        _model = Pasillo_Asteroide.GetModel();

        var ajusteDeTraslacion = Matrix.CreateTranslation(Vector3.Right * AJUSTE_TRASLACION_Z + Vector3.Down * AJUSTE_TRASLACION_Y);
        var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(MODEL_ROTATION));

        _rotationScaleMatrix = rotation * Matrix.CreateScale(SCALE) * ajusteDeTraslacion;

        _obstacles.Add(new CargoShip());
        _obstacles.Add(new CargoShip());
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        _worldMatrix = _rotationScaleMatrix * worldMatrix;

        GenerateObstacles(worldMatrix);
    }


    private void GenerateObstacles(Matrix worldMatrix)
    {
        var traslacion1 = Matrix.CreateTranslation(Vector3.Left * 15f);
        var traslacion2 = Matrix.CreateTranslation(Vector3.Right * 15f);

        _obstacles[0].SetWorldMatrix(worldMatrix * traslacion1);
        _obstacles[1].SetWorldMatrix(worldMatrix * traslacion2);
    }


    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime)
    {
        foreach (var mesh in _model.Meshes)
        {
            var meshWorld = mesh.ParentBone.Transform;
            var world = meshWorld * _worldMatrix;
            foreach (var meshPart in mesh.MeshParts)
            {
                var effect = meshPart.Effect;
                effect.Parameters["ViewProjection"].SetValue(viewProjection);
                effect.Parameters["World"].SetValue(world);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }
            mesh.Draw();
        }
        foreach (CargoShip ship in _obstacles)
        {
            ship.Draw(viewProjection, cameraPosition);
        }
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
        foreach (var obstacle in _obstacles)
            obstacle.Update(gameTime, player);

    }

    public void DrawBloom(Matrix viewProjection)
    {
        foreach (CargoShip ship in _obstacles)
        {
            ship.DrawBloom(viewProjection);
        }
    }

    public static int GetModuleNumber()
    {
        return 2;
    }
}