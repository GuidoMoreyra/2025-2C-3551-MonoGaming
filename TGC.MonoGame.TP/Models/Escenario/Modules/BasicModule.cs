using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Models.Modules.Contract;

namespace TGC.MonoGame.TP.Models.Modules;

internal class BasicModule : IModule
{
    private const float SCALE = 0.1f;
    private const TipoDeModulo TIPO = TipoDeModulo.Basic;

    private readonly Model _model;
    private readonly Matrix _scaleMatrix;
    public bool IsOn { get; set; }
    private Matrix _worldMatrix;

    public BasicModule()
    {
        IsOn = false;

        _model = Pasillo.GetModel();

        _scaleMatrix = Matrix.CreateScale(SCALE);
    }

    public void SetWorldMatrix(Matrix worldMatrix)
    {
        IsOn = false;
        _worldMatrix = _scaleMatrix * worldMatrix;
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
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
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
    }

    public TipoDeModulo GetTipoDeModulo()
    {
        return TIPO;
    }
}