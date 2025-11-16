using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Models.BaseModels;


namespace TGC.MonoGame.TP.Models
{
    internal class PlayerShip
    {
        private Matrix _worldMatrix;
        private Model _model;

        private int cantidad_de_balas = 10;
        public List<Proyectil> proyectiles = new List<Proyectil>();

        private float tiempoAcumulado = 0f;
        private const float VELOCIDAD = 28.25F;
        private const float VELOCIDAD_DE_GIRO = 180F;
        private const float TIME_BETWEEN_SHOTS = 0.5f;
        private const float SCALE = 0.01f;
        private const float ALTURA_MIN = -5f;
        private const float ALTURA_MAX = 10f;

        private const float DISTANCIA_MIN = -15f;
        private const float DISTANCIA_MAX = 15f;

        private bool estaDestruido = false;


        private float angulo = 0;

        private BoundingBox _boundingBox;
        private BoundingBox _worldBoundingBox;

        public BoundingBox BoundingBox => _worldBoundingBox;

        private SoundEffect sonidoColision;


        public PlayerShip(ContentManager content)
        {
            sonidoColision = content.Load<SoundEffect>(MonoGaming.ContentFolderSounds + "ExplosionJugador");

            //Recupero el modelo con las texturas
            _model = Nave_1.GetModel(content);

            //Creo la matriz de mundo inicial
            var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(-90));
            _worldMatrix = rotation * Matrix.Identity;

            //Creo al bounding box
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

        private void UpdateBoundingBoxWorld()
        {
            var scaleMatrix = Matrix.CreateScale(SCALE);
            var worldTransform = scaleMatrix * _worldMatrix;

            var corners = _boundingBox.GetCorners();
            var transformedCorners = new Vector3[corners.Length];
            for (int i = 0; i < corners.Length; i++)
                transformedCorners[i] = Vector3.Transform(corners[i], worldTransform);

            // Reducir bounding box a un porcentaje del tamaño original
            Vector3 center = (BoundingBox.Min + BoundingBox.Max) / 2f;
            for (int i = 0; i < transformedCorners.Length; i++)
                transformedCorners[i] = center + (transformedCorners[i] - center) * 0.6f; // 60% del tamaño

            _worldBoundingBox = BoundingBox.CreateFromPoints(transformedCorners);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            foreach (var mesh in _model.Meshes)
            {
                var rotacionJugador = Matrix.CreateRotationZ(MathHelper.ToRadians(angulo));
                var meshWorld = mesh.ParentBone.Transform;
                var scaleMatrix = Matrix.CreateScale(SCALE);
                var world = meshWorld * rotacionJugador * scaleMatrix * _worldMatrix;

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
            foreach (var proyectil in proyectiles)
            {
                proyectil.Draw(view, projection);
            }
        }

        public void SetWorldMatrix(Matrix newWorld)
        {
            _worldMatrix = newWorld;
            UpdateBoundingBoxWorld();
        }


        public void UpdateCamera(GameTime gameTime, ref Matrix view, ref Matrix projection)
        {
            Vector3 cameraOffset = Vector3.Right * 60 + Vector3.Up * 7f;

            Vector3 targetPosition = (BoundingBox.Min + BoundingBox.Max) / 2f;
            Vector3 cameraPosition = targetPosition + cameraOffset;

            // Limitar altura de la cámara
            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, ALTURA_MIN, ALTURA_MAX);

            Vector3 upVector = Vector3.Up;
            view = Matrix.CreateLookAt(cameraPosition, targetPosition, upVector);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 16f / 9f, 0.1f, 1000f);
        }

        public void Update(GameTime gameTime, ref Matrix view, ref Matrix projection, ContentManager content)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Vector3 nuevoMovimiento = Vector3.Zero;

            nuevoMovimiento += Vector3.Left * 4 * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;

            proyectiles.RemoveAll(o => o.estaDestruido);

            tiempoAcumulado += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (cantidad_de_balas > 0 && tiempoAcumulado >= TIME_BETWEEN_SHOTS)
            {
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    cantidad_de_balas--;
                    tiempoAcumulado = 0f;
                    proyectiles.Add(new Proyectil(content, _worldMatrix, gameTime.TotalGameTime.TotalSeconds));
                }

            }



            if (keyboardState.IsKeyDown(Keys.Left))
                angulo -= VELOCIDAD_DE_GIRO * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.Right))
                angulo += VELOCIDAD_DE_GIRO * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.A))
                nuevoMovimiento += Vector3.Backward * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.W))
                nuevoMovimiento += Vector3.Up * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.S))
                nuevoMovimiento += Vector3.Down * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.D))
                nuevoMovimiento += Vector3.Forward * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;



            var traslacion = Matrix.CreateTranslation(nuevoMovimiento);
            _worldMatrix = _worldMatrix * traslacion;

            Vector3 pos = _worldMatrix.Translation;
            pos.Y = MathHelper.Clamp(pos.Y, ALTURA_MIN, ALTURA_MAX);
            pos.Z = MathHelper.Clamp(pos.Z, DISTANCIA_MIN, DISTANCIA_MAX);
            _worldMatrix.Translation = pos;

            foreach (Proyectil proyectil in proyectiles)
            {
                proyectil.Update(gameTime);
            }
            UpdateBoundingBoxWorld();
            UpdateCamera(gameTime, ref view, ref projection);
        }

        public void Destroy()
        {
            sonidoColision.Play();
            estaDestruido = true;
        }

        public bool EstaDestruido()
        {
            return estaDestruido;
        }

        public void Restart()
        {
            estaDestruido = false;
            cantidad_de_balas = 10;
            var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(-90));
            _worldMatrix = rotation * Matrix.Identity;
        }
    }
}
