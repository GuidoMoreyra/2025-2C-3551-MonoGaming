using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Models;
using TGC.MonoGame.TP.Models.Escenario;
using TGC.MonoGame.TP.Models.Modules;
using TGC.MonoGame.TP.Models.Modules.Contract;

namespace TGC.MonoGame.TP.Util;

internal class EscenarioGenerator
{
    private const int MAX_MODULES = 20;
    private const int MAX_DISTINCT_MODULES = 5;

    private readonly Random rng;
    private readonly Matrix inicio;
    private readonly IModule[] modulos;
    private int lastPosition;
    private int modulosRecorridos;

    Dictionary<TipoDeModulo, Stack<IModule>> stackModulos;

    public const float DISTANCE_BETWEEN_MODULES = 56.5f;

    public EscenarioGenerator()
    {
        rng = new Random();
        inicio = Matrix.Identity;

        modulos = new IModule[MAX_MODULES];
    }

    public void GenerarEscenario()
    {

        stackModulos = [];

        Stack<IModule> basicModules = new();
        stackModulos.Add(TipoDeModulo.Basic, basicModules);

        Stack<IModule> box1Modules = new();
        stackModulos.Add(TipoDeModulo.Box1, box1Modules);

        Stack<IModule> box2Modules = new();
        stackModulos.Add(TipoDeModulo.Box2, box2Modules);

        Stack<IModule> box3Modules = new();
        stackModulos.Add(TipoDeModulo.Box3, box3Modules);

        Stack<IModule> ship1Modules = new();
        stackModulos.Add(TipoDeModulo.Ship1, ship1Modules);


        for (int i = 0; i < MAX_MODULES / 2; i++)
        {
            basicModules.Push(new BasicModule());
            box1Modules.Push(new BoxModule_1());
            box2Modules.Push(new BoxModule_2());
            box3Modules.Push(new BoxModule_3());
            ship1Modules.Push(new ShipModule_1());
        }

        Matrix modulo2 = inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES);

        modulos[0] = stackModulos.GetValueOrDefault(TipoDeModulo.Basic).Pop();
        modulos[0].SetWorldMatrix(inicio);

        modulos[1] = stackModulos.GetValueOrDefault(TipoDeModulo.Basic).Pop();
        modulos[1].SetWorldMatrix(modulo2);

        for (int i = 2; i < MAX_MODULES; i++)
        {
            modulos[i] = GetRandomModule();
            modulos[i].SetWorldMatrix(inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES * i));
        }

        lastPosition = 20;
        modulosRecorridos = 0;
    }

    private IModule GetRandomModule()
    {
        int indiceModulo = rng.Next(MAX_DISTINCT_MODULES);
        return GetModule(indiceModulo);        
    }

    //Devuelve el modulo del tipo indicado o de algun tipo siguiente si esta vacia la pila
    private IModule GetModule(int indice)
    {
        TipoDeModulo tipoDeModulo = (TipoDeModulo)Enum.GetValues(typeof(TipoDeModulo)).GetValue(indice);
        var stack = stackModulos.GetValueOrDefault(tipoDeModulo);
        if (stack.Count != 0)
        {
            return stack.Pop();
        }
        else
        {
            indice = (indice + 1) % MAX_DISTINCT_MODULES;
            return GetModule(indice);
        }
    }

    public void AvanzarEscenario()
    {
        var posicionModulo = lastPosition % MAX_MODULES;
        var moduloARemover = modulos[posicionModulo];
        stackModulos.GetValueOrDefault(moduloARemover.GetTipoDeModulo()).Push(moduloARemover);

        modulos[posicionModulo] = GetRandomModule();
        modulos[posicionModulo].SetWorldMatrix(inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES * lastPosition));

        lastPosition++;
    }

    public void Update(GameTime gameTime, PlayerShip player)
    {
        if (modulosRecorridos < Math.Floor(Math.Abs(player.GetDistanciaRecorrida()) / DISTANCE_BETWEEN_MODULES))
        {
            // Se retrasa el avance del escenario para que no se vea como se deja de dibujar el modulo
            if (modulosRecorridos > 1)
            {
                AvanzarEscenario();
            }
            modulosRecorridos++;
        }

        modulos[modulosRecorridos % MAX_MODULES].IsOn = true;

        foreach (var module in modulos)
        {
            module.Update(gameTime, player);
        }
    }

    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime)
    {
        foreach (var module in modulos)
        {
            module.Draw(viewProjection, cameraPosition, elapsedTime);
        }
    }

    public void DrawBloom(Matrix viewProjection)
    {
        foreach (var module in modulos)
        {
            module.DrawBloom(viewProjection);
        }
    }
}