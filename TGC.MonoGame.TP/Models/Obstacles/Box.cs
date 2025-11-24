using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Util;

namespace TGC.MonoGame.TP.Models.Obstacles
{
    internal class Box
    {
        private const float SCALE = 0.05f;

        private readonly Model _model;
        private Matrix _scaleMatrix;
        private Matrix _worldMatrix;
        private bool _estaDestruido;
        private BoundingSphere _boundingSphereLocal;
        private BoundingSphere _boundingSphereWorld;
        public BoundingSphere BoundingSphere => _boundingSphereWorld;

        public Box()
        {
            _model = Caja_1.GetModel();
            _estaDestruido = false;

            _scaleMatrix = Matrix.CreateScale(SCALE);

            _boundingSphereLocal = CalculateBoundingSphere(_model);
        }

        public void SetWorldMatrix(Matrix worldMatrix)
        {
            _estaDestruido = false;
            _worldMatrix = _scaleMatrix * Matrix.CreateRotationX(MathHelper.ToRadians(Utils.GenerateNumber(90))) * worldMatrix;

            UpdateBoundingSphereWorld();
        }


        private BoundingSphere CalculateBoundingSphere(Model model)
        {
            BoundingSphere mergedSphere = new();
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

        private void UpdateBoundingSphereWorld()
        {
            _boundingSphereWorld = _boundingSphereLocal.Transform(_worldMatrix);
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

        //TODO Revisar colisiones
        public void Update(PlayerShip player)
        {
            if (!_estaDestruido)
            {
                if (BoundingSphere.Intersects(player.BoundingBox) && !player.tieneEscudo)
                {
                    player.Destroy();
                }
                foreach (var proyectil in player.proyectiles)
                {
                    if (BoundingSphere.Intersects(proyectil.BoundingBox))
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
