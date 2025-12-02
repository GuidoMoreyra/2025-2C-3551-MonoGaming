using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Models.Obstacles;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Models.BaseModels;
using TGC.MonoGame.TP.Util;


namespace TGC.MonoGame.TP.Models.Player
{
    internal class PlayerShip
    {
        private const float VELOCIDAD = 28.25f;
        private const float VELOCIDAD_DE_GIRO = 180f;
        private const float TIME_BETWEEN_SHOTS = 0.5f;
        private const float SCALE = 0.01f;
        private const float ALTURA_MIN = -5f;
        private const float ALTURA_MAX = 10f;
        private const float DURACION_ESCUDO = 3f;
        private const float DISTANCIA_MIN = -15f;
        private const float DISTANCIA_MAX = 15f;
        private const float ROTACION_INICIAL = -90.0f;

        private readonly Model _model;
        private readonly Escudo _escudo;
        public readonly Proyectil _proyectil;
        private readonly SoundEffect _sonidoColision;
        private readonly Matrix _scaleMatrix;
        public Vector3 Position { get; set; }
        private bool _toogleGodModeActive;
        private bool _godMode;
        private Matrix _worldMatrix;
        public List<DestroyableBox> _objetivos;
        private float _tiempoAcumuladoEscudo;
        private float _tiempoAcumuladoProyectil;
        public bool _tieneEscudo;
        private bool _estaDestruido;
        private float _distanciaRecorrida;
        private float _angulo;
        private BoundingBox _boundingBoxLocal;
        private OrientedBoundingBox _obbWorld;
        public OrientedBoundingBox BoundingBox => _obbWorld;


        public PlayerShip(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _tiempoAcumuladoEscudo = 0.0f;
            _tiempoAcumuladoProyectil = 0.0f;
            _tieneEscudo = true;
            _estaDestruido = false;
            _distanciaRecorrida = 0.0f;
            _angulo = 0.0f;
            _proyectil = new Proyectil(content);
            _godMode = false;
            _toogleGodModeActive = true;
            _escudo = new Escudo(graphicsDevice);
            Position = Vector3.Zero;
            _sonidoColision = content.Load<SoundEffect>(MonoGaming.ContentFolderSounds + "ExplosionJugador");
            _scaleMatrix = Matrix.CreateScale(SCALE);

            //Recupero el modelo con las texturas
            _model = Nave_1.GetModel();

            //Creo la matriz de mundo inicial
            var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(ROTACION_INICIAL));
            _worldMatrix = rotation * _scaleMatrix;

            //Creo al bounding box
            _boundingBoxLocal = Utils.CalculateBoundingBox(_model);
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
        }

        public void Draw(Matrix viewProjection, Vector3 CameraPosition, Vector3 LightPosition, GraphicsDevice graphicsDevice)
        {
            foreach (var mesh in _model.Meshes)
            {
                var meshWorld = mesh.ParentBone.Transform;
                var world = meshWorld * _worldMatrix;

                foreach (var meshPart in mesh.MeshParts)
                {
                    var effect = meshPart.Effect;
                    effect.CurrentTechnique = effect.Techniques["MainTechnique"];
                    effect.Parameters["ViewProjection"].SetValue(viewProjection);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
                    effect.Parameters["lightPosition"].SetValue(LightPosition);
                    effect.Parameters["eyePosition"]?.SetValue(CameraPosition);

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                    }
                }
                mesh.Draw();
            }
            _proyectil.Draw(viewProjection);
        }

        public void DrawBloom(Matrix viewProjection)
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

        public void UpdateCamera(ref Matrix view)
        {
            Vector3 cameraOffset = Vector3.Right * 60 + Vector3.Up * 7f;

            Vector3 targetPosition = BoundingBox.Center;
            Vector3 cameraPosition = targetPosition + cameraOffset;

            cameraPosition.Y = MathHelper.Clamp(cameraPosition.Y, ALTURA_MIN, ALTURA_MAX);

            view = Matrix.CreateLookAt(cameraPosition, targetPosition, Vector3.Up);
        }

        public void Update(GameTime gameTime, ref Matrix view)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            Vector3 nuevoMovimiento = Vector3.Left * 4 * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;

            _distanciaRecorrida += nuevoMovimiento.X;

            Matrix objetivoWorldMatrix;
            if (_objetivos != null)
            {
                Console.WriteLine("Objetivo encontrado");
                objetivoWorldMatrix = Matrix.CreateTranslation(_objetivos[0].OBB.Center);
            }
            else
            {
                objetivoWorldMatrix = Matrix.Identity;
            }

            if (_proyectil.EstaDestruido())
            {
                _tiempoAcumuladoProyectil += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_tiempoAcumuladoProyectil > TIME_BETWEEN_SHOTS && _objetivos != null)
                {
                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        _objetivos = null;
                        _tiempoAcumuladoProyectil = 0.0f;
                        _proyectil.SetWorldMatrix(_worldMatrix.Translation, objetivoWorldMatrix, gameTime);
                    }
                }
            }

            if (_tieneEscudo)
            {
                _tiempoAcumuladoEscudo += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (keyboardState.IsKeyDown(Keys.E))
            {
                SetEscudo();
            }
            if (_tiempoAcumuladoEscudo >= DURACION_ESCUDO)
            {
                _tieneEscudo = false;
            }


            if (keyboardState.IsKeyDown(Keys.Left))
                _angulo += VELOCIDAD_DE_GIRO * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.Right))
                _angulo -= VELOCIDAD_DE_GIRO * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.A))
                nuevoMovimiento += Vector3.Backward * 2f * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.W))
                nuevoMovimiento += Vector3.Up * 2f * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.S))
                nuevoMovimiento += Vector3.Down * 2f * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyboardState.IsKeyDown(Keys.D))
                nuevoMovimiento += Vector3.Forward * 2f * VELOCIDAD * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.G) && !_toogleGodModeActive)
            {
                _godMode = !_godMode;
            }
            _toogleGodModeActive = keyboardState.IsKeyDown(Keys.G);



            Position += nuevoMovimiento;
            Position = new Vector3(Position.X, MathHelper.Clamp(Position.Y, ALTURA_MIN, ALTURA_MAX), MathHelper.Clamp(Position.Z, DISTANCIA_MIN, DISTANCIA_MAX));

            _worldMatrix =
                _scaleMatrix *
                Matrix.CreateRotationY(MathHelper.ToRadians(ROTACION_INICIAL)) *
                Matrix.CreateRotationX(MathHelper.ToRadians(_angulo)) *
                Matrix.CreateTranslation(Position);

            _proyectil.Update(gameTime);
            UpdateOrientedBoundingBoxWorld();

            UpdateCamera(ref view);
        }

        public void DrawEscudo(Matrix viewProjection, GameTime gameTime)
        {
            if (_tieneEscudo)
            {
                _escudo.Draw(viewProjection, Matrix.CreateTranslation(_worldMatrix.Translation), gameTime);
            }
        }

        public void Destroy()
        {
            if (!_godMode)
            {
                _sonidoColision.Play();
                _estaDestruido = true;
            }
        }

        public bool EstaDestruido()
        {
            return _estaDestruido;
        }

        public float GetDistanciaRecorrida()
        {
            return _distanciaRecorrida;
        }

        public void SetEscudo()
        {
            _tieneEscudo = true;
            _tiempoAcumuladoEscudo = 0.0f;
        }

        public void Restart()
        {
            _tiempoAcumuladoEscudo = 0.0f;
            _tiempoAcumuladoProyectil = 0.0f;
            _tieneEscudo = true;
            _estaDestruido = false;
            _distanciaRecorrida = 0.0f;
            _angulo = 0.0f;
            _godMode = false;
            _toogleGodModeActive = true;
            Position = Vector3.Zero;
            _distanciaRecorrida = 0.0f;
            _estaDestruido = false;
            _tieneEscudo = true;
            var rotation = Matrix.CreateRotationY(MathHelper.ToRadians(ROTACION_INICIAL));
            _worldMatrix = rotation * _scaleMatrix;
        }
    }
}
