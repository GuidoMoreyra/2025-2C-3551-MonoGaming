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
        private const float SCALE = 0.05f;

        public bool estaDestruido = false;

        // ✅ BoundingBox
        // private BoundingBox _boundingBoxLocal;
        // private BoundingBox _boundingBoxWorld;
        // public BoundingBox BoundingBox => _boundingBoxWorld;

        private BoundingSphere _boundingSphereLocal;
        private BoundingSphere _boundingSphereWorld;
        public BoundingSphere BoundingSphere => _boundingSphereWorld;
        

        public Box(ContentManager content, Matrix worldMatrix, float angle)
        {
            _model = Caja_1.GetModel(content);

            _worldMatrix = worldMatrix;
            _rotation = Matrix.CreateRotationX(MathHelper.ToRadians(angle));

            _boundingSphereLocal = CalculateBoundingSphere(_model);
            UpdateBoundingSphereWorld();

            // ✅ Calcular bounding box local
            // _boundingBoxLocal = CalculateBoundingBox(_model);
            // UpdateBoundingBoxWorld();
        }

        private BoundingSphere CalculateBoundingSphere(Model model)
        {
            BoundingSphere mergedSphere = new BoundingSphere();
            bool first = true;

            foreach (var mesh in model.Meshes)
            {
                Matrix meshTransform = mesh.ParentBone.Transform;

                BoundingSphere transformedMeshSphere = mesh.BoundingSphere.Transform(meshTransform);

                if (first)
                {
                    mergedSphere = transformedMeshSphere;
                    first = false;
                }
                else
                {
                    mergedSphere = BoundingSphere.CreateMerged(mergedSphere, transformedMeshSphere);
                }
            }
            return mergedSphere;
        }

        // private BoundingBox CalculateBoundingBox(Model model)
        // {
        //     Vector3 min = new Vector3(float.MaxValue);
        //     Vector3 max = new Vector3(float.MinValue);

        //     foreach (var mesh in model.Meshes)
        //     {
        //         var meshTransform = mesh.ParentBone.Transform;
        //         foreach (var meshPart in mesh.MeshParts)
        //         {
        //             var vertexData = new VertexPositionNormalTexture[meshPart.NumVertices];
        //             meshPart.VertexBuffer.GetData(vertexData);

        //             foreach (var vertex in vertexData)
        //             {
        //                 var transformed = Vector3.Transform(vertex.Position, meshTransform);
        //                 min = Vector3.Min(min, transformed);
        //                 max = Vector3.Max(max, transformed);
        //             }
        //         }
        //     }

        //     return new BoundingBox(min, max);
        // }


        // private void UpdateBoundingBoxWorld(float reductionFactor = 0.6f)
        // {
        //     var scaleMatrix = Matrix.CreateScale(SCALE);
        //     var worldTransform = scaleMatrix * _worldMatrix;

        //     var corners = _boundingBoxLocal.GetCorners();
        //     var transformedCorners = new Vector3[corners.Length];
        //     for (int i = 0; i < corners.Length; i++)
        //         transformedCorners[i] = Vector3.Transform(corners[i], worldTransform);

        //     Vector3 center = Vector3.Zero;
        //     foreach (var v in transformedCorners)
        //         center += v;
        //     center /= transformedCorners.Length;

        //     for (int i = 0; i < transformedCorners.Length; i++)
        //         transformedCorners[i] = center + (transformedCorners[i] - center) * reductionFactor;

        //     _boundingBoxWorld = BoundingBox.CreateFromPoints(transformedCorners);
        // }

        private void UpdateBoundingSphereWorld()
        {
            var scaleMatrix = Matrix.CreateScale(SCALE);
            Matrix worldTransform = scaleMatrix * _rotation * _worldMatrix;

            _boundingSphereWorld = _boundingSphereLocal.Transform(worldTransform);
        }


        public void Draw(Matrix view, Matrix projection)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var scaleMatrix = Matrix.CreateScale(SCALE);
                var world = meshWorld * _rotation * scaleMatrix * _worldMatrix;

                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
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
            // Dibujar el wireframe de la esfera
            // Color debugColor = Color.Red; // Por ejemplo, rojo
            // BoundingSphereRenderer.Draw(_boundingSphereWorld, view, projection, debugColor);
        }

        // ✅ Si la caja se mueve, llamá esto con el nuevo worldMatrix
        public void SetWorldMatrix(Matrix newWorld)
        {
            _worldMatrix = newWorld;
            UpdateBoundingSphereWorld();
            // UpdateBoundingBoxWorld();
        }

        public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
        {
            if (BoundingSphere.Intersects(player.BoundingBox) && !player.tieneEscudo)
            {
                player.Destroy();
                Console.WriteLine("Caja");
            }
            foreach (var proyectil in player.proyectiles)
            {
                if (BoundingSphere.Intersects(proyectil.BoundingBox))
                {
                    Destroy();
                    proyectil.Destroy(true);
                }
            }
            // if (this.BoundingBox.Intersects(player.BoundingBox))
            // {
            //     player.Restart();
            //     Console.WriteLine("Caja");
            //     generator.GenerarEscenario(ref escenario);
            // }
            // foreach (var proyectil in player.proyectiles)
            // {
            //     if (this.BoundingBox.Intersects(proyectil.BoundingBox))
            //     {
            //         this.Destroy();
            //         proyectil.Destroy();
            //     }
            // }
        }

        public void Destroy()
        {
            estaDestruido = true;
        }
    }
}
