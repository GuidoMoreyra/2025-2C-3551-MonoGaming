using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;

namespace TGC.MonoGame.TP.Models
{
    internal class Proyectil
    {
        private Matrix _worldMatrix;
        private const float SCALE = 0.02f;
        private const float VELOCIDAD = 1160f;

        public bool estaDestruido = false;
        private BoundingBox _boundingBoxLocal;
        private BoundingBox _boundingBoxWorld;
        public BoundingBox BoundingBox => _boundingBoxWorld;

        private Effect _effect;

        private Matrix objWM;
        private SoundEffect sonidoDisparo;
        private SoundEffect sonidoColision;

        private double tiempoCreacion = 0;

        public Proyectil(ContentManager content, Matrix worldMatrix, double tiempoCreacion, Matrix objWM)
        {
            _worldMatrix = worldMatrix;
            this.objWM = objWM;
            _effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShader").Clone();
            _effect.Parameters["DiffuseColor"]?.SetValue(Color.White.ToVector3());

            sonidoColision = content.Load<SoundEffect>(MonoGaming.ContentFolderSounds + "Explosion");

            sonidoDisparo = content.Load<SoundEffect>(MonoGaming.ContentFolderSounds + "ProyectilLaser");
            sonidoDisparo.Play();
            _boundingBoxLocal = ProyectilModel.GetBoundingBox();
            UpdateBoundingBoxWorld();

            this.tiempoCreacion = tiempoCreacion;
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

        private void UpdateBoundingBoxWorld(float reductionFactor = 1f)
        {
            // Aplica escala y mundo
            var scaleMatrix = Matrix.CreateScale(SCALE);
            var worldTransform = scaleMatrix * _worldMatrix;

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


        public void Draw(Matrix viewProjection)
        {
            _effect.Parameters["ViewProjection"].SetValue(viewProjection);
            _effect.Parameters["World"].SetValue(_worldMatrix);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }
            // Dibujar las primitivas
            _effect.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                PrimitiveType.TriangleList, // Dibujar triángulos (superficie sólida)
                ProyectilModel.GetVertices(new Color(_effect.Parameters["DiffuseColor"].GetValueVector3())),                   // Array de vértices
                0,                          // Offset de vértices
                ProyectilModel.GetVertices(new Color(_effect.Parameters["DiffuseColor"].GetValueVector3())).Length,            // Número de vértices
                ProyectilModel.GetIndices(),                    // Array de índices
                0,                          // Offset de índices
                ProyectilModel.GetIndices().Length / 3          // Número de primitivas (índices.Length / 3 = N° de triángulos)
            );

        }

        public void SetWorldMatrix(Matrix newWorld)
        {
            _worldMatrix = newWorld;
            UpdateBoundingBoxWorld();
        }


        public void Update(GameTime gameTime)
        {
            if ((gameTime.TotalGameTime.TotalSeconds - tiempoCreacion) > 2)
            {
                Destroy(false);
            }
            else
            {

                Vector3 targetPosition = objWM.Translation;
                Vector3 projectilePosition = _worldMatrix.Translation; 
                Vector3 direction = targetPosition - projectilePosition;
                Vector3 versor = Vector3.Normalize(direction);
                var nuevoMovimiento = versor * 4 * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;
                _worldMatrix = _worldMatrix * Matrix.CreateTranslation(nuevoMovimiento);
                UpdateBoundingBoxWorld();
            }
        }

        public void Destroy(bool fueColision)
        {
            if (fueColision)
            {
                sonidoColision.Play(0.5f, 0, 0);
            }
            estaDestruido = true;
        }

    }
}
