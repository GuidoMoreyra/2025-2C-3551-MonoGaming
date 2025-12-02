using System.Collections.Generic;

namespace TGC.MonoGame.TP.Models.Extras;

internal sealed class ParticlePool
{
    private readonly List<Particle> _available = [];
    private readonly List<Particle> _inUse = [];

    public ParticlePool(int capacity)
    {
        // Pre‑allocamos la capacidad máxima para evitar re‑allocations.
        for (int i = 0; i < capacity; i++)
            _available.Add(new Particle());
    }

    /// <summary>
    /// Obtiene una partícula del pool (crea una nueva sólo si
    /// la capacidad máxima no se ha alcanzado y no hay libres).
    /// </summary>
    public Particle Acquire()
    {
        Particle p;
        if (_available.Count > 0)
        {
            // Tomamos la última para O(1)
            int last = _available.Count - 1;
            p = _available[last];
            _available.RemoveAt(last);
        }
        else
        {
            // Pool agotado → devolvemos null y el llamador decide
            // si descarta la emisión o la reduce.
            return null;
        }

        _inUse.Add(p);
        return p;
    }

    /// <summary>
    /// Devuelve la partícula al pool para reutilizarla.
    /// </summary>
    public void Release(Particle p)
    {
        // Eliminamos de la lista activa (O(n) pero n es pequeño)
        _inUse.Remove(p);
        _available.Add(p);
    }

    /// <summary>
    /// Enumerador de partículas activas (para Update/Draw).
    /// </summary>
    public IEnumerable<Particle> Active => _inUse;
}