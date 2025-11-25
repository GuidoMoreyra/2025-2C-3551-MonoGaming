using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Models.BaseModels
{
    internal class Caja_1
    {
        private static Model _model = null;
        
        // El nombre del nuevo archivo de shader (asumiendo que lo nombraste TextureWithOpacity.fx)
        private const string ShaderPath = "TextureOpacityShader"; 
        
        public static void InitializeModel(ContentManager content)
        {
            _model = content.Load<Model>(MonoGaming.ContentFolder3D + "Caja_1/Caja_1");
            
            // 1. Cargar la textura (debe tener el canal Alfa, como sugiere el nombre)
            var texture = content.Load<Texture2D>(MonoGaming.ContentFolderTextures + "Caja_1/Caja1_AlbedoTransparency");
            
            // 2. Cargar el nuevo shader combinado
            var effect = content.Load<Effect>(MonoGaming.ContentFolderEffects + ShaderPath);

            foreach (var mesh in _model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    // Clonar el efecto para que cada malla pueda tener sus propios parámetros (matrices, etc.)
                    var meshEffect = effect.Clone(); 
                    meshPart.Effect = meshEffect;
                    
                    // 3. Asignar la textura al parámetro 'Texture' definido en el HLSL
                    // (Esto conecta el Texture2D de C# con el sampler 's' del shader)
                    if (meshPart.Effect.Parameters["Texture"] != null)
                    {
                        meshPart.Effect.Parameters["Texture"].SetValue(texture);
                    }
                    
                    // NOTA IMPORTANTE: El parámetro 'Opacity' se setea en el método Draw
                    // de la clase Box, ya que es un valor dinámico que cambia por frame.
                }
            }
        }

        public static Model GetModel()
        {
            if(_model == null)
            {
                throw new InvalidOperationException("No se inicializo el modelo");
            }
            return _model;
        }
    }
}