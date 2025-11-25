
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.Models.Modules.Contract;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Modules;

internal class PipeModule : IModule
{
    private  Matrix _worldMatrix;
    public bool IsOn { get; set; }
    private readonly Model _model;

    private Matrix worldMatrix2 = Matrix.CreateTranslation(Vector3.Forward * 7f);
    private Matrix worldMatrix3 = Matrix.CreateTranslation(Vector3.Backward * 7f);

    public List<DestroyableBox> obstaclesD{ get; set; }
    public Boolean isOn { get; set; }
    private readonly TipoDeModulo tipo = TipoDeModulo.Pipe;
    private float scale = 0.1f;


    public PipeModule()
    {
        _model =  Pasillo.GetModel();

        obstaclesD = new List<DestroyableBox>{new DestroyableBox()};


    }


    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime, GraphicsDevice _graphicsDevice )
    {
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
        foreach (var ob in obstaclesD)
        {
                ob.Draw(viewProjection,cameraPosition);
        }
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        IsOn = false;
        var _scaleMatrix = Matrix.CreateScale(scale);
        _worldMatrix = _scaleMatrix * worldMatrix;

        GenerateObstacles(worldMatrix);
    }

    private void GenerateObstacles(Matrix worldMatrix)
    {

        obstaclesD[0].SetWorldMatrix(worldMatrix);
    }


    public  void DrawBloom(Matrix viewProjection)
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
        foreach (var ob in obstaclesD)
        {
                ob.DrawBloom(viewProjection);
        }
    }
    public void Update(GameTime gameTime, PlayerShip player)
    {
        if(IsOn){
            foreach (var ob in obstaclesD)
        {
                ob.Update(gameTime, player);
        }
        }
    }

    public TipoDeModulo GetTipoDeModulo()
    {
        return tipo;
    }

    public void DrawBloom(Matrix view, Matrix projection)
    {
    }
}