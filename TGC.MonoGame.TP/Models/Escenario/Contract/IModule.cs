using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Models.Escenario;

namespace TGC.MonoGame.TP.Models.Modules.Contract;

internal interface IModule
{

    public List<DestroyableBox> obstaclesD{ get; set; }
    public bool IsOn { get; set; }
    public void SetWorldMatrix(Matrix worldMatrix);
    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime, GraphicsDevice _graphicsDevice );
    public void DrawBloom(Matrix viewProjection);
    public void Update(GameTime gameTime, PlayerShip player);
    public TipoDeModulo GetTipoDeModulo();
}
