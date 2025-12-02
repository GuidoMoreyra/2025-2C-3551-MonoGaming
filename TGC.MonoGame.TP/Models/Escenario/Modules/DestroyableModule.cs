using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Models.Escenario.Contract;
using TGC.MonoGame.TP.Models.Player;

namespace TGC.MonoGame.TP.Models.Escenario.Modules;

internal class DestroyableModule : IModule
{
    private const float SCALE = 0.1f;
    private const TipoDeModulo tipo = TipoDeModulo.Destroyable;

    private readonly Model _model;
    private readonly Matrix _scaleMatrix;
    private Matrix _worldMatrix;
    public bool IsOn { get; set; }
    public List<DestroyableBox> Obstacles { get; }

    public DestroyableModule()
    {
        _model = Pasillo.GetModel();

        IsOn = false;

        _scaleMatrix = Matrix.CreateScale(SCALE);

        Obstacles = [];
        Obstacles.Add(new DestroyableBox());
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        IsOn = false;
        _worldMatrix = _scaleMatrix * worldMatrix;

        GenerateObstacles(worldMatrix);
    }

    private void GenerateObstacles(Matrix worldMatrix)
    {
        Obstacles[0].SetWorldMatrix(worldMatrix);
    }

    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime, GraphicsDevice _graphicsDevice)
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
        foreach (var ob in Obstacles)
        {
            ob.Draw(viewProjection, cameraPosition, _graphicsDevice);
        }
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
        foreach (var ob in Obstacles)
        {
            ob.DrawBloom(viewProjection);
        }
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
        if (IsOn)
        {
            foreach (var ob in Obstacles)
            {
                ob.Update(player);
            }
        }
    }

    public TipoDeModulo GetTipoDeModulo()
    {
        return tipo;
    }
}