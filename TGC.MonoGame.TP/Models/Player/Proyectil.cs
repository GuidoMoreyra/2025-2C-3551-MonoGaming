using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.BaseModels;

namespace TGC.MonoGame.TP.Models.Player
{
    internal class Proyectil
    {
        private const float SCALE = 2f;
        private const float VELOCIDAD = 150f;

        private readonly Effect _effect;
        private readonly SoundEffect sonidoDisparo;
        private readonly SoundEffect sonidoColision;
        private readonly Matrix _scaleMatrix;
        private Vector3 _position;
        private float tiempoCreacion;
        private Matrix _objetivoWorldMatrix;
        private Matrix _worldMatrix;
        private bool _estaDestruido;
        private BoundingBox _boundingBoxLocal;
        private BoundingBox _boundingBoxWorld;
        public BoundingBox BoundingBox => _boundingBoxWorld;


        public Proyectil(ContentManager content)
        {
            tiempoCreacion = 0.0f;
            _estaDestruido = true;
            _position = Vector3.Zero;

            _scaleMatrix = Matrix.CreateScale(SCALE);

            _effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + "BasicShader").Clone();
            _effect.Parameters["DiffuseColor"]?.SetValue(Color.White.ToVector3());

            sonidoColision = content.Load<SoundEffect>(MonoGaming.ContentFolderSounds + "Explosion");
            sonidoDisparo = content.Load<SoundEffect>(MonoGaming.ContentFolderSounds + "ProyectilLaser");

            _boundingBoxLocal = ProyectilModel.GetBoundingBox();
        }

        public void SetWorldMatrix(Vector3 position, Matrix objetivoWorldMatrix, GameTime gameTime)
        {
            tiempoCreacion = (float)gameTime.TotalGameTime.TotalSeconds;
            _estaDestruido = false;
            _objetivoWorldMatrix = objetivoWorldMatrix;
            _position = position;

            sonidoDisparo.Play();

            UpdateBoundingBoxWorld();
        }

        private void UpdateBoundingBoxWorld()
        {
            float reductionFactor = 1f;

            // Obtiene los 8 vértices del bounding box original
            var corners = _boundingBoxLocal.GetCorners();
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
            _boundingBoxWorld = BoundingBox.CreateFromPoints(transformedCorners);
        }


        public void Draw(Matrix viewProjection)
        {
            if (!_estaDestruido)
            {
                _effect.CurrentTechnique = _effect.Techniques["BasicColorDrawing"];
                _effect.Parameters["ViewProjection"].SetValue(viewProjection);
                _effect.Parameters["World"].SetValue(_worldMatrix);

                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
                // Dibujar las primitivas
                _effect.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, // Dibujar triángulos (superficie sólida)
                    ProyectilModel.GetVertices(new Color(_effect.Parameters["DiffuseColor"].GetValueVector3())),                   // Array de vértices
                    0,                          // Offset de vértices
                    ProyectilModel.GetVertices(new Color(_effect.Parameters["DiffuseColor"].GetValueVector3())).Length,            // Número de vértices
                    ProyectilModel.GetIndices(),                    // Array de índices
                    0,                          // Offset de índices
                    ProyectilModel.GetIndices().Length / 3          // Número de primitivas (índices.Length / 3 = N° de triángulos)
                );
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!_estaDestruido)
            {
                if ((gameTime.TotalGameTime.TotalSeconds - tiempoCreacion) > 2)
                {
                    Destroy(false);
                }
                else
                {
                    Vector3 targetPosition = _objetivoWorldMatrix.Translation;
                    Vector3 direction = targetPosition - _position;
                    Vector3 versor = Vector3.Normalize(direction);

                    var nuevoMovimiento = versor * Math.Min(4 * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds, Vector3.Distance(targetPosition, _position));
                    _position += nuevoMovimiento;

                    _worldMatrix =
                        _scaleMatrix *
                        Matrix.CreateTranslation(_position);
                    UpdateBoundingBoxWorld();
                }
            }
        }

        public bool EstaDestruido()
        {
            return _estaDestruido;
        }

        public void Destroy(bool fueColision)
        {
            if (fueColision)
            {
                sonidoColision.Play(1.0f, 0.0f, 0.0f);
            }
            _estaDestruido = true;
            _position = Vector3.Zero;
            _worldMatrix =
                _scaleMatrix *
                Matrix.CreateTranslation(_position);
            UpdateBoundingBoxWorld();
        }

    }
}
