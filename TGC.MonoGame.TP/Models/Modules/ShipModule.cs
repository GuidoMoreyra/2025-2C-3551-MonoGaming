using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Modules.Contract;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Modules;

internal class ShipModule : IModule
{
    //Medidas del Modulo
    private const float SCALE = 0.1f;
    private const int UP = 6;
    private const int RIGHT = 20;
    private const int FORWARD = 13;
    private const int CANTIDAD_OBSTACULOS = 2;

    private readonly Model _model;
    private readonly Matrix _scaleMatrix;
    private readonly List<Ship> _obstacles = [];
    private Matrix _worldMatrix;



    public ShipModule()
    {
        _model = Pasillo.GetModel();

        _scaleMatrix = Matrix.CreateScale(SCALE);

        for (int index = 0; index < CANTIDAD_OBSTACULOS; index++)
        {
            _obstacles.Add(new Ship());
        }
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        _worldMatrix = _scaleMatrix * worldMatrix;

        GenerateObstacles(worldMatrix);
    }

    private void GenerateObstacles(Matrix worldMatrix)
    {
        foreach (Ship ship in _obstacles)
        {
            Matrix traslacionDeNave = Matrix.CreateTranslation(Vector3.Forward * Utils.GenerateNumber(FORWARD) + Vector3.Up * Utils.GenerateNumber(UP) + Vector3.Right * Utils.GenerateNumber(RIGHT));
            ship.SetWorldMatrix(worldMatrix * traslacionDeNave);
        }
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

        foreach (Ship ship in _obstacles)
        {
            ship.Draw(viewProjection);
        }
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
        foreach (var obstacle in _obstacles)
            obstacle.Update(player);
    }

    public void DrawBloom(Matrix viewProjection)
    {
        foreach (Ship ship in _obstacles)
        {
            ship.DrawBloom(viewProjection);
        }
    }

    public static int GetModuleNumber()
    {
        return 4;
    }
}