using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.TP.Models.BaseModels;


namespace TGC.MonoGame.TP.Models.Obstacles
{
    internal class CargoShip
    {
        private const float SCALE = 0.0005f;

        private const float ROTACION_MIN = 0f;
        private const float ROTACION_MAX = 45f;

        private const float VELOCIDAD_ROTACION_MIN = 60f;
        private const float VELOCIDAD_ROTACION_MAX = 90f;

        private readonly Model _model;
        private readonly Matrix _scaleMatrix;
        private readonly Random random;
        private Matrix _worldMatrix;
        private float _velocidadDeRotacion;
        private float _rotacionX;
        private bool _estaDestruido;
        private BoundingBox _boundingBoxLocal;
        private BoundingBox _boundingBoxWorld;
        public BoundingBox BoundingBox => _boundingBoxWorld;

        public CargoShip()
        {
            _model = Nave_2.GetModel();

            _scaleMatrix = Matrix.CreateScale(SCALE);

            random = new();

            _estaDestruido = false;

            _boundingBoxLocal = CalculateBoundingBox(_model);
        }

        public void SetWorldMatrix(Matrix worldMatrix)
        {
            var rotacionY = Matrix.CreateRotationY(MathHelper.ToRadians(random.NextSingle() * (ROTACION_MAX - ROTACION_MIN) + ROTACION_MIN));

            _worldMatrix = rotacionY * _scaleMatrix * worldMatrix;

            _estaDestruido = false;

            //Calculo rotacion
            _rotacionX = random.NextSingle() * (ROTACION_MAX - ROTACION_MIN) + ROTACION_MIN;
            _velocidadDeRotacion = random.NextSingle() * (VELOCIDAD_ROTACION_MAX - VELOCIDAD_ROTACION_MIN) + VELOCIDAD_ROTACION_MIN;

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
            var rotationXMat = Matrix.CreateRotationX(MathHelper.ToRadians(_rotacionX));
            var worldTransform = rotationXMat * _worldMatrix;

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

        public void Draw(Matrix viewProjection, Vector3 cameraPosition)
        {
            if (!_estaDestruido)
            {
                var rotationXMat = Matrix.CreateRotationX(MathHelper.ToRadians(_rotacionX));

                foreach (var mesh in _model.Meshes)
                {
                    var meshWorld = mesh.ParentBone.Transform;
                    var world = meshWorld * rotationXMat * _worldMatrix;

                    foreach (var meshPart in mesh.MeshParts)
                    {
                        var effect = meshPart.Effect;
                        effect.CurrentTechnique = effect.Techniques["BasicColorDrawing"];
                        effect.Parameters["ViewProjection"].SetValue(viewProjection);
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
        }

        public void DrawBloom(Matrix viewProjection)
        {
            if (!_estaDestruido)
            {
                var rotationXMat = Matrix.CreateRotationX(MathHelper.ToRadians(_rotacionX));

                foreach (var mesh in _model.Meshes)
                {
                    var meshWorld = mesh.ParentBone.Transform;
                    var world = meshWorld * rotationXMat * _worldMatrix;
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        var effect = meshPart.Effect;
                        effect.CurrentTechnique = effect.Techniques["Bloom"];
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
        }

        public void Update(GameTime gameTime, PlayerShip player)
        {
            if (!_estaDestruido)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _rotacionX += _velocidadDeRotacion * deltaTime;

                if (BoundingBox.Intersects(player.BoundingBox) && !player.tieneEscudo)
                {
                    player.Destroy();
                }
                foreach (var proyectil in player.proyectiles)
                {
                    if (BoundingBox.Intersects(proyectil.BoundingBox))
                    {
                        Destroy();
                        proyectil.Destroy(true);
                    }
                }

                UpdateBoundingBoxWorld();
            }
        }

        public void Destroy()
        {
            _estaDestruido = true;
        }
    }
}
