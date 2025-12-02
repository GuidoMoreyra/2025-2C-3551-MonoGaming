using System;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.Util
{
    public struct OrientedBoundingBox
    {
        public Vector3 Center;
        public Vector3 HalfExtents;
        public Vector3 AxisX;
        public Vector3 AxisY;
        public Vector3 AxisZ;

        public OrientedBoundingBox(Vector3 localCenter,
                                   Vector3 localHalfExtents,
                                   Matrix world)
        {
            // Posición en espacio mundial
            Center = Vector3.Transform(localCenter, world);

            // Extraer rotación + escala (sin traslación)
            Matrix rotScale = world;
            rotScale.Translation = Vector3.Zero;

            AxisX = rotScale.Right;
            AxisY = rotScale.Up;
            AxisZ = rotScale.Forward;

            // Escalar las medias extensiones según la longitud de cada eje
            HalfExtents.X = AxisX.Length() * localHalfExtents.X;
            HalfExtents.Y = AxisY.Length() * localHalfExtents.Y;
            HalfExtents.Z = AxisZ.Length() * localHalfExtents.Z;

            // Normalizar ejes (la magnitud quedó en HalfExtents)
            AxisX.Normalize();
            AxisY.Normalize();
            AxisZ.Normalize();
        }

        /// <summary>
        /// Intersección OBB ↔ OBB usando el Separating Axis Theorem.
        /// </summary>
        public readonly bool Intersects(OrientedBoundingBox other) =>
            IntersectsInternal(this, other);

        /// <summary>
        /// Intersección OBB ↔ AABB usando el Separating Axis Theorem.
        /// </summary>
        public readonly bool Intersects(BoundingBox aabb) =>
            IntersectsInternal(this, aabb);

        // ---------- OBB ↔ OBB ----------
        private static bool IntersectsInternal(OrientedBoundingBox a,
                                               OrientedBoundingBox b)
        {
            // 3 ejes propios de a
            if (IsSeparatingAxis(a.AxisX, a, b)) return false;
            if (IsSeparatingAxis(a.AxisY, a, b)) return false;
            if (IsSeparatingAxis(a.AxisZ, a, b)) return false;

            // 3 ejes propios de b
            if (IsSeparatingAxis(b.AxisX, a, b)) return false;
            if (IsSeparatingAxis(b.AxisY, a, b)) return false;
            if (IsSeparatingAxis(b.AxisZ, a, b)) return false;

            // 9 ejes cruzados
            if (IsSeparatingAxis(Vector3.Cross(a.AxisX, b.AxisX), a, b)) return false;
            if (IsSeparatingAxis(Vector3.Cross(a.AxisX, b.AxisY), a, b)) return false;
            if (IsSeparatingAxis(Vector3.Cross(a.AxisX, b.AxisZ), a, b)) return false;

            if (IsSeparatingAxis(Vector3.Cross(a.AxisY, b.AxisX), a, b)) return false;
            if (IsSeparatingAxis(Vector3.Cross(a.AxisY, b.AxisY), a, b)) return false;
            if (IsSeparatingAxis(Vector3.Cross(a.AxisY, b.AxisZ), a, b)) return false;

            if (IsSeparatingAxis(Vector3.Cross(a.AxisZ, b.AxisX), a, b)) return false;
            if (IsSeparatingAxis(Vector3.Cross(a.AxisZ, b.AxisY), a, b)) return false;
            if (IsSeparatingAxis(Vector3.Cross(a.AxisZ, b.AxisZ), a, b)) return false;

            // Ningún eje separó → colisión
            return true;
        }

        // ---------- OBB ↔ AABB ----------
        private static bool IntersectsInternal(OrientedBoundingBox obb,
                                               BoundingBox aabb)
        {
            // 3 ejes de la OBB
            if (IsSeparatingAxis(obb.AxisX, obb, aabb)) return false;
            if (IsSeparatingAxis(obb.AxisY, obb, aabb)) return false;
            if (IsSeparatingAxis(obb.AxisZ, obb, aabb)) return false;

            // 3 ejes del mundo (AABB)
            if (IsSeparatingAxis(Vector3.Right,   obb, aabb)) return false;
            if (IsSeparatingAxis(Vector3.Up,      obb, aabb)) return false;
            if (IsSeparatingAxis(Vector3.Backward,obb, aabb)) return false;

            // 9 ejes cruzados OBB × AABB
            Vector3 Xw = Vector3.Right;
            Vector3 Yw = Vector3.Up;
            Vector3 Zw = Vector3.Backward;

            if (IsSeparatingAxis(Vector3.Cross(obb.AxisX, Xw), obb, aabb)) return false;
            if (IsSeparatingAxis(Vector3.Cross(obb.AxisX, Yw), obb, aabb)) return false;
            if (IsSeparatingAxis(Vector3.Cross(obb.AxisX, Zw), obb, aabb)) return false;

            if (IsSeparatingAxis(Vector3.Cross(obb.AxisY, Xw), obb, aabb)) return false;
            if (IsSeparatingAxis(Vector3.Cross(obb.AxisY, Yw), obb, aabb)) return false;
            if (IsSeparatingAxis(Vector3.Cross(obb.AxisY, Zw), obb, aabb)) return false;

            if (IsSeparatingAxis(Vector3.Cross(obb.AxisZ, Xw), obb, aabb)) return false;
            if (IsSeparatingAxis(Vector3.Cross(obb.AxisZ, Yw), obb, aabb)) return false;
            if (IsSeparatingAxis(Vector3.Cross(obb.AxisZ, Zw), obb, aabb)) return false;

            return true;
        }

        // ---------- SEPARATING AXIS TEST ----------
        // OBB ↔ OBB
        private static bool IsSeparatingAxis(Vector3 axis,
                                            OrientedBoundingBox a,
                                            OrientedBoundingBox b)
        {
            // Ejes casi nulos (producto cruzado paralelo) no aportan información
            if (axis.LengthSquared() < 1e-5f) return false;

            ProjectOBB(a, axis, out float minA, out float maxA);
            ProjectOBB(b, axis, out float minB, out float maxB);

            // Si los intervalos no se solapan → eje separador
            return maxA < minB || maxB < minA;
        }

        // OBB ↔ AABB
        private static bool IsSeparatingAxis(Vector3 axis,
                                            OrientedBoundingBox obb,
                                            BoundingBox aabb)
        {
            if (axis.LengthSquared() < 1e-5f) return false;

            ProjectOBB(obb, axis, out float minO, out float maxO);
            ProjectAABB(aabb, axis, out float minA, out float maxA);

            return maxO < minA || maxA < minO;
        }

        // ---------- PROJECTIONS ----------
        // Proyección de una OBB sobre un eje (estática, reutilizable)
        private static void ProjectOBB(OrientedBoundingBox obb,
                                      Vector3 axis,
                                      out float min,
                                      out float max)
        {
            // Proyección del centro
            float centre = Vector3.Dot(obb.Center, axis);

            // Extensión total = Σ |HalfExtent_i * (Axis_i • axis)|
            float extent =
                Math.Abs(Vector3.Dot(obb.AxisX * obb.HalfExtents.X, axis)) +
                Math.Abs(Vector3.Dot(obb.AxisY * obb.HalfExtents.Y, axis)) +
                Math.Abs(Vector3.Dot(obb.AxisZ * obb.HalfExtents.Z, axis));

            min = centre - extent;
            max = centre + extent;
        }

        // Proyección de una AABB sobre un eje (estática, reutilizable)
        private static void ProjectAABB(BoundingBox aabb,
                                        Vector3 axis,
                                        out float min,
                                        out float max)
        {
            // Obtener los 8 vértices de la AABB
            Vector3[] corners = aabb.GetCorners();

            // Inicializar con el primer vértice
            min = max = Vector3.Dot(corners[0], axis);

            // Recorrer los restantes
            for (int i = 1; i < corners.Length; i++)
            {
                float proj = Vector3.Dot(corners[i], axis);
                if (proj < min) min = proj;
                else if (proj > max) max = proj;
            }
        }
    }
}
// using System;
// using Microsoft.Xna.Framework;

// namespace TGC.MonoGame.TP.Util;

// public struct OrientedBoundingBox
// {
//     public Vector3 Center;
//     public Vector3 HalfExtents;
//     public Vector3 AxisX;
//     public Vector3 AxisY;
//     public Vector3 AxisZ;

//     public OrientedBoundingBox(Vector3 localCenter, Vector3 localHalfExtents, Matrix world)
//     {
//         Center = Vector3.Transform(localCenter, world);

//         // Aislar la rotación y escala
//         Matrix rotationScale = world;
//         rotationScale.Translation = Vector3.Zero;

//         AxisX = rotationScale.Right;
//         AxisY = rotationScale.Up;
//         AxisZ = rotationScale.Forward;

//         // Las HalfExtents deben escalarse por la longitud (factor de escala) de sus respectivos ejes.
//         HalfExtents.X = AxisX.Length() * localHalfExtents.X;
//         HalfExtents.Y = AxisY.Length() * localHalfExtents.Y;
//         HalfExtents.Z = AxisZ.Length() * localHalfExtents.Z;

//         // Normalizar los ejes de la OBB (dejando la magnitud en HalfExtents)
//         AxisX.Normalize();
//         AxisY.Normalize();
//         AxisZ.Normalize();
//     }

//     /// <summary>
//     /// Devuelve true si esta OBB intersecta con <paramref name="other"/>.
//     /// Utiliza el Separating Axis Theorem (15 posibles ejes separadores).
//     /// </summary>
//     public bool Intersects(OrientedBoundingBox other)
//     {
//         // 3 ejes propios de esta OBB
//         if (IsSeparatingAxis(AxisX, this, other)) return false;
//         if (IsSeparatingAxis(AxisY, this, other)) return false;
//         if (IsSeparatingAxis(AxisZ, this, other)) return false;

//         // 3 ejes propios de la otra OBB
//         if (IsSeparatingAxis(other.AxisX, this, other)) return false;
//         if (IsSeparatingAxis(other.AxisY, this, other)) return false;
//         if (IsSeparatingAxis(other.AxisZ, this, other)) return false;

//         // 9 ejes cruzados
//         if (IsSeparatingAxis(Vector3.Cross(AxisX, other.AxisX), this, other)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisX, other.AxisY), this, other)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisX, other.AxisZ), this, other)) return false;

//         if (IsSeparatingAxis(Vector3.Cross(AxisY, other.AxisX), this, other)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisY, other.AxisY), this, other)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisY, other.AxisZ), this, other)) return false;

//         if (IsSeparatingAxis(Vector3.Cross(AxisZ, other.AxisX), this, other)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisZ, other.AxisY), this, other)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisZ, other.AxisZ), this, other)) return false;

//         // Ningún eje separó → colisión
//         return true;
//     }

//     /// <summary>
//     /// Determina si <paramref name="axis"/> separa a las dos OBB.
//     /// </summary>
//     private static bool IsSeparatingAxis(Vector3 axis,
//                                          OrientedBoundingBox a,
//                                          OrientedBoundingBox b)
//     {
//         // Un eje de longitud ≈ 0 no aporta información (producto cruzado paralelo)
//         if (axis.LengthSquared() < 1e-5f) return false;

//         // Proyección de la primera caja
//         ProjectOBB(a, axis, out float minA, out float maxA);
//         // Proyección de la segunda caja
//         ProjectOBB(b, axis, out float minB, out float maxB);

//         // Si los intervalos no se solapan → eje separador
//         return maxA < minB || maxB < minA;
//     }


//     /// <summary>
//     /// Proyección de una OBB sobre <paramref name="axis"/>.
//     /// Devuelve los valores mínimo y máximo de la proyección.
//     /// </summary>
//     private static void ProjectOBB(OrientedBoundingBox obb,
//                                    Vector3 axis,
//                                    out float min,
//                                    out float max)
//     {
//         // Proyección del centro
//         float centerProj = Vector3.Dot(obb.Center, axis);

//         // Extensión total de la OBB en ese eje:
//         // |e·axis| = Σ |HalfExtent_i * (Axis_i · axis)|
//         float extent =
//             Math.Abs(Vector3.Dot(obb.AxisX * obb.HalfExtents.X, axis)) +
//             Math.Abs(Vector3.Dot(obb.AxisY * obb.HalfExtents.Y, axis)) +
//             Math.Abs(Vector3.Dot(obb.AxisZ * obb.HalfExtents.Z, axis));

//         min = centerProj - extent;
//         max = centerProj + extent;
//     }


//     public bool Intersects(BoundingBox aabb)
//     {
//         // 3 Ejes de la OBB
//         if (IsSeparatingAxis(AxisX, aabb)) return false;
//         if (IsSeparatingAxis(AxisY, aabb)) return false;
//         if (IsSeparatingAxis(AxisZ, aabb)) return false;

//         // 3 Ejes de la AABB (Ejes del mundo: X, Y, Z)
//         if (IsSeparatingAxis(Vector3.Right, aabb)) return false;      // Eje X del mundo
//         if (IsSeparatingAxis(Vector3.Up, aabb)) return false;         // Eje Y del mundo
//         if (IsSeparatingAxis(Vector3.Backward, aabb)) return false;   // Eje Z del mundo

//         // 9 Ejes Cruzados (Producto Cruz OBB x AABB)
//         // Ejes AABB: Xw (Right), Yw (Up), Zw (Backward)

//         Vector3 Xw = Vector3.Right;
//         Vector3 Yw = Vector3.Up;
//         Vector3 Zw = Vector3.Backward;

//         // OBB AxisX x AABB axes
//         if (IsSeparatingAxis(Vector3.Cross(AxisX, Xw), aabb)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisX, Yw), aabb)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisX, Zw), aabb)) return false;

//         // OBB AxisY x AABB axes
//         if (IsSeparatingAxis(Vector3.Cross(AxisY, Xw), aabb)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisY, Yw), aabb)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisY, Zw), aabb)) return false;

//         // OBB AxisZ x AABB axes
//         if (IsSeparatingAxis(Vector3.Cross(AxisZ, Xw), aabb)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisZ, Yw), aabb)) return false;
//         if (IsSeparatingAxis(Vector3.Cross(AxisZ, Zw), aabb)) return false;

//         // Si NINGÚN eje separador fue encontrado, las cajas colisionan.
//         return true;
//     }

//     /// <summary>
//     /// Verifica si el eje dado es un eje separador (no hay solapamiento de proyecciones).
//     /// </summary>
//     private bool IsSeparatingAxis(Vector3 axis, BoundingBox aabb)
//     {
//         // Ignorar ejes con longitud cero (puede ser resultado del producto cruz en el mismo plano)
//         if (axis.LengthSquared() < 0.00001f) return false;

//         // Proyectar ambos objetos sobre el eje.
//         ProjectOBB(axis, out float minA, out float maxA); // Proyección de la OBB (this)
//         ProjectAABB(aabb, axis, out float minB, out float maxB); // Proyección de la AABB

//         // Comprobar si las proyecciones se superponen
//         // No hay colisión si (maxA < minB) o (maxB < minA)
//         return maxA < minB || maxB < minA;
//     }

//     /// <summary>
//     /// Proyecta la OBB sobre un eje y devuelve los valores mínimo y máximo.
//     /// </summary>
//     private void ProjectOBB(Vector3 axis, out float min, out float max)
//     {
//         // Proyectar el centro en el eje
//         float centerProjection = Vector3.Dot(Center, axis);

//         // Calcular la extensión de la OBB en ese eje
//         // Se usa el valor absoluto del producto punto del eje con cada eje local de la OBB,
//         // multiplicado por la HalfExtent de ese eje.
//         float extent =
//             Math.Abs(Vector3.Dot(HalfExtents.X * AxisX, axis)) +
//             Math.Abs(Vector3.Dot(HalfExtents.Y * AxisY, axis)) +
//             Math.Abs(Vector3.Dot(HalfExtents.Z * AxisZ, axis));

//         min = centerProjection - extent;
//         max = centerProjection + extent;
//     }

//     /// <summary>
//     /// Proyecta la AABB sobre un eje y devuelve los valores mínimo y máximo.
//     /// </summary>
//     private static void ProjectAABB(BoundingBox aabb, Vector3 axis, out float min, out float max)
//     {
//         // Obtener los 8 vértices de la AABB
//         Vector3[] corners = new Vector3[8];
//         aabb.GetCorners(corners);

//         // Proyectar el primer vértice para inicializar min/max
//         min = max = Vector3.Dot(corners[0], axis);

//         // Proyectar los 7 vértices restantes y actualizar min/max
//         for (int i = 1; i < 8; i++)
//         {
//             float projection = Vector3.Dot(corners[i], axis);

//             if (projection < min)
//                 min = projection;
//             else if (projection > max)
//                 max = projection;
//         }
//     }
// }