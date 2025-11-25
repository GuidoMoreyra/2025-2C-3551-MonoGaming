using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Models;
using TGC.MonoGame.TP.Util;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
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

    public static Color LightAmbientColor = new(0.25f, 0.0f, 0.0f);
    public static Color LightDiffuseColor = Color.LightYellow;
    public static Color LightSpecularColor = Color.White;

    private readonly GraphicsDeviceManager _graphics;
    private Vector2 _viewportDimensions;

    private Matrix _projection;
    private Matrix _view;

    private EscenarioGenerator escenarioGenerator;

    private int puntos = 0;
    private int multiplicador = 1;
    private double acumuladorIntermedioPuntos = 0;
    private int vueltasAcumulador = 0;
    private PlayerShip player;

    private Vector3 CameraPosition;
    private Vector3 LightPosition;

    private bool _wasPaused = false;
    private bool _isFirstFrame = true;
    private GameState gameState = GameState.Menu;

    private SpriteBatch spriteBatch;
    private Background background;
    private Song song;
    private PauseMenu pauseMenu;
    private Menu mainMenu;
    private HUD hud;
    private GameOverScreen gameOverScreen;

    private RenderTarget2D _firstPassBloomRenderTarget;

    private RenderTarget2D _mainSceneRenderTarget;

    private RenderTarget2D _secondPassBloomRenderTarget;
    private RenderTarget2D _previousFrameRenderTarget;
    private RenderTarget2D _currentFrameRenderTarget;

    private Effect _gaussianBlur;

    private Effect _bloomPost;

    private Effect _motionBlur;

    private Effect _basicShaderTexture;

    public MonoGaming()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100,
            PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100
        };

        // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
        // Carpeta raiz donde va a estar toda la Media.
        Content.RootDirectory = "Content";
        // Hace que el mouse sea visible.
        IsMouseVisible = true;
    }


    protected override void Initialize()
    {
        DebugDraw.GraphicsDevice = GraphicsDevice; //TODO Sacar cuando funcione bien las obb

        _viewportDimensions = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        spriteBatch = new SpriteBatch(GraphicsDevice);

        //Se posiciona la camara y la luz para el menu
        LightPosition = Vector3.One * 1000;
        CameraPosition = Vector3.One;

        //Cargo los modelos
        Caja_1.InitializeModel(Content);
        Nave_1.InitializeModel(Content);
        Nave_2.InitializeModel(Content);
        Pasillo_Asteroide.InitializeModel(Content);
        Pasillo.InitializeModel(Content);
        Pipe.InitializeModel(Content);

        //Inicializo el generador de escenarios (TIENE QUE SER DESPUES DE INICIALIZAR LOS MODELOS)
        escenarioGenerator = new EscenarioGenerator();
        escenarioGenerator.GenerarEscenario();

        background = new Background(Content, GraphicsDevice);
        player = new PlayerShip(Content);
        hud = new HUD(Content, spriteBatch);
        pauseMenu = GeneratePauseMenu();
        mainMenu = GenerateMenu();
        gameOverScreen = GenerateGameOverScreen();

        //Creo render targets
        _mainSceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
            RenderTargetUsage.DiscardContents);
        _firstPassBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
            RenderTargetUsage.DiscardContents);
        _secondPassBloomRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
            RenderTargetUsage.DiscardContents);
        _previousFrameRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
            RenderTargetUsage.DiscardContents);
        _currentFrameRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0,
            RenderTargetUsage.DiscardContents);

        // Configuramos nuestras matrices de la escena.
        _view = Matrix.CreateLookAt(new Vector3(0, 0, 300), Vector3.Zero, Vector3.Up);
        _projection =
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 2500);

        base.Initialize();
    }

    private PauseMenu GeneratePauseMenu()
    {
        //Como el estado y Exit() solo existen es esta clase, se tienen que crear aca
        List<RectangleButton> pauseButtons = [];

        // Crear el botón "Reanudar"
        RectangleButton resumeButton = new(
            Content,
            "Reanudar",
            new Rectangle(MathHelper.Max(0, ((int)(_viewportDimensions.X / 2)) - 400 - (384 / 2)), (int)_viewportDimensions.Y / 2, 384, 128), // Posición X, Y, Ancho, Alto
            spriteBatch)
        {
            // Asignar la acción que debe ejecutar (usando una función lambda)
            OnClick = () =>
                {
                    gameState = GameState.Playing; // Cambia el estado del juego
                }
        };

        pauseButtons.Add(resumeButton);

        // Crear el botón "Salir"
        RectangleButton quitButton = new(
            Content,
            "Salir del juego",
            new Rectangle(MathHelper.Min((int)_viewportDimensions.X, ((int)(_viewportDimensions.X / 2)) + 400), (int)_viewportDimensions.Y / 2, 384, 128),
            spriteBatch)
        {
            OnClick = () =>
            {
                Exit(); // Cierra el juego
            }
        };

        pauseButtons.Add(quitButton);

        return new PauseMenu(Content, pauseButtons, spriteBatch, GraphicsDevice);
    }

    private Menu GenerateMenu()
    {
        List<RectangleButton> menuButtons = [];

        // Crear el botón "Salir"
        RectangleButton quitButton = new(
            Content,
            "Salir del juego",
            new Rectangle(MathHelper.Min((int)_viewportDimensions.X, ((int)(_viewportDimensions.X / 2)) + 400), (int)_viewportDimensions.Y / 2, 384, 128),
            spriteBatch)
        {
            OnClick = () =>
            {
                Exit(); // Cierra el juego
            }
        };

        RectangleButton playButton = new(
            Content,
            "Jugar",
            new Rectangle(MathHelper.Max(0, ((int)(_viewportDimensions.X / 2)) - 400 - (384 / 2)), (int)_viewportDimensions.Y / 2, 384, 128),
            spriteBatch)
        {
            OnClick = () =>
            {
                gameState = GameState.Playing; // Cambia el estado del juego
            }
        };

        menuButtons.Add(playButton);
        menuButtons.Add(quitButton);

        return new Menu(menuButtons, spriteBatch);
    }

    private GameOverScreen GenerateGameOverScreen()
    {
        List<RectangleButton> gameOverButtons = [];

        // Crear el botón "Salir"
        RectangleButton quitButton = new(
            Content,
            "Salir del juego",
            new Rectangle(MathHelper.Min((int)_viewportDimensions.X, ((int)(_viewportDimensions.X / 2)) + 400), (int)_viewportDimensions.Y / 2, 384, 128),
            spriteBatch)
        {
            OnClick = () =>
            {
                Exit(); // Cierra el juego
            }
        };

        RectangleButton playButton = new(
            Content,
            "Jugar",
            new Rectangle(MathHelper.Max(0, ((int)(_viewportDimensions.X / 2)) - 400 - (384 / 2)), (int)_viewportDimensions.Y / 2, 384, 128),
            spriteBatch)
        {
            OnClick = () =>
            {
                gameState = GameState.Playing; // Cambia el estado del juego
            }
        };

        gameOverButtons.Add(playButton);
        gameOverButtons.Add(quitButton);

        return new GameOverScreen(Content, gameOverButtons, spriteBatch, _viewportDimensions / 2, GraphicsDevice);
    }

    protected override void LoadContent()
    {
        _gaussianBlur = Content.Load<Effect>(ContentFolderEffects + "GaussianBlur");
        _gaussianBlur.Parameters["screenSize"]
                .SetValue(_viewportDimensions);


        _bloomPost = Content.Load<Effect>(ContentFolderEffects + "PostProcesadoBloom");
        _motionBlur = Content.Load<Effect>(ContentFolderEffects + "MotionBlur");

        _basicShaderTexture = Content.Load<Effect>(ContentFolderEffects + "BasicShaderTexture");

        _basicShaderTexture.Parameters["ViewProjection"].SetValue(Matrix.Identity);
        _basicShaderTexture.Parameters["World"].SetValue(Matrix.Identity);

        song = Content.Load<Song>(ContentFolderMusic + "GameBackgroundSong");

        // check the current state of the MediaPlayer.
        if (MediaPlayer.State != MediaState.Stopped)
        {
            MediaPlayer.Stop(); // stop current audio playback if playing or paused.
        }
        MediaPlayer.IsRepeating = true;
        // Play the selected song reference.
        MediaPlayer.Play(song);

        Content.Load<SoundEffect>(ContentFolderSounds + "Explosion"); //TODO Sacar cuando haga el object pooling de las balas
        Content.Load<SoundEffect>(ContentFolderSounds + "ProyectilLaser"); //Precarga para que no bajen los fps cuando se dispare el primer disparo


        base.LoadContent();
    }


    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        if (gameState == GameState.Menu)
        {
            mainMenu.Update();
        }
        else if (gameState == GameState.GameOver)
        {
            gameOverScreen.Update();

        }
        else if (player.EstaDestruido())
        {
            gameState = GameState.GameOver;
            gameOverScreen.SetPuntos(puntos);
            player.Restart();
            puntos = 0;
            multiplicador = 1;
            vueltasAcumulador = 0;
            acumuladorIntermedioPuntos = 0;
            _isFirstFrame = true;
            escenarioGenerator.GenerarEscenario();
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

                escenarioGenerator.Update(gameTime, player);

                base.Update(gameTime);
            }
            else
            {
                pauseMenu.Update();
            }
        }
    }



    protected override void Draw(GameTime gameTime)
    {
        
        float elapsedTime = (float)gameTime.TotalGameTime.TotalSeconds;
        //El fondo es negro
        GraphicsDevice.Clear(Color.Black);

        if (gameState == GameState.Menu)
        {
            background.Draw();
            mainMenu.Draw(_view * _projection, LightPosition, CameraPosition);
        }
        else if (gameState == GameState.GameOver)
        {
            background.Draw();
            gameOverScreen.Draw();

        }
        else
        {

            //Se dibuja la escena principal en el rendertarget main
            #region Pass 1

            // Use the default blend and depth configuration
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Set the main render target, here we'll draw the base scene
            GraphicsDevice.SetRenderTarget(_mainSceneRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            var viewProjection = _view * _projection;

            background.Draw();

            player.Draw(viewProjection, CameraPosition, LightPosition, GraphicsDevice);

            escenarioGenerator.Draw(viewProjection, CameraPosition, elapsedTime,GraphicsDevice);
            hud.Draw();

            #endregion

            #region Pass 2
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Set the render target as our bloomRenderTarget, we are drawing the bloom color into this texture
            GraphicsDevice.SetRenderTarget(_firstPassBloomRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            player.DrawBloom(viewProjection);
            escenarioGenerator.DrawBloom(viewProjection);

            #endregion

            #region GaussianBlur
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            GraphicsDevice.SetRenderTarget(_secondPassBloomRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            _gaussianBlur.Parameters["baseTexture"].SetValue(_firstPassBloomRenderTarget);
            Quad.Draw(_gaussianBlur, GraphicsDevice);

            #endregion

            #region FinalBloom

            // Set the depth configuration as none, as we don't use depth in this pass
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.BlendState = BlendState.Opaque;

            GraphicsDevice.SetRenderTarget(_currentFrameRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            _bloomPost.Parameters["Texture"]?.SetValue(_mainSceneRenderTarget);
            _bloomPost.Parameters["bloomTexture"]?.SetValue(_secondPassBloomRenderTarget);

            Quad.Draw(_bloomPost, GraphicsDevice);

            #endregion

            #region MotionBlur

            // Set the depth configuration as none, as we don't use depth in this pass
            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            // Set the render target as null, we are drawing into the screen now!
            GraphicsDevice.SetRenderTarget(null);

            _motionBlur.Parameters["CurrentFrame"]?.SetValue(_currentFrameRenderTarget);
            if (_isFirstFrame)
            {
                _motionBlur.Parameters["PreviousFrame"]?.SetValue(_currentFrameRenderTarget);
                _isFirstFrame = false;
            }
            else
            {
                _motionBlur.Parameters["PreviousFrame"]?.SetValue(_previousFrameRenderTarget);
            }

            Quad.Draw(_motionBlur, GraphicsDevice);

            (_currentFrameRenderTarget, _previousFrameRenderTarget) = (_previousFrameRenderTarget, _currentFrameRenderTarget);

            #endregion

            if (gameState == GameState.Paused)
            {
                //TIENE QUE IR DESPUES DEL DRAW PRINICPAL
                pauseMenu.Draw();
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