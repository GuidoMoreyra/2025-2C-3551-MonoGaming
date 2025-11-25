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
        };
        lastModule = "Basic";
        for(int index = 3; index <= MAX_MODULES; index++)
        {
            Matrix worldMatrix = inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES * index);
            lastPosition = index;
        }
    }


    

        

}