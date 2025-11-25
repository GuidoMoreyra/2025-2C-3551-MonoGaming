using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Models.Modules.Contract;
using TGC.MonoGame.TP.Models.Obstacles;


namespace TGC.MonoGame.TP.Models.Modules;

internal class BoxModule_2 : IModule
{
    private const float SCALE = 0.1f;
    private const TipoDeModulo TIPO = TipoDeModulo.Box2;

    private readonly Model _model;
    private readonly List<Box> _obstacles;
    private readonly Matrix _scaleMatrix;
    private readonly Matrix _cajaTranslation;
    public bool IsOn { get; set; }
    private Matrix _worldMatrix;

    public BoxModule_2()
    {
        IsOn = false;

        _model = Pasillo.GetModel();

        _scaleMatrix = Matrix.CreateScale(SCALE);

        _cajaTranslation = Matrix.CreateTranslation(Vector3.Up * 13f + Vector3.Down * 15f);

        _obstacles = [];
        _obstacles.Add(new Box(0, 0.2f, 0.5f));
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        IsOn = false;

        _worldMatrix = _scaleMatrix * worldMatrix;

        GenerateObstacles(worldMatrix);
    }

    private void GenerateObstacles(Matrix worldMatrix)
    {
        _obstacles[0].SetWorldMatrix(worldMatrix * _cajaTranslation);
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

        foreach (Box box in _obstacles)
        {
            box.Draw(viewProjection);
        }
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
        // if (IsOn)
        // {
            foreach (var obstacle in _obstacles)
                obstacle.Update(player);
        // }
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
        foreach (Box box in _obstacles)
        {
            box.DrawBloom(viewProjection);
        }
    }

    public TipoDeModulo GetTipoDeModulo()
    {
        return TIPO;
    }
}