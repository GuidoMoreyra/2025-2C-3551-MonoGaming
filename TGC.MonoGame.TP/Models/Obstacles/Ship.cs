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
    internal class Ship
    {
        private Matrix _worldMatrix;
        private Model _model;
        private const float SCALE = 0.02f;
        public bool estaDestruido = false;
        private BoundingBox _boundingBox;
        private BoundingBox _worldBoundingBox;
        public BoundingBox BoundingBox => _worldBoundingBox;


        public Ship(ContentManager content, Matrix worldMatrix)
        {
            //Instancio model
            _model = Nave_1.GetModel(content);

            //Matriz de mundo
            var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(90));
            _worldMatrix = rotation * worldMatrix;

            _boundingBox = CalculateBoundingBox(_model);
            UpdateBoundingBoxWorld();
        }

        private BoundingBox CalculateBoundingBox(Model model)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var mesh in model.Meshes)
            {
                var transforms = mesh.ParentBone.Transform;
                foreach (var part in mesh.MeshParts)
                {
                    var vertexData = new VertexPositionNormalTexture[part.NumVertices];
                    part.VertexBuffer.GetData(vertexData);

                    foreach (var vertex in vertexData)
                    {
                        var transformed = Vector3.Transform(vertex.Position, transforms);
                        min = Vector3.Min(min, transformed);
                        max = Vector3.Max(max, transformed);
                    }
                }
            }

            return new BoundingBox(min, max);
        }

        private void UpdateBoundingBoxWorld(float reductionFactor = 0.6f)
        {
            // Aplica escala y mundo
            var scaleMatrix = Matrix.CreateScale(SCALE);
            var worldTransform = scaleMatrix * _worldMatrix;

            // Obtiene los 8 vértices del bounding box original
            var corners = _boundingBox.GetCorners();
            var transformedCorners = new Vector3[corners.Length];
            for (int i = 0; i < corners.Length; i++)
                transformedCorners[i] = Vector3.Transform(corners[i], worldTransform);

            // Calcula el centro real del bounding box transformado
            Vector3 center = Vector3.Zero;
            foreach (var v in transformedCorners)
                center += v;
            center /= transformedCorners.Length;

            // Reduce los vértices respecto al centro
            for (int i = 0; i < transformedCorners.Length; i++)
                transformedCorners[i] = center + (transformedCorners[i] - center) * reductionFactor;

            // Crea el bounding box final
            _worldBoundingBox = BoundingBox.CreateFromPoints(transformedCorners);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var scaleMatrix = Matrix.CreateScale(SCALE);
                var world = meshWorld * scaleMatrix * _worldMatrix;

                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.CurrentTechnique = effect.Techniques["MainTechnique"];
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
                var world = meshWorld * scaleMatrix * _worldMatrix;
                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.CurrentTechnique = effect.Techniques["Bloom"];
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(world)));

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
            UpdateBoundingBoxWorld();
        }

        public void Update(GameTime gameTime, PlayerShip player)
        {
            if (this.BoundingBox.Intersects(player.BoundingBox) && !player.tieneEscudo)
            {
                player.Destroy();
                Console.WriteLine("Caja");
            }
            foreach (var proyectil in player.proyectiles)
            {
                if (this.BoundingBox.Intersects(proyectil.BoundingBox))
                {
                    this.Destroy();
                    proyectil.Destroy(true);
                }
            }
        }
        public void Reset()
        {
        }
        public void Destroy()
        {
            estaDestruido = true;
        }
    }


}
