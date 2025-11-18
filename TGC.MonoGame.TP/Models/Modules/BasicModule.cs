
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Modules;

internal class BasicModule : IModule
{
    private Matrix _worldMatrix;
    private Model _model;

    private float scale = 0.1f;


    public BasicModule(ContentManager content, Matrix worldMatrix)
    {
        _model = Pasillo.GetModel(content);

        _worldMatrix = worldMatrix;
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
    }

    private void GenerateObstacles(ContentManager content, string contentFolder3D, string contentFolderEffects, Matrix worldMatrix) { }
    private void GenerateDecoration() { }


    public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
    {

    }

    public string Modulo()
    {
        return "Basic";
    }

    public void DrawBloom(Matrix view, Matrix projection)
    {
    }
}