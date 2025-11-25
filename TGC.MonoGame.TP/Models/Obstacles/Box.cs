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

        // Definiciones de distancia para la opacidad
        private const float MAX_DISTANCE = 50f; // Completamente opaco a partir de esta distancia
        private const float MIN_DISTANCE = 20f;  // Completamente transparente (o casi) a esta distancia
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


public void Draw(Matrix viewProjection, Vector3 playerPosition,GraphicsDevice _graphicsDevice ) 
        {
            if (!_estaDestruido)
            {
                // 2. Cálculo de la Opacidad
                Vector3 modelPosition = _worldMatrix.Translation; 
                float distance = Vector3.Distance(playerPosition, modelPosition);

                float opacity = 1.0f; 
                
                if (distance < MAX_DISTANCE)
                {
                    // Mapear la distancia de [MIN_DISTANCE, MAX_DISTANCE] a [0, 1]
                    // El valor 't' es 1.0 cuando está lejos y 0.0 cuando está cerca.
                    float t = MathHelper.Clamp((distance - MIN_DISTANCE) / (MAX_DISTANCE - MIN_DISTANCE), 0f, 1f);
                    opacity = t; 
                }

                // 3. 🚨 Habilitar el Blending para transparencia (CRUCIAL)
                if (opacity < 1.0f)
                {
                    _graphicsDevice.BlendState = BlendState.AlphaBlend;
                }
                else
                {
                    _graphicsDevice.BlendState = BlendState.Opaque;
                }

                foreach (var mesh in _model.Meshes)
                {
                    var meshWorld = mesh.ParentBone.Transform;
                    var world = meshWorld * _worldMatrix;

                    foreach (var meshPart in mesh.MeshParts)
                    {
                        var effect = meshPart.Effect;
                        effect.CurrentTechnique = effect.Techniques["BasicColorDrawing"];
                        
                        // 4. Setea el valor de Opacity en el shader combinado
                        if (effect.Parameters["Opacity"] != null)
                        {
                            effect.Parameters["Opacity"].SetValue(opacity);
                        }
                        
                        effect.Parameters["ViewProjection"].SetValue(viewProjection);
                        effect.Parameters["World"].SetValue(world);

                        foreach (var pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                        }
                    }
                    mesh.Draw();
                }
                
                // 5. Restaurar el Blending después de dibujar la caja para no afectar otros objetos opacos.
                _graphicsDevice.BlendState = BlendState.Opaque; 
                
                //DebugDraw.DrawOBB(OBB, viewProjection, Color.Red);
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
                //UpdateOrientedBoundingBoxWorld();
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
