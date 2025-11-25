using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Modules;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Obstacles
{
    internal class Box
    {
        private Matrix _worldMatrix;
        private Model _model;
        private Matrix _rotation;

        public bool estaDestruido = false;
        
        private readonly Vector3 SCALE ; 

  
        private BoundingBox _boundingBoxLocal;
   
        private OrientedBoundingBox _obbWorld;
        public OrientedBoundingBox OBB => _obbWorld; 


        public Box(ContentManager content, Matrix worldMatrix, float angle , float y , float z)
        {
            _model = Caja_1.GetModel(content);

            _worldMatrix = worldMatrix;

            _rotation = Matrix.CreateRotationX(MathHelper.ToRadians(angle));

            _boundingBoxLocal = CalculateBoundingBox(_model);

            SCALE = new Vector3(0.03f, y, z); 

            
            UpdateOrientedBoundingBoxWorld(); 
        }

        private BoundingBox CalculateBoundingBox(Model model)
        {
            BoundingBox mergedBox = new BoundingBox();
            bool first = true;

            foreach (var mesh in model.Meshes)
            {
                Matrix meshTransform = mesh.ParentBone.Transform;
                         
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
        private void UpdateOrientedBoundingBoxWorld()
        {
            // 1. Calcular el centro y las medias extensiones (HalfExtents) de la AABB local
            Vector3 localCenter = (_boundingBoxLocal.Min + _boundingBoxLocal.Max) / 2.0f;
            Vector3 localHalfExtents = (_boundingBoxLocal.Max - _boundingBoxLocal.Min) / 2.0f;

            // 2. Crear la matriz de transformación final (World Matrix)
            // El orden correcto de las transformaciones es:
            Matrix scaleMatrix = Matrix.CreateScale(SCALE);
            
            // Esta matriz worldTransform es la matriz de mundo COMPLETA
            Matrix worldTransform = scaleMatrix * _rotation * _worldMatrix; 
            
            // NOTA: Si _worldMatrix ya contiene rotación o escala de la creación del escenario,
            // esto es un riesgo de doble transformación. Asumiremos que _worldMatrix es SOLO traslación.

            // 3. Crear la OBB usando el constructor
            _obbWorld = new OrientedBoundingBox(
                localCenter, 
                localHalfExtents, 
                worldTransform // Matriz de Escala, Rotación y Traslación.
            );
            
            // ¡La OBB se está creando aquí! La variable _obbWorld es inicializada
            // con los ejes y centro correctos basados en worldTransform.
        }



        public void Draw(Matrix view, Matrix projection)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var scaleMatrix = Matrix.CreateScale(SCALE);

                var world = meshWorld * scaleMatrix * _rotation * _worldMatrix; 

                foreach (var meshPart in mesh.MeshParts)
                {

                    var effect = meshPart.Effect;
                    effect.CurrentTechnique = effect.Techniques["BasicColorDrawing"];
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

        public void DrawBloom(Matrix view, Matrix projection)
        {

            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var scaleMatrix = Matrix.CreateScale(SCALE);
                var world = meshWorld * scaleMatrix * _rotation * _worldMatrix;

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
            UpdateOrientedBoundingBoxWorld(); 
        }

        public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
        {

            if (OBB.Intersects(player.BoundingBox) && !player.tieneEscudo) 
            {
                player.Destroy();
                Console.WriteLine("Caja");
            }
            
            foreach (var proyectil in player.proyectiles)
            {
                if (OBB.Intersects(proyectil.BoundingBox)) 
                {
                    Destroy();
                    proyectil.Destroy(true);
                }
            }

        }

        public void Destroy()
        {
            estaDestruido = true;
        }
    }
}