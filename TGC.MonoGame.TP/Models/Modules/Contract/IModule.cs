using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TGC.MonoGame.TP.Util;

namespace  TGC.MonoGame.TP.Models.Modules;

internal interface IModule
{
    public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition);
    public void DrawBloom(Matrix view, Matrix projection);
    public void Update(GameTime gameTime, PlayerShip player,EscenarioGenerator generator, ref List<IModule> escenario);
    private void GenerateObstacles(ContentManager content, string contentFolder3D, string contentFolderEffects, Matrix worldMatrix) { }//Esto deberia llamar a los generadores de los obstaculos del modulo.
    private void GenerateDecoration() { }//Esto deberia llamar a los generadores de las decoraciones y pocisionarlos.

    public string Modulo();
    //private void Destroy(){}
} 

