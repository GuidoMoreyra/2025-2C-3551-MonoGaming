using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Util;

internal class GeneradorTextura
{
    /// <summary>
    /// Crea una textura cuadrada con líneas diagonales alternadas de dos colores.
    /// </summary>
    /// <param name="graphics">GraphicsDevice para crear la textura.</param>
    /// <param name="size">Ancho/alto en píxeles (p.ej. 128).</param>
    /// <param name="colorA">Primer color de la línea.</param>
    /// <param name="colorB">Segundo color de la línea.</param>
    /// <param name="stripeWidth">Grosor de la línea en píxeles.</param>
    public static Texture2D CreateDiagonalStripeTexture(GraphicsDevice graphics,
                                                        int size,
                                                        Color colorA,
                                                        Color colorB,
                                                        int stripeWidth = 4)
    {
        // Creamos un array de colores que contendrá los píxeles.
        Color[] data = new Color[size * size];

        // Cada píxel pertenece a una de las dos franjas según la suma (x+y).
        // Si la suma está dentro de un múltiplo de (2*stripeWidth) usamos colorA,
        // de lo contrario usamos colorB.
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int sum = x + y;                         // diagonal ↘︎
                int period = stripeWidth * 2;
                bool useA = (sum % period) < stripeWidth; // primera mitad del periodo
                data[y * size + x] = useA ? colorA : colorB;
            }
        }

        // Creamos la textura y la cargamos con los datos.
        Texture2D tex = new(graphics, size, size, false, SurfaceFormat.Color);
        tex.SetData(data);
        return tex;
    }

    /// <summary>
    /// Crea una textura cuadrada que representa una diana de disparo:
    /// tres círculos concéntricos y un punto en el centro.
    /// </summary>
    /// <param name="graphics">GraphicsDevice con el que crear la textura.</param>
    /// <param name="size">
    /// Tamaño de la textura (anchura y altura). Debe ser impar para que el punto quede centrado.
    /// </param>
    /// <param name="outerRadius">Radio del círculo exterior (en píxeles).</param>
    /// <param name="midRadius">Radio del círculo medio.</param>
    /// <param name="innerRadius">Radio del círculo interior (el punto).</param>
    /// <param name="outerColor">Color del anillo exterior.</param>
    /// <param name="midColor">Color del anillo medio.</param>
    /// <param name="innerColor">Color del anillo interior.</param>
    /// <param name="centerColor">Color del punto central.</param>
    /// <returns>Texture2D lista para asignar a un sampler.</returns>
    public static Texture2D CreateTargetTexture(
        GraphicsDevice graphics,
        int size,
        int outerRadius,
        int midRadius,
        int innerRadius,
        Color outerColor,
        Color midColor,
        Color innerColor,
        Color centerColor)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        if (outerRadius <= 0 || midRadius <= 0 || innerRadius <= 0)
            throw new ArgumentException("Todos los radios deben ser mayores que cero.");
        if (outerRadius > size / 2 || midRadius > size / 2 || innerRadius > size / 2)
            throw new ArgumentException("Los radios no pueden exceder la mitad del tamaño de la textura.");

        Color[] pixels = new Color[size * size];

        // Centro de la textura (coordenadas en píxeles)
        int cx = size / 2;
        int cy = size / 2;

        // Pre‑calculamos los cuadrados de los radios para evitar sqrt().
        int outerSq = outerRadius * outerRadius;
        int midSq = midRadius * midRadius;
        int innerSq = innerRadius * innerRadius;

        for (int y = 0; y < size; y++)
        {
            int dy = y - cy;
            int dy2 = dy * dy; // (y - cy)^2

            for (int x = 0; x < size; x++)
            {
                int dx = x - cx;
                int distSq = dx * dx + dy2; // distancia al cuadrado

                // Determinamos en qué anillo está el píxel
                Color chosen;

                if (distSq <= innerSq)               // punto central
                    chosen = centerColor;
                else if (distSq <= innerSq)           // (redundante, pero deja la lógica clara)
                    chosen = innerColor;
                else if (distSq <= midSq)             // anillo interior
                    chosen = innerColor;
                else if (distSq <= outerSq)           // anillo medio
                    chosen = midColor;
                else                                 // fuera del círculo externo
                    chosen = outerColor; // opcional: puedes dejarlo transparente (new Color(0,0,0,0))

                // Guardamos el color en el array (fila‑major)
                pixels[y * size + x] = chosen;
            }
        }

        Texture2D tex = new(
            graphics,
            width: size,
            height: size,
            mipmap: false,
            format: SurfaceFormat.Color); // 4 bytes/píxel = RGBA8

        tex.SetData(pixels);
        return tex;
    }
}