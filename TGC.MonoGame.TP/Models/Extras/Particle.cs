using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Models.Extras;

internal class Particle
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Lifetime;
    public float Age;
    public float Scale;          // escala del quad
    public Color Color;          // incluye alfa

    public bool IsAlive => Age < Lifetime;

    public void Reset(Vector3 position, Vector3 velocity,
                  float lifetime, float scale, Color color)
    {
        Position = position;
        Velocity = velocity;
        Lifetime = lifetime;
        Age = 0.0f;
        Scale = scale;
        Color = color;
    }
}