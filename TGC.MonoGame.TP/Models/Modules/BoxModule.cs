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

internal class BoxModule : IModule
{
    private Matrix _worldMatrix;
    private Model _model;
    private List<Box> obstacles = new List<Box>();
    //Medidas del Modulo
    private float scale = 0.1f;
    private readonly int Up = 8;
    private readonly int Right = 25;
    private readonly int Foward = 18;

    public BoxModule(ContentManager content, Matrix worldMatrix)
    {
        _model = Pasillo.GetModel(content);

        _worldMatrix = worldMatrix;

        GenerateObstacles(content, worldMatrix);
    }

    public void GenerateDecoration()
    {
        //Deberia generar las decoraciones del modulo.
    }


    //Deberia generar  una pocision aleatoria con respecto del centro del modulo.
    //De momento se deja con una posicion fija.
    public void GenerateObstacles(ContentManager content, Matrix worldMatrix)
    {
        int cantidadMaximaDeObstaculos = 6;
        for (int index = 0; index < cantidadMaximaDeObstaculos; index++)
        {
            Matrix traslacionDeCaja = Matrix.CreateTranslation(Vector3.Forward * GenerateNumber(Foward) + Vector3.Up * GenerateNumber(Up) + Vector3.Right * GenerateNumber(Right));
            obstacles.Add(new Box(content, worldMatrix * traslacionDeCaja, GenerateNumber(90)));
        }
    }

    public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition,float elapsedTime)
    {

        // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.

        foreach (var mesh in _model.Meshes)
        {
            var meshWorld = mesh.ParentBone.Transform;
            var scaleMatrix = Matrix.CreateScale(scale);
            var world = meshWorld * scaleMatrix * _worldMatrix;
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

        foreach (Box box in obstacles)
        {
            box.Draw(view, projection);
        }
    }



    public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
    {
        foreach (var obstacle in obstacles)
            obstacle.Update(gameTime, player, generator, ref escenario);

        obstacles.RemoveAll(o => o.estaDestruido);
    }


    private float GenerateNumber(float x)
    {
        Random random = new Random();
        return (float)((random.NextDouble() * 2 - 1) * x);
    }

    public string Modulo()
    {
        return "Corridor";
    }
}