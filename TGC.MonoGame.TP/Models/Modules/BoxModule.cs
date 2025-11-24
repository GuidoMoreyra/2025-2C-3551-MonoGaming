using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Modules.Contract;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Util;


namespace TGC.MonoGame.TP.Models.Modules;

internal class BoxModule : IModule
{
    private const int CANTIDAD_OBSTACULOS = 6;
    private const float SCALE = 0.1f;
    //Medidas del Modulo
    private const int UP = 8;
    private const int RIGHT = 25;
    private const int FORWARD = 18;

    private readonly Model _model;
    private readonly List<Box> _obstacles = [];
    private readonly Matrix _scaleMatrix;
    private Matrix _worldMatrix;

    public BoxModule()
    {
        _model = Pasillo.GetModel();

        _scaleMatrix = Matrix.CreateScale(SCALE);

        for (int i = 0; i < CANTIDAD_OBSTACULOS; i++)
        {
            _obstacles.Add(new Box());
        }
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        _worldMatrix = _scaleMatrix * worldMatrix;

        GenerateObstacles(worldMatrix);
    }

    private void GenerateObstacles(Matrix worldMatrix)
    {
        foreach (Box box in _obstacles)
        {
            Matrix traslacionDeCaja = Matrix.CreateTranslation(Vector3.Forward * Utils.GenerateNumber(FORWARD) + Vector3.Up * Utils.GenerateNumber(UP) + Vector3.Right * Utils.GenerateNumber(RIGHT));
            box.SetWorldMatrix(worldMatrix * traslacionDeCaja);
        }
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
                effect.Parameters["ViewProjection"].SetValue(viewProjection);
                effect.Parameters["World"].SetValue(world);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }
            mesh.Draw();
        }

        foreach (Box box in _obstacles)
        {
            box.Draw(viewProjection);
        }
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
        foreach (var obstacle in _obstacles)
            obstacle.Update(player);

    }

    public void DrawBloom(Matrix viewProjection)
    {
        foreach (Box box in _obstacles)
        {
            box.DrawBloom(viewProjection);
        }
    }

    public static int GetModuleNumber()
    {
        return 1;
    }
}