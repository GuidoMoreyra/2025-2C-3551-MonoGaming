using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.Modules;
using TGC.MonoGame.TP.Util;
using System;
using TGC.MonoGame.TP.Models.BaseModels;


namespace TGC.MonoGame.TP.Models.Obstacles
{
    internal class CargoShip
    {
        private Matrix _worldMatrix;
        private Model _model;

        private const float SCALE = 0.0005f;

        private const float ROTACION_MIN = 0f;
        private const float ROTACION_MAX = 45f;

        private const float VELOCIDAD_ROTACION_MIN = 60f;
        private const float VELOCIDAD_ROTACION_MAX = 90f;

        private float velocidadDeRotacion;

        private static Random random = new Random();
        private float rotacionX;
        private float rotacionY;

        public bool estaDestruido = false;

        // ✅ BoundingBox
        private BoundingBox _boundingBoxLocal;
        private BoundingBox _boundingBoxWorld;
        public BoundingBox BoundingBox => _boundingBoxWorld;

        public CargoShip(ContentManager content, Matrix worldMatrix)
        {
            //Modelo
            _model = Nave_2.GetModel(content);

            //Calculo rotacion
            rotacionY = random.NextSingle() * (ROTACION_MAX - ROTACION_MIN) + ROTACION_MIN;
            rotacionX = random.NextSingle() * (ROTACION_MAX - ROTACION_MIN) + ROTACION_MIN;
            velocidadDeRotacion = random.NextSingle() * (VELOCIDAD_ROTACION_MAX - VELOCIDAD_ROTACION_MIN) + VELOCIDAD_ROTACION_MIN;

            //Matriz de mundo
            _worldMatrix = worldMatrix;

            // ✅ Calcular BoundingBox local
            _boundingBoxLocal = CalculateBoundingBox(_model);
            UpdateBoundingBoxWorld();
        }

        private BoundingBox CalculateBoundingBox(Model model)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var mesh in model.Meshes)
            {
                var meshTransform = mesh.ParentBone.Transform;
                foreach (var meshPart in mesh.MeshParts)
                {
                    var vertexData = new VertexPositionNormalTexture[meshPart.NumVertices];
                    meshPart.VertexBuffer.GetData(vertexData);

                    foreach (var vertex in vertexData)
                    {
                        var transformed = Vector3.Transform(vertex.Position, meshTransform);
                        min = Vector3.Min(min, transformed);
                        max = Vector3.Max(max, transformed);
                    }
                }
            }

            return new BoundingBox(min, max);
        }

        private void UpdateBoundingBoxWorld(float reductionFactor = 0.6f)
        {
            var rotationXMat = Matrix.CreateRotationX(MathHelper.ToRadians(rotacionX));
            var rotationYMat = Matrix.CreateRotationY(MathHelper.ToRadians(rotacionY));
            var scaleMatrix = Matrix.CreateScale(SCALE);
            var worldTransform = rotationYMat * rotationXMat * scaleMatrix * _worldMatrix;

            // Obtiene los 8 vértices del bounding box original
            var corners = _boundingBoxLocal.GetCorners();
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
            _boundingBoxWorld = BoundingBox.CreateFromPoints(transformedCorners);
        }

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            var rotationXMat = Matrix.CreateRotationX(MathHelper.ToRadians(rotacionX));
            var rotationYMat = Matrix.CreateRotationY(MathHelper.ToRadians(rotacionY));

            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var scaleMatrix = Matrix.CreateScale(SCALE);
                var world = meshWorld * rotationYMat * rotationXMat * scaleMatrix * _worldMatrix;

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
        }

        public void Update(GameTime gameTime, PlayerShip player, EscenarioGenerator generator, ref List<IModule> escenario)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            rotacionX += velocidadDeRotacion * deltaTime;

            if (this.BoundingBox.Intersects(player.BoundingBox))
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


            UpdateBoundingBoxWorld();
        }

        // Si en algún momento movés el objeto, también actualizarías _worldMatrix y llamás UpdateBoundingBoxWorld()
        public void SetWorldMatrix(Matrix newWorld)
        {
            _worldMatrix = newWorld;
            UpdateBoundingBoxWorld();
        }


        public void Destroy()
        {
            estaDestruido = true;
        }
    }
}
