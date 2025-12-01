using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.Player;

namespace TGC.MonoGame.TP.Models.Escenario.Contract;

internal interface IModule
{
    public bool IsOn { get; set; }
    public void SetWorldMatrix(Matrix worldMatrix);
    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime, GraphicsDevice _graphicsDevice);
    public void DrawBloom(Matrix viewProjection);
    public void Update(GameTime gameTime, PlayerShip player);
    public TipoDeModulo GetTipoDeModulo();
}
