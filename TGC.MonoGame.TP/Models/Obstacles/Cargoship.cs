using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.Modules;
using TGC.MonoGame.TP.Util; // -> Aquí debe estar definida la estructura OrientedBoundingBox
using System;
using TGC.MonoGame.TP.Models.BaseModels;

namespace TGC.MonoGame.TP.Models.Obstacles
{
    internal class CargoShip
    {
        private Matrix _worldMatrix;
        private Model _model;

        // Si la escala es uniforme, puedes usar un solo float.
        private const float SCALE = 0.00075f; 
        private readonly Matrix _scaleMatrix = Matrix.CreateScale(SCALE);

        public bool estaDestruido = false;

        // CAMBIO: Usaremos BoundingBox local (AABB) para construir la OBB
        private BoundingBox _boundingBoxLocal;
        
        // CAMBIO: OBB para colisión mundial
        private OrientedBoundingBox _obbWorld;
        public OrientedBoundingBox OBB => _obbWorld; 


        public CargoShip(ContentManager content, Matrix worldMatrix)
        {
            // Modelo
            _model = Nave_2.GetModel(content);

            // Matriz de mundo
            _worldMatrix = worldMatrix;

            // ✅ Calcular BoundingBox local (AABB del modelo sin transformaciones)
            _boundingBoxLocal = CalculateBoundingBox(_model);
            
            // ✅ Inicializar la OBB
            UpdateOrientedBoundingBoxWorld();
        }

        // ----------------------------------------------------------------------
        // CÁLCULO INICIAL DE LA AABB LOCAL (SIN ESCALA NI ROTACIÓN)
        // ----------------------------------------------------------------------

        /// <summary>
        /// Calcula la BoundingBox (AABB) local que envuelve al modelo.
        /// </summary>
        private BoundingBox CalculateBoundingBox(Model model)
        {
            BoundingBox mergedBox = new BoundingBox();
            bool first = true;

            foreach (var mesh in model.Meshes)
            {
                // La BoundingSphere de la malla ya está en el espacio local del modelo,
                // pero a veces se necesita transformarla por el hueso padre.
                
                // Opción rápida (Aproximación AABB de la BoundingSphere):
                Vector3 min = mesh.BoundingSphere.Center - new Vector3(mesh.BoundingSphere.Radius);
                Vector3 max = mesh.BoundingSphere.Center + new Vector3(mesh.BoundingSphere.Radius);
                BoundingBox meshBox = new BoundingBox(min, max);

                if (first)
                {
                    mergedBox = meshBox;
                    first = false;
                }
                else
                {
                    mergedBox = BoundingBox.CreateMerged(mergedBox, meshBox);
                }
            }
            return mergedBox;
        }

        // ----------------------------------------------------------------------
        // ACTUALIZACIÓN DE LA OBB MUNDIAL
        // ----------------------------------------------------------------------

        /// <summary>
        /// Actualiza la OBB aplicando Rotación, Escala y Traslación.
        /// </summary>
        private void UpdateOrientedBoundingBoxWorld()
        {
            // 1. Calcular el centro y las medias extensiones (HalfExtents) de la AABB local
            Vector3 localCenter = (_boundingBoxLocal.Min + _boundingBoxLocal.Max) / 2.0f;
            Vector3 localHalfExtents = (_boundingBoxLocal.Max - _boundingBoxLocal.Min) / 2.0f;
            
            // 2. Crear la matriz de transformación COMPLETA
            // Orden: Escala * Rotación (si la hay) * Traslación
            
            // NOTA: Si _worldMatrix solo contiene traslación, la rotación debe aplicarse.
            // Aquí asumimos que la rotación ya está implícita en _worldMatrix o que no rota activamente.
            // Si el objeto NO ROTA, _worldMatrix es Traslación. Si rota, _worldMatrix es Rotación * Traslación.
            
            // Usaremos la matriz de mundo tal como se usa en Draw:
            Matrix worldTransform = _scaleMatrix * _worldMatrix;
            
            // 3. Crear la OBB usando el constructor
            _obbWorld = new OrientedBoundingBox(
                localCenter, 
                localHalfExtents, 
                worldTransform
            );
        }

        // ----------------------------------------------------------------------
        // MÉTODOS DRAW, SETWORLDMATRIX Y DESTROY (sin cambios funcionales)
        // ----------------------------------------------------------------------

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                // Usamos la matriz de escala precalculada (_scaleMatrix)
                var world = meshWorld * _scaleMatrix * _worldMatrix; 

                // ... (Draw logic) ...
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.CurrentTechnique = effect.Techniques["BasicColorDrawing"];
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["eyePosition"].SetValue(cameraPosition);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                    }
                }
                mesh.Draw();
            }
        }

        public void DrawBloom(Matrix view, Matrix projection)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var world = meshWorld * _scaleMatrix * _worldMatrix;

                // ... (DrawBloom logic) ...
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.CurrentTechnique = effect.Techniques["Bloom"];
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["World"].SetValue(world);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                    }
                }
                mesh.Draw();
            }
        }

        public void SetWorldMatrix(Matrix newWorld)
        {
            _worldMatrix = newWorld;
            // ✅ Actualizar la OBB cuando la matriz de mundo cambia
            UpdateOrientedBoundingBoxWorld(); 
        }

        public void Destroy()
        {
            estaDestruido = true;
        }

        // ----------------------------------------------------------------------
        // ACTUALIZACIÓN Y COLISIÓN
        // ----------------------------------------------------------------------

        public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
        {
            // ✅ Actualizar OBB (si el objeto se mueve, su matriz de mundo cambió antes de esta llamada)
            UpdateOrientedBoundingBoxWorld(); 

            // CAMBIO: Colisión OBB (CargoShip) vs AABB (PlayerShip)
            // Asumiendo que player.BoundingBox es una AABB.
            if (OBB.Intersects(player.BoundingBox) && !player.tieneEscudo)
            {
                player.Destroy();
                Console.WriteLine("Colisión CargoShip vs Player");
            }
            
            foreach (var proyectil in player.proyectiles)
            {
                // CAMBIO: Colisión OBB (CargoShip) vs AABB (Proyectil)
                if (OBB.Intersects(proyectil.BoundingBox)) 
                {
                    this.Destroy();
                    proyectil.Destroy(true);
                }
            }
        }
    }
}