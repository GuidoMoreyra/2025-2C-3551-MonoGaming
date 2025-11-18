using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Modules;

internal class PipeModule : IModule
{
    private Matrix _worldMatrix;
    private Model _model;
    private List<Ship> obstacles = new List<Ship>();

    //Medidas del Modulo
    private float scale = 0.2f;

    public PipeModule(ContentManager content, Matrix worldMatrix)
    {
        //Instancio modelo
        _model = Pipe.GetModel(content);

        //Matriz de mundo
        _worldMatrix = worldMatrix;

    }

public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, float elapsedTime)
{
    foreach (var mesh in _model.Meshes)
    {

        var meshWorld = mesh.ParentBone.Transform;


        var scaleMatrix = Matrix.CreateScale(scale);
        var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(-90f));


        var world = meshWorld * rotation * scaleMatrix * _worldMatrix;

        foreach (var meshPart in mesh.MeshParts)
        {
            var effect = meshPart.Effect;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);


            effect.Parameters["Time"].SetValue(elapsedTime*1);
            effect.Parameters["Speed"].SetValue(new Vector2(0.1f, 0.1f));
        }


        mesh.Draw();
    }
}




    public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
    {
        foreach (var obstacle in obstacles)
            obstacle.Update(gameTime, player, generator, ref escenario);

        obstacles.RemoveAll(o => o.estaDestruido);
    }

    public string Modulo()
    {
        return "Corridor";
    }
}