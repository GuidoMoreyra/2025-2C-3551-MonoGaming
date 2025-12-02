using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models.BaseModels;

public class QuadMesh
{
    private static readonly VertexPositionTexture[] _vertices =
        [
        new VertexPositionTexture(new Vector3(-0.5f,  0.5f, 0f), new Vector2(0, 0)), // Top-Left
        new VertexPositionTexture(new Vector3( 0.5f,  0.5f, 0f), new Vector2(1, 0)), // Top-Right
        new VertexPositionTexture(new Vector3(-0.5f, -0.5f, 0f), new Vector2(0, 1)), // Bottom-Left
        new VertexPositionTexture(new Vector3( 0.5f, -0.5f, 0f), new Vector2(1, 1)), // Bottom-Right
        ];

    private static readonly short[] _indices = [
        2, 1, 0, 
        3, 1, 2
        ];
    public readonly Effect _effect;
    private readonly GraphicsDevice _graphicsDevice;


    public QuadMesh(GraphicsDevice graphicsDevice, Effect effect)
    {
        _effect = effect;
        _graphicsDevice = graphicsDevice;
    }

    public void Draw(Matrix world, Matrix viewProjection)
    {
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            _effect.Parameters["World"].SetValue(world);
            _effect.Parameters["ViewProjection"].SetValue(viewProjection);
            pass.Apply();

            _graphicsDevice.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                _vertices,
                0,
                _vertices.Length,
                _indices,
                0,
                _indices.Length / 3);
        }


    }
}