using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using TGC.MonoGame.TP.CameraDebug;
using TGC.MonoGame.TP.Models.Modules;
using TGC.MonoGame.TP.Models;
using TGC.MonoGame.TP.Util;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.TP.Models.Obstacles;
using TGC.MonoGame.TP.Models.BaseModels;



namespace TGC.MonoGame.TP;

public class MonoGaming : Game
{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderMusic = "Music/";
    public const string ContentFolderSounds = "Sounds/";
    public const string ContentFolderSpriteFonts = "SpriteFonts/";
    public const string ContentFolderTextures = "Textures/";

    private Camera DebugCamera { get; set; }

    private readonly GraphicsDeviceManager _graphics;

    private Microsoft.Xna.Framework.Matrix _projection;
    private Microsoft.Xna.Framework.Matrix _view;

    private List<IModule> escenario;
    private EscenarioGenerator escenarioGenerator;

    private const float VELOCIDAD = 20f;

    private int puntos = 0;
    private int multiplicador = 1;
    private double acumuladorIntermedioPuntos = 0;
    private int vueltasAcumulador = 0;
    private PlayerShip player;

    float tiempoAcumulado = 0f;
    private Vector3 CameraPosition;
    private Vector3 LightPosition;
    public static Color LightAmbientColor = new Color(0.25f, 0.0f, 0.0f);
    public static Color LightDiffuseColor = Color.LightYellow;
    public static Color LightSpecularColor = Color.White;

    private bool _wasPaused = false;
    private GameState gameState = GameState.Menu;

    private SpriteBatch spriteBatch;
    private Background background;
    private Song song;
    private PauseMenu pauseMenu;
    private Menu mainMenu;
    private HUD hud;
    private GameOverScreen gameOverScreen;

    public MonoGaming()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

        // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
        // Carpeta raiz donde va a estar toda la Media.
        Content.RootDirectory = "Content";
        // Hace que el mouse sea visible.
        IsMouseVisible = true;


        //Inicializo el escenario y su generador infinito
        escenarioGenerator = new EscenarioGenerator(Content);
        escenario = null;
    }


    protected override void Initialize()
    {


        int Width = GraphicsDevice.Viewport.Width;
        int Height = GraphicsDevice.Viewport.Height;

        spriteBatch = new SpriteBatch(GraphicsDevice);
        // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.
        LightPosition = Vector3.One * 1000;
        CameraPosition = Vector3.One;

        DebugCamera = new SimpleCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.UnitZ * 150, 400, 2.0f, 1, 3000);


        //Como el estado y Exit() solo existen es esta clase, se tienen que crear aca
        List<RectangleButton> pauseButtons = new List<RectangleButton>();

        // Crear el botón "Reanudar"
        RectangleButton resumeButton = new RectangleButton(
            Content,
            "Reanudar",
            new Rectangle(MathHelper.Max(0, (Width / 2) - 400 - (384 / 2)), Height / 2, 384, 128)); // Posición X, Y, Ancho, Alto

        // Asignar la acción que debe ejecutar (usando una función lambda)
        resumeButton.OnClick = () =>
        {
            gameState = GameState.Playing; // Cambia el estado del juego
        };

        pauseButtons.Add(resumeButton);

        // Crear el botón "Salir"
        RectangleButton quitButton = new RectangleButton(
            Content,
            "Salir del juego",
            new Rectangle(MathHelper.Min(Width, (Width / 2) + 400), Height / 2, 384, 128));

        quitButton.OnClick = () =>
        {
            Exit(); // Cierra el juego (o vuelve al menú principal)
        };

        pauseButtons.Add(quitButton);

        // BOTONES MENU PRINCIPAL

        List<RectangleButton> menuButtons = new List<RectangleButton>();

        RectangleButton playButton = new RectangleButton(
            Content,
            "Jugar",
            new Rectangle(MathHelper.Max(0, (Width / 2) - 400 - (384 / 2)), Height / 2, 384, 128)); // Posición X, Y, Ancho, Alto

        playButton.OnClick = () =>
        {
            gameState = GameState.Playing; // Cambia el estado del juego
        };

        menuButtons.Add(playButton);
        menuButtons.Add(quitButton);

        background = new Background(Content);
        player = new PlayerShip(Content);
        pauseMenu = new PauseMenu(Content, pauseButtons, spriteBatch);
        mainMenu = new Menu(Content, menuButtons, spriteBatch);
        hud = new HUD(Content);
        gameOverScreen = new GameOverScreen(Content, menuButtons, spriteBatch, puntos, new Vector2(Width / 2, Height / 2));

        // Configuramos nuestras matrices de la escena.
        _view = Matrix.CreateLookAt(new Vector3(0, 0, 300), Vector3.Zero, Vector3.Up);
        _projection =
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 2500);

        base.Initialize();
    }

    protected override void LoadContent()
    {

  
        song = Content.Load<Song>(ContentFolderMusic + "GameBackgroundSong");

        // check the current state of the MediaPlayer.
        if (MediaPlayer.State != MediaState.Stopped)
        {
            MediaPlayer.Stop(); // stop current audio playback if playing or paused.
        }

        // Play the selected song reference.
        MediaPlayer.Play(song);

        var worldMatrix_1 = Matrix.Identity * Matrix.CreateTranslation(Vector3.Left * 60.5f);
        var worldMatrix_2 = Matrix.Identity * Matrix.CreateTranslation(Vector3.Left * 60.5f * 2);
        Content.Load<SoundEffect>(ContentFolderSounds + "Explosion");
        Content.Load<SoundEffect>(ContentFolderSounds + "ProyectilLaser"); //Precarga para que no bajen los fps cuando se dispare el primer disparo
        //Se genera el escenario.
        escenarioGenerator.GenerarEscenario(ref escenario);
        BoundingSphereRenderer.Initialize(GraphicsDevice);

        base.LoadContent();
    }


     protected override void Update(GameTime gameTime)
     {
        KeyboardState keyboardState = Keyboard.GetState();

        DebugCamera.Update(gameTime);
        _view = DebugCamera.View;
        _projection = DebugCamera.Projection;

        if (gameState == GameState.Menu)
        {
            mainMenu.Update(gameTime);
        }
        else if (gameState == GameState.GameOver)
        {
            gameOverScreen.Update();

        }
        else if (player.EstaDestruido())
        {
            gameState = GameState.GameOver;
            gameOverScreen.setPuntos(puntos);
           player.Restart();
            puntos = 0;
            multiplicador = 1;
            vueltasAcumulador = 0;
            acumuladorIntermedioPuntos = 0;
            escenarioGenerator.GenerarEscenario(ref escenario);
        }
         else
        {
            acumuladorIntermedioPuntos += gameTime.ElapsedGameTime.Milliseconds;
            if ((acumuladorIntermedioPuntos / 1000) >= 1)
            {
                acumuladorIntermedioPuntos -= 1000;
                vueltasAcumulador++;
                puntos += multiplicador;
                multiplicador = (vueltasAcumulador / 5) + 1;
            }
           hud.Update(puntos, multiplicador);

            if (keyboardState.IsKeyDown(Keys.Escape) && !_wasPaused)
            {
                 if (gameState == GameState.Playing)
                    gameState = GameState.Paused;
               else if (gameState == GameState.Paused)
                   gameState = GameState.Playing;
            }
           _wasPaused = keyboardState.IsKeyDown(Keys.Escape);

            if (gameState == GameState.Playing)
        {
                player.Update(gameTime, ref _view, ref _projection, Content);
              Matrix aux = Matrix.Invert(_view);
               CameraPosition = new Vector3(aux.M41, aux.M42, aux.M43);
                LightPosition = player.Position + (Vector3.Left * 50) + (Vector3.Down * 3);
              
                tiempoAcumulado += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (tiempoAcumulado >= 0.75f)
                {
                    tiempoAcumulado = 0f;
                   escenarioGenerator.AvanzarEscenario(ref escenario);
                }
                foreach (var modulo in escenario)
                {
                    modulo.Update(gameTime, player, escenarioGenerator, ref escenario);
                }

                base.Update(gameTime);
            }
            else
            {
                pauseMenu.Update(gameTime);
            }
        }
    }
 


    protected override void Draw(GameTime gameTime)
    {


        float elapsedTime = (float)gameTime.TotalGameTime.TotalSeconds;
        //El fondo es negro
        GraphicsDevice.Clear(Color.Black);

        //TIENE QUE IR ANTES DEL DRAW PRINCIPAL
        background.Draw(GraphicsDevice);

        if (gameState == GameState.Menu)
        {
            mainMenu.Draw(_view, _projection, LightPosition, CameraPosition);
        }
        else if (gameState == GameState.GameOver)
        {
            gameOverScreen.Draw(GraphicsDevice);

        }
        else
        {

            player.Draw(_view, _projection, CameraPosition, LightPosition,GraphicsDevice);
            foreach (IModule module in escenario)
            {
                module.Draw(_view, _projection, CameraPosition,elapsedTime);
                //new Box(Content, Matrix.Identity * Matrix.CreateTranslation(LightPosition), 0.0f).Draw(_view, _projection);
            }
            hud.Draw(spriteBatch);

            if (gameState == GameState.Paused)
            {
                //TIENE QUE IR DESPUES DEL DRAW PRINICPAL
                pauseMenu.Draw(GraphicsDevice);
            }
        }


        //Cada modelo deberia tener su propio draw.
        //A menos que sea para prueba no deberian haber dibujos en este metodo
    }


    protected override void UnloadContent()
    {
        Content.Unload();

        base.UnloadContent();
    }
}