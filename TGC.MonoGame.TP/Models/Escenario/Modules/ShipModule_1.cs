using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Modules;

internal class ShipMdoule_1 : IModule
{
    private readonly Matrix _worldMatrix;
    private readonly Model _model;

    private readonly TipoDeModulo tipo = TipoDeModulo.Astetroid;

    public Boolean isOn { get; set; }
    private List<Ship> dinamicObstacles = new List<Ship>();

    private CargoShip staticObatacle;
    private float scale = 0.04f;


    public ShipMdoule_1(ContentManager content, Matrix worldMatrix)
    {   
        //Se construye los datos el pasillo
        isOn = false;
        _model = Pasillo_Asteroide.GetModel(content);
        _worldMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(90)) * worldMatrix;

        //Se construyen sus obstaculos
        
        Matrix shipWorldMatrix1 =   worldMatrix ;
        Matrix shipWorldMatrix2 =  Matrix.CreateTranslation(Vector3.Forward * 15f + Vector3.Up *10f) * worldMatrix ;
       
        var Nave_1 = new Ship(content,shipWorldMatrix1);
        var Nave_2 = new Ship(content,shipWorldMatrix2);

        dinamicObstacles.Add(Nave_1);
        dinamicObstacles.Add(Nave_2);

        Matrix shipWorldMatrix3 =  Matrix.CreateRotationX(MathHelper.ToDegrees(60)) * Matrix.CreateTranslation(Vector3.Backward * 10f + Vector3.Up *10f) * worldMatrix ;
        staticObatacle = new CargoShip(content, shipWorldMatrix3);


    }

    public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition,float elapsedTime)
    {


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

        foreach (var obstacle in dinamicObstacles)
        {
            obstacle.Draw(view, projection);
        }
        staticObatacle.Draw(view, projection,cameraPosition);
    }

    public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
    {   
        //La nave solo se mueve si el jugador esta sobre el modulo
        if (isOn)
        {
            foreach(var ship in dinamicObstacles)
            {
                ship.Update(gameTime,player);
            }
        }
        else {
            foreach(var ship in dinamicObstacles)
            {
                ship.Reset();
            }
        }
    }

    public TipoDeModulo Modulo()
    {
        return tipo;
    }

    public void DrawBloom(Matrix view, Matrix projection)
    {
    }
}