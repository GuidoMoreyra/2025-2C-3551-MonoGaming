using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Util;



namespace TGC.MonoGame.TP.Models.Modules;

internal class BoxModule_1 : IModule
{
    private Matrix _worldMatrix;
    private Model _model;
    private List<Box> obstacles = new List<Box>();

    public Boolean isOn { get; set; }

    private TipoDeModulo tipo = TipoDeModulo.Corridor;
    private float scale = 0.1f;

    public BoxModule_1(ContentManager content, Matrix worldMatrix)
    {
        //Se construye los datos el pasillo
        _model = Pasillo.GetModel(content);
        _worldMatrix = worldMatrix;

        //Se construyen sus obstaculos
    

        Matrix worldMatrix_caja_1 =  worldMatrix * Matrix.CreateTranslation(Vector3.Backward*16f + Vector3.Down * 15f);
        Matrix worldMatrix_caja_2 =  worldMatrix * Matrix.CreateTranslation(Vector3.Forward*16f + Vector3.Down * 15f);

        var Caja_1 = new Box(content, worldMatrix_caja_1, 0, 0.3f, 0.25f);
        var Caja_2 = new Box(content, worldMatrix_caja_2, 0, 0.3f, 0.25f);

        obstacles.Add(Caja_1);
        obstacles.Add(Caja_2);
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

        foreach (var obstacle in obstacles)
        {
            obstacle.Draw(view, projection);
        }
    }



    public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
    {
        foreach (var obstacle in obstacles)
            obstacle.Update(gameTime, player, generator, ref escenario);

        obstacles.RemoveAll(o => o.estaDestruido);
    }

    public void DrawBloom(Matrix view, Matrix projection)
    {
        foreach (Box box in obstacles)
        {
            box.DrawBloom(view, projection);
        }
    }

    public TipoDeModulo Modulo()
    {
        return tipo;
    }
}