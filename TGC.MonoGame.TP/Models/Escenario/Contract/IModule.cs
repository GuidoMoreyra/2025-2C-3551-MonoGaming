using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Util;

namespace  TGC.MonoGame.TP.Models.Modules;

internal interface IModule
{
    public Boolean isOn { get; set; }
    public void Update(GameTime gameTime, PlayerShip player,EscenarioGenerator generator, ref List<IModule> escenario);
    public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, float elapsedTime);
    public void DrawBloom(Matrix view, Matrix projection);
    public TipoDeModulo Modulo();

} 

