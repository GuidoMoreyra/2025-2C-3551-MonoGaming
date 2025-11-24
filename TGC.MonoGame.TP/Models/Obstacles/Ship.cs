using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Models.Modules.Contract;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Obstacles
{
    internal class Ship
    {
        private const float SCALE = 0.02f;
        private const float MODEL_ROTATION = 90f;

        private readonly Model _model;
        private readonly Matrix _rotationScaleMatrix;
        private bool _estaDestruido = false;
        private Matrix _worldMatrix;
        private BoundingBox _boundingBox;
        private BoundingBox _worldBoundingBox;
        public BoundingBox BoundingBox => _worldBoundingBox;

        public Ship()
        {
            _estaDestruido = false;

            _model = Nave_1.GetModel();

            _rotationScaleMatrix = Matrix.CreateScale(SCALE) * Matrix.CreateRotationY(MathHelper.ToRadians(MODEL_ROTATION));

            _boundingBox = CalculateBoundingBox(_model);
        }

        public void SetWorldMatrix(Matrix worldMatrix)
        {
            _estaDestruido = false;

            _worldMatrix = _rotationScaleMatrix * worldMatrix;

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
            // Obtiene los 8 vértices del bounding box original
            var corners = _boundingBox.GetCorners();
            var transformedCorners = new Vector3[corners.Length];
            for (int i = 0; i < corners.Length; i++)
                transformedCorners[i] = Vector3.Transform(corners[i], _worldMatrix);

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

        public void Draw(Matrix viewProjection)
        {
            if (!_estaDestruido)
            {
                foreach (var mesh in _model.Meshes)
                {
                    var meshWorld = mesh.ParentBone.Transform;
                    var world = meshWorld * _worldMatrix;
                    var inverseTrasposeWorld = Matrix.Transpose(Matrix.Invert(world));

                    foreach (var meshPart in mesh.MeshParts)
                    {
                        var effect = meshPart.Effect;
                        effect.CurrentTechnique = effect.Techniques["MainTechnique"];
                        effect.Parameters["ViewProjection"].SetValue(viewProjection);
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["InverseTransposeWorld"].SetValue(inverseTrasposeWorld);

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
                foreach (var mesh in _model.Meshes)
                {
                    var meshWorld = mesh.ParentBone.Transform;
                    var world = meshWorld * _worldMatrix;
                    var inverseTrasposeWorld = Matrix.Transpose(Matrix.Invert(world));
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        var effect = meshPart.Effect;
                        effect.CurrentTechnique = effect.Techniques["Bloom"];
                        effect.Parameters["ViewProjection"].SetValue(viewProjection);
                        effect.Parameters["World"].SetValue(world);
                        effect.Parameters["InverseTransposeWorld"].SetValue(inverseTrasposeWorld);

                        foreach (var pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                        }
                    }
                    mesh.Draw();
                }
            }
        }

        public void Update(PlayerShip player)
        {
            if (!_estaDestruido)
            {
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
            }
        }

        public void Destroy()
        {
            _estaDestruido = true;
        }
    }
}
