using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Modules;

internal class CargoModule : IModule
{
    private Matrix _worldMatrix;
    private Model _model;
    private List<CargoShip> obstacles = new List<CargoShip>();
    private float scale = 0.04f;


    private float ajusteDeTraslacionY = 6.5f;
    private float ajusteDeTraslacionZ = 18.5f;

    public CargoModule(ContentManager content, Matrix worldMatrix)
    {
        _model = Pasillo_Asteroide.GetModel(content);

        _worldMatrix = worldMatrix;
        GenerateObstacles(content, worldMatrix);
    }


    public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition,float elapsedTime)
    {

        var ajusteDeTraslacion_ = Matrix.CreateTranslation(Vector3.Right * ajusteDeTraslacionZ + Vector3.Down * ajusteDeTraslacionY);
        var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(-90f));

        foreach (var mesh in _model.Meshes)
        {
            var meshWorld = mesh.ParentBone.Transform;
            var scaleMatrix = Matrix.CreateScale(scale);
            var world = meshWorld * rotation * scaleMatrix * ajusteDeTraslacion_ * _worldMatrix;
            foreach (var meshPart in mesh.MeshParts)
            {
                var effect = meshPart.Effect;
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                effect.Parameters["World"].SetValue(world);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
            }
            // Draw the mesh.
            mesh.Draw();
        }
        foreach (CargoShip ship in obstacles)
        {
            ship.Draw(view, projection, cameraPosition);
        }
    }

    public void GenerateObstacles(ContentManager content, Matrix worldMatrix)
    {
        var traslacion1 = Matrix.CreateTranslation(Vector3.Left * 15f);
        var traslacion2 = Matrix.CreateTranslation(Vector3.Right * 15f);
        obstacles.Add(new CargoShip(content, worldMatrix * traslacion1));
        obstacles.Add(new CargoShip(content, worldMatrix * traslacion2));

    }
    private void GenerateDecoration() { }


    public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
    {
        foreach (var obstacle in obstacles)
            obstacle.Update(gameTime, player, generator, ref escenario);

        obstacles.RemoveAll(o => o.estaDestruido);
    }

    private float GenerateNumber(float x)
    {
        Random random = new Random();
        // random.NextDouble() devuelve un valor entre 0.0 y 1.0
        // Lo transformamos para que quede entre -x y +x
        return (float)((random.NextDouble() * 2 - 1) * x);
    }

    public string Modulo()
    {
        return "Asteroid";
    }
}