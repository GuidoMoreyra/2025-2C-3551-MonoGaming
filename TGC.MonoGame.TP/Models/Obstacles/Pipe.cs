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
    internal class PipeObs
    {
        private Matrix _worldMatrix;
        private Model _model;

        // Si la escala es uniforme, puedes usar un solo float.
        private readonly Matrix _scaleMatrix = Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.03f)) * Matrix.CreateRotationX(MathHelper.ToRadians(90));



        public bool estaDestruido = false;

        // CAMBIO: Usaremos BoundingBox local (AABB) para construir la OBB
        private BoundingBox _boundingBoxLocal;
        
        // CAMBIO: OBB para colisión mundial
        private OrientedBoundingBox _obbWorld;
        public OrientedBoundingBox OBB => _obbWorld; 


        public PipeObs()
        {
            _model = Pipe.GetModel();

            _boundingBoxLocal = CalculateBoundingBox(_model);

            UpdateOrientedBoundingBoxWorld();
        }


        private BoundingBox CalculateBoundingBox(Model model)
        {
            BoundingBox mergedBox = new BoundingBox();
            bool first = true;

            foreach (var mesh in model.Meshes)
            {
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

            Vector3 localCenter = (_boundingBoxLocal.Min + _boundingBoxLocal.Max) / 2.0f;
            Vector3 localHalfExtents = (_boundingBoxLocal.Max - _boundingBoxLocal.Min) / 2.0f;

            Matrix worldTransform = _scaleMatrix * _worldMatrix;

            _obbWorld = new OrientedBoundingBox(
                localCenter, 
                localHalfExtents, 
                worldTransform
            );
        }

        public void Draw(Matrix viewProjection, Vector3 cameraPosition)
        {
             Matrix _scaleMatrix2 = Matrix.CreateScale(new Vector3(0.01f, 0.01f, 0.03f)) * Matrix.CreateRotationX(MathHelper.ToRadians(90));
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;

                var world = meshWorld * _scaleMatrix2 * _worldMatrix; 
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
            UpdateOrientedBoundingBoxWorld(); 
        }

        public void Destroy()
        {
            estaDestruido = true;
        }


        public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator)
        {

            UpdateOrientedBoundingBoxWorld(); 

  
            if (OBB.Intersects(player.BoundingBox) && !player.tieneEscudo)
            {
                player.Destroy();
                Console.WriteLine("Colisión CargoShip vs Player");
            }
            
            foreach (var proyectil in player.proyectiles)
            {

                if (OBB.Intersects(proyectil.BoundingBox)) 
                {
                    this.Destroy();
                    proyectil.Destroy(true);
                }
            }
        }
    }
}