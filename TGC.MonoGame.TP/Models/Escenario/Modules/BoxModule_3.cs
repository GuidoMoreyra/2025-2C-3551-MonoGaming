using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Escenario.Contract;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Models.Player;


namespace TGC.MonoGame.TP.Models.Escenario.Modules;

internal class BoxModule_3 : IModule
{
    private const float SCALE = 0.1f;
    private const float BOX_TRANSLATION_Y = 15.0f;
    private const float BOX_ANGLE = 0.0f;
    private const float BOX_SCALE_Y = 0.2f;
    private const float BOX_SCALE_Z = 0.5f;
    private const TipoDeModulo TIPO = TipoDeModulo.Box3;
    private readonly Model _model;
    private readonly List<Box> _obstacles;
    private readonly Matrix _scaleMatrix;
    private readonly Matrix _cajaTranslation;
    public bool IsOn { get; set; }
    private Matrix _worldMatrix;

    public BoxModule_3()
    {
        IsOn = false;

        _model = Pasillo.GetModel();

        _scaleMatrix = Matrix.CreateScale(SCALE);

        _cajaTranslation = Matrix.CreateTranslation(Vector3.Down * BOX_TRANSLATION_Y);

        _obstacles = [];
        _obstacles.Add(new Box(BOX_ANGLE, BOX_SCALE_Y, BOX_SCALE_Z));
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

    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime, GraphicsDevice _graphicsDevice)
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
            box.Draw(viewProjection, cameraPosition, _graphicsDevice);
        }
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
        if (IsOn)
        {
            foreach (var obstacle in _obstacles)
            {
                obstacle.Update(player);
            }
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