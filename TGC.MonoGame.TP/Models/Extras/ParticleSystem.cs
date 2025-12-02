using System;
using Microsoft.Xna.Framework;
using System.Linq;
using TGC.MonoGame.TP.Models.BaseModels;

namespace TGC.MonoGame.TP.Models.Extras;

internal class ParticleSystem
{
    public const int MaxParticles = 60;          // capacidad del pool
    public const float Movement = 5.0f;

    private static ParticleSystem instance;

    private readonly QuadMesh _quadMesh;
    private readonly ParticlePool _pool;
    private readonly Random _rnd = new();

    public static void InitializeParticleSystem(QuadMesh quadMesh)
    {
        instance = new ParticleSystem(quadMesh);
    }

    public static ParticleSystem GetParticleSystem()
    {
        return instance;
    }

    public ParticleSystem(QuadMesh quadMesh)
    {
        _quadMesh = quadMesh;
        _pool = new ParticlePool(MaxParticles);
    }

    public void Emit(Vector3 position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Particle p = _pool.Acquire();
            if (p == null) break; // pool agotado → salimos temprano

            // Generamos valores aleatorios
            Vector3 vel = new(
                RandomRange(3.0f, 7.0f),
                RandomRange(3.0f, 7.0f),
                RandomRange(3.0f, 7.0f));

            float life = RandomRange(0.1f, 0.5f);
            float scale = RandomRange(0.2f, 0.6f);
            Color color = Color.OrangeRed;

            p.Reset(position, vel, life, scale, color);
        }
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Particle[] active = _pool.Active.ToArray();

        foreach (Particle p in active)
        {
            // Física básica
            p.Velocity += new Vector3(0.0f, Movement, 0.0f) * dt;
            p.Position += p.Velocity * dt;
            p.Age += dt;

            // Fade‑out (reducción de alfa)
            Vector4 colorVector = p.Color.ToVector4();
            float t = p.Age / p.Lifetime;
            colorVector.W = t;
            p.Color = new Color(colorVector);

            // Cuando la vida termina, devolvemos al pool
            if (!p.IsAlive)
                _pool.Release(p);
        }
    }

    public void Draw(Matrix viewProjection, Vector3 cameraPosition)
    {
        foreach (Particle p in _pool.Active)
        {
            // World = escala → billboard → traslación
            Matrix scale = Matrix.CreateScale(p.Scale);
            Matrix billboard = Matrix.CreateBillboard(p.Position, cameraPosition, Vector3.Up, null);
            Matrix world = scale * billboard;

            // Pasamos el color (incluido alfa) al shader.
            _quadMesh._effect.Parameters["DiffuseColor"].SetValue(p.Color.ToVector4());

            // Dibujamos el quad.
            _quadMesh.Draw(world, viewProjection);
        }
    }

    private float RandomRange(float min, float max) =>
        (float)_rnd.NextDouble() * (max - min) + min;
}