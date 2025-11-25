using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Obstacles
{
    internal class Box
    {
        private readonly Model _model;
        private Matrix _rotationScaleMatrix;
        private Matrix _worldMatrix;
        private bool _estaDestruido;
        private BoundingBox _boundingBoxLocal;
        private OrientedBoundingBox _obbWorld;
        public OrientedBoundingBox OBB => _obbWorld;
        public Box(float angle, float y, float z)
        {
            _model = Caja_1.GetModel();
            _estaDestruido = false;

            _rotationScaleMatrix = Matrix.CreateScale(new Vector3(0.03f, y, z)) * Matrix.CreateRotationX(MathHelper.ToRadians(angle));

            _boundingBoxLocal = Utils.CalculateBoundingBox(_model);
        }

        public void SetWorldMatrix(Matrix worldMatrix)
        {
            _estaDestruido = false;
            _worldMatrix = _rotationScaleMatrix * worldMatrix;

            UpdateOrientedBoundingBoxWorld();
        }


        private void UpdateOrientedBoundingBoxWorld()
        {
            // 1. Calcular el centro y las medias extensiones (HalfExtents) de la AABB local
            Vector3 localCenter = (_boundingBoxLocal.Min + _boundingBoxLocal.Max) / 2.0f;
            Vector3 localHalfExtents = (_boundingBoxLocal.Max - _boundingBoxLocal.Min) / 2.0f;

            // 3. Crear la OBB usando el constructor
            _obbWorld = new OrientedBoundingBox(
                localCenter,
                localHalfExtents,
                _worldMatrix // Matriz de Escala, Rotación y Traslación.
            );

            // ¡La OBB se está creando aquí! La variable _obbWorld es inicializada
            // con los ejes y centro correctos basados en worldTransform.
        }


        public void Draw(Matrix viewProjection)
        {
            if (!_estaDestruido)
            {
                foreach (var mesh in _model.Meshes)
                {
                    var meshWorld = mesh.ParentBone.Transform;
                    var world = meshWorld * _worldMatrix;

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
        }

        public void DrawBloom(Matrix viewProjection)
        {
            if (!_estaDestruido)
            {
                foreach (var mesh in _model.Meshes)
                {
                    var meshWorld = mesh.ParentBone.Transform;
                    var world = meshWorld * _worldMatrix;
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

        public void Update(PlayerShip player)
        {
            if (!_estaDestruido)
            {
                UpdateOrientedBoundingBoxWorld();
                if (OBB.Intersects(player.BoundingBox) && !player.tieneEscudo) 
                {
                    player.Destroy();
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
        }

        public void Destroy()
        {
            _estaDestruido = true;
        }
    }
}
