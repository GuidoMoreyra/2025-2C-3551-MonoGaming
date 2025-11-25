using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Models.Modules.Contract;
using TGC.MonoGame.TP.Models.Obstacles;


namespace TGC.MonoGame.TP.Models.Modules;

internal class ShipModule_1 : IModule
{
    private const float SCALE = 0.1f;
    private const TipoDeModulo TIPO = TipoDeModulo.Ship1;

    private readonly Model _model;
    private readonly List<Ship> _dinamicObstacles;
    private readonly CargoShip _staticObstacle;
    private readonly Matrix _rotationScaleMatrix;
    private readonly Matrix _ship2Matrix;
    private readonly Matrix _cargoShipMatrix;
    public bool IsOn { get; set; }
    private Matrix _worldMatrix;

    public ShipModule_1()
    {
        IsOn = false;

        _model = Pasillo.GetModel();

        _rotationScaleMatrix = Matrix.CreateScale(SCALE);

        _ship2Matrix = Matrix.CreateTranslation(Vector3.Forward * 15f + Vector3.Up * 10f);
        _cargoShipMatrix = Matrix.CreateRotationX(MathHelper.ToDegrees(60)) * Matrix.CreateTranslation(Vector3.Backward * 10f + Vector3.Up * 10f);

        _dinamicObstacles = [];
        _dinamicObstacles.Add(new Ship());
        _dinamicObstacles.Add(new Ship());

        _staticObstacle = new CargoShip();
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        IsOn = false;

        _worldMatrix = _rotationScaleMatrix * worldMatrix;

        GenerateObstacles(worldMatrix);
    }

    private void GenerateObstacles(Matrix worldMatrix)
    {
        _dinamicObstacles[0].SetWorldMatrix(worldMatrix);
        _dinamicObstacles[1].SetWorldMatrix(_ship2Matrix * worldMatrix);

        _staticObstacle.SetWorldMatrix(_cargoShipMatrix * worldMatrix);
    }

    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime)
    {
        // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
        foreach (var mesh in _model.Meshes)
        {
            var meshWorld = mesh.ParentBone.Transform;
            var world = meshWorld * _worldMatrix;
            foreach (var meshPart in mesh.MeshParts)
            {
                var effect = meshPart.Effect;
                effect.CurrentTechnique = effect.Techniques["BasicColorDrawing"];
                effect.Parameters["ViewProjection"].SetValue(viewProjection);
                effect.Parameters["World"].SetValue(world);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }
            mesh.Draw();
        }

        foreach (var obstacle in _dinamicObstacles)
        {
            obstacle.Draw(viewProjection);
        }
        _staticObstacle.Draw(viewProjection, cameraPosition);
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
        if (IsOn)
        {
            foreach (var ship in _dinamicObstacles)
            {
                ship.Update(gameTime, player);
            }
            _staticObstacle.Update(player);
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
                effect.Parameters["ViewProjection"].SetValue(viewProjection);
                effect.Parameters["World"].SetValue(world);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }
            mesh.Draw();
        }
        foreach (var obstacle in _dinamicObstacles)
        {
            obstacle.DrawBloom(viewProjection);
        }
        _staticObstacle.DrawBloom(viewProjection);
    }

    public TipoDeModulo GetTipoDeModulo()
    {
        return TIPO;
    }
}