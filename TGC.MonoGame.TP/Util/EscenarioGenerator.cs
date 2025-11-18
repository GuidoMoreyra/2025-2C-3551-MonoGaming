using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using TGC.MonoGame.TP.Models.Modules;

namespace TGC.MonoGame.TP.Util;

internal class EscenarioGenerator
{
    private readonly Random rng = new Random();
    private const int MAX_MODULES = 20;
    private readonly Matrix inicio = Matrix.Identity;
    private int lastPosition;
    private string lastModule;
    ContentManager contentManager;

    public const float DISTANCE_BETWEEN_MODULES = 56.5f;

    public EscenarioGenerator(ContentManager contentManager)
    {
        this.contentManager = contentManager;
    }

    public void GenerarEscenario( ref List<IModule> escenario)
    {
        escenario = null;
            
        Matrix modulo2 = inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES);
        Matrix modulo3 = inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES * 2);
        escenario = new List<IModule>
        {
            new BasicModule(contentManager,inicio),
            new BasicModule(contentManager,modulo2),
            new BasicModule(contentManager,modulo3),
        };
        lastModule = "Basic";
        for(int index = 3; index <= MAX_MODULES; index++)
        {
            Matrix worldMatrix = inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES * index);
            lastPosition = index;
            escenario.Add(SeleccionarModulo(worldMatrix));
        }
    }

    public void AvanzarEscenario( ref List<IModule> escenario)
    {
        escenario.RemoveAt(0);
        Matrix worldMatrix = inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES * ++lastPosition);
        escenario.Add(SeleccionarModulo(worldMatrix));
    }
    
    public IModule SeleccionarModulo(Matrix worldMatrix)
    {
        //Seguramente haya una mejor manera de hacer las cosas , deberia refactorizar  esta clase.

        int randomNumber = rng.Next(0, 101);

        switch (lastModule)
        {
            case "Basic":
                if (randomNumber < 25)
                {
                    lastModule = "Corridor";
                    return new BoxModule(contentManager, worldMatrix);
                }
                else if (randomNumber < 50)
                {
                    lastModule = "Corridor";
                    return new ShipModule(contentManager, worldMatrix);
                }
                //else if (randomNumber < 75) return new BreakableModule(contentManager, worldMatrix);
                
                lastModule = "Asteroid";
                return new CargoModule(contentManager, worldMatrix);
            
            case "Asteroid":
                if (randomNumber < 15)
                {
                    lastModule = "Corridor";
                    return new BoxModule(contentManager, worldMatrix);
                }
                else if (randomNumber < 30)
                {
                    lastModule = "Corridor";
                    return new ShipModule(contentManager, worldMatrix);
                }
                //else if (randomNumber < 75) return new BreakableModule(contentManager, worldMatrix);
                lastModule = "Asteroid";
                return new CargoModule(contentManager, worldMatrix);

            
            case "Corridor": 
                if (randomNumber < 35)
                {
                    lastModule = "Corridor";
                    return new BoxModule(contentManager, worldMatrix);
                }
                else if (randomNumber < 70)
                {
                    lastModule = "Corridor";
                    return new ShipModule(contentManager, worldMatrix);
                }
                //else if (randomNumber < 85) return new BreakableModule(contentManager, worldMatrix);
                lastModule = "Asteroid";
                return new CargoModule(contentManager, worldMatrix);

            default:
                lastModule = "Basic";
                return new BasicModule(contentManager, worldMatrix);
            
        }
        
    }
}