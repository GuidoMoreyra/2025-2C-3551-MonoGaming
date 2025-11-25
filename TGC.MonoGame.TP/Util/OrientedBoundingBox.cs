using System;
using Microsoft.Xna.Framework;

public struct OrientedBoundingBox
{
    public Vector3 Center;
    public Vector3 HalfExtents;
    public Vector3 AxisX;
    public Vector3 AxisY;
    public Vector3 AxisZ;

    public OrientedBoundingBox(Vector3 localCenter, Vector3 localHalfExtents, Matrix world)
    {
        // 1. Transformar el centro local al espacio mundial
        Center = Vector3.Transform(localCenter, world);
        
        // 2. Extraer y normalizar los ejes rotados y escalados
        
        // Aislar la rotación y escala
        Matrix rotationScale = world;
        rotationScale.Translation = Vector3.Zero; 

        AxisX = rotationScale.Right;
        AxisY = rotationScale.Up;
        AxisZ = rotationScale.Forward;

        // Las HalfExtents deben escalarse por la longitud (factor de escala) de sus respectivos ejes.
        
        HalfExtents.X = AxisX.Length() * localHalfExtents.X;
        HalfExtents.Y = AxisY.Length() * localHalfExtents.Y;
        HalfExtents.Z = AxisZ.Length() * localHalfExtents.Z;
        
        // Normalizar los ejes de la OBB (dejando la magnitud en HalfExtents)
        AxisX.Normalize();
        AxisY.Normalize();
        AxisZ.Normalize();
    }
    
    // ==============================================================================
    // FUNCIÓN CRÍTICA: TEOROMA DEL EJE SEPARADOR (SAT) - OBB vs AABB
    // ==============================================================================
    
    public bool Intersects(BoundingBox aabb)
    {
        // 3 Ejes de la OBB
        if (IsSeparatingAxis(AxisX, aabb)) return false;
        if (IsSeparatingAxis(AxisY, aabb)) return false;
        if (IsSeparatingAxis(AxisZ, aabb)) return false;

        // 3 Ejes de la AABB (Ejes del mundo: X, Y, Z)
        if (IsSeparatingAxis(Vector3.Right, aabb)) return false;      // Eje X del mundo
        if (IsSeparatingAxis(Vector3.Up, aabb)) return false;         // Eje Y del mundo
        if (IsSeparatingAxis(Vector3.Backward, aabb)) return false;   // Eje Z del mundo
        
        // 9 Ejes Cruzados (Producto Cruz OBB x AABB)
        // Ejes AABB: Xw (Right), Yw (Up), Zw (Backward)

        Vector3 Xw = Vector3.Right;
        Vector3 Yw = Vector3.Up;
        Vector3 Zw = Vector3.Backward; 

        // OBB AxisX x AABB axes
        if (IsSeparatingAxis(Vector3.Cross(AxisX, Xw), aabb)) return false;
        if (IsSeparatingAxis(Vector3.Cross(AxisX, Yw), aabb)) return false;
        if (IsSeparatingAxis(Vector3.Cross(AxisX, Zw), aabb)) return false;
        
        // OBB AxisY x AABB axes
        if (IsSeparatingAxis(Vector3.Cross(AxisY, Xw), aabb)) return false;
        if (IsSeparatingAxis(Vector3.Cross(AxisY, Yw), aabb)) return false;
        if (IsSeparatingAxis(Vector3.Cross(AxisY, Zw), aabb)) return false;

        // OBB AxisZ x AABB axes
        if (IsSeparatingAxis(Vector3.Cross(AxisZ, Xw), aabb)) return false;
        if (IsSeparatingAxis(Vector3.Cross(AxisZ, Yw), aabb)) return false;
        if (IsSeparatingAxis(Vector3.Cross(AxisZ, Zw), aabb)) return false;

        // Si NINGÚN eje separador fue encontrado, las cajas colisionan.
        return true;
    }

    // ==============================================================================
    // MÉTODOS AUXILIARES
    // ==============================================================================
    
    /// <summary>
    /// Verifica si el eje dado es un eje separador (no hay solapamiento de proyecciones).
    /// </summary>
    private bool IsSeparatingAxis(Vector3 axis, BoundingBox aabb)
    {
        // Ignorar ejes con longitud cero (puede ser resultado del producto cruz en el mismo plano)
        if (axis.LengthSquared() < 0.00001f) return false;

        // Proyectar ambos objetos sobre el eje.
        ProjectOBB(axis, out float minA, out float maxA); // Proyección de la OBB (this)
        ProjectAABB(aabb, axis, out float minB, out float maxB); // Proyección de la AABB
        
        // Comprobar si las proyecciones se superponen
        // No hay colisión si (maxA < minB) o (maxB < minA)
        return maxA < minB || maxB < minA;
    }
    
    /// <summary>
    /// Proyecta la OBB sobre un eje y devuelve los valores mínimo y máximo.
    /// </summary>
    private void ProjectOBB(Vector3 axis, out float min, out float max)
    {
        // Proyectar el centro en el eje
        float centerProjection = Vector3.Dot(Center, axis);

        // Calcular la extensión de la OBB en ese eje
        // Se usa el valor absoluto del producto punto del eje con cada eje local de la OBB,
        // multiplicado por la HalfExtent de ese eje.
        float extent = 
            Math.Abs(Vector3.Dot(HalfExtents.X * AxisX, axis)) +
            Math.Abs(Vector3.Dot(HalfExtents.Y * AxisY, axis)) +
            Math.Abs(Vector3.Dot(HalfExtents.Z * AxisZ, axis));

        min = centerProjection - extent;
        max = centerProjection + extent;
    }

    /// <summary>
    /// Proyecta la AABB sobre un eje y devuelve los valores mínimo y máximo.
    /// </summary>
    private static void ProjectAABB(BoundingBox aabb, Vector3 axis, out float min, out float max)
    {
        // Obtener los 8 vértices de la AABB
        Vector3[] corners = new Vector3[8];
        aabb.GetCorners(corners);

        // Proyectar el primer vértice para inicializar min/max
        min = max = Vector3.Dot(corners[0], axis);

        // Proyectar los 7 vértices restantes y actualizar min/max
        for (int i = 1; i < 8; i++)
        {
            float projection = Vector3.Dot(corners[i], axis);

            if (projection < min)
                min = projection;
            else if (projection > max)
                max = projection;
        }
    }
}