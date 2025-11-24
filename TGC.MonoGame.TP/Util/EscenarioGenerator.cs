using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.Models;
using TGC.MonoGame.TP.Models.Modules;
using TGC.MonoGame.TP.Models.Modules.Contract;

namespace TGC.MonoGame.TP.Util;

internal class EscenarioGenerator
{
    private const int MAX_MODULES = 20;
    private const int MAX_DISTINCT_MODULES = 5;

    private readonly Random rng;
    private readonly Matrix inicio;
    private readonly (int, IModule)[] modulos;
    private int lastPosition;
    private int modulosRecorridos;

    Dictionary<int, Stack<IModule>> stackModulos;

    public const float DISTANCE_BETWEEN_MODULES = 56.5f;

    public EscenarioGenerator()
    {
        rng = new Random();
        inicio = Matrix.Identity;

        modulos = new (int, IModule)[MAX_MODULES];
    }

    public void GenerarEscenario()
    {

        stackModulos = [];

        Stack<IModule> basicModules = new();
        stackModulos.Add(BasicModule.GetModuleNumber(), basicModules);

        Stack<IModule> boxModules = new();
        stackModulos.Add(BoxModule.GetModuleNumber(), boxModules);

        Stack<IModule> cargoModules = new();
        stackModulos.Add(CargoModule.GetModuleNumber(), cargoModules);

        Stack<IModule> pipeModules = new();
        stackModulos.Add(PipeModule.GetModuleNumber(), pipeModules);

        Stack<IModule> shipModules = new();
        stackModulos.Add(ShipModule.GetModuleNumber(), shipModules);


        for (int i = 0; i < MAX_MODULES / 2; i++)
        {
            basicModules.Push(new BasicModule());
            boxModules.Push(new BoxModule());
            cargoModules.Push(new CargoModule());
            pipeModules.Push(new PipeModule());
            shipModules.Push(new ShipModule());
        }

        Matrix modulo2 = inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES);

        modulos[0] = (PipeModule.GetModuleNumber(), stackModulos.GetValueOrDefault(PipeModule.GetModuleNumber()).Pop());
        modulos[0].Item2.SetWorldMatrix(inicio);

        modulos[1] = (PipeModule.GetModuleNumber(), stackModulos.GetValueOrDefault(PipeModule.GetModuleNumber()).Pop());
        modulos[1].Item2.SetWorldMatrix(modulo2);

        for (int i = 2; i < MAX_MODULES; i++)
        {
            int indiceModulo = rng.Next(MAX_DISTINCT_MODULES);
            var modulo = GetModule(ref indiceModulo);
            modulos[i] = (indiceModulo, modulo);
            modulos[i].Item2.SetWorldMatrix(inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES * i));
        }

        lastPosition = 20;
        modulosRecorridos = 0;
    }

    //Devuelve el modulo del tipo indicado o de algun tipo siguiente si esta vacia la pila
    private IModule GetModule(ref int indice)
    {
        var stack = stackModulos.GetValueOrDefault(indice);
        if (stack.Count != 0)
        {
            return stack.Pop();
        }
        else
        {
            indice = (indice + 1) % MAX_DISTINCT_MODULES;
            return GetModule(ref indice);
        }
    }

    public void AvanzarEscenario()
    {
        var posicionModulo = lastPosition % MAX_MODULES;
        var moduloARemover = modulos[posicionModulo];
        stackModulos.GetValueOrDefault(moduloARemover.Item1).Push(moduloARemover.Item2);

        int indiceModulo = rng.Next(MAX_DISTINCT_MODULES);
        var modulo = GetModule(ref indiceModulo);
        modulos[posicionModulo] = (indiceModulo, modulo);
        modulos[posicionModulo].Item2.SetWorldMatrix(inicio * Matrix.CreateTranslation(Vector3.Left * DISTANCE_BETWEEN_MODULES * lastPosition));

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

        foreach (var module in modulos)
        {
            module.Item2.Update(gameTime, player);
        }
    }

    public void Draw(Matrix viewProjection, Vector3 cameraPosition, float elapsedTime)
    {
        foreach (var module in modulos)
        {
            module.Item2.Draw(viewProjection, cameraPosition, elapsedTime);
        }
    }

    public void DrawBloom(Matrix viewProjection)
    {
        foreach (var module in modulos)
        {
            module.Item2.DrawBloom(viewProjection);
        }
    }
}