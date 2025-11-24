using Microsoft.Xna.Framework;

namespace  TGC.MonoGame.TP.Models.Modules.Contract;

internal interface IModule
{
    public void SetWorldMatrix(Matrix worldMatrix);
    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime);
    public void DrawBloom(Matrix viewProjection);
    public void Update(GameTime gameTime, PlayerShip player);
} 
