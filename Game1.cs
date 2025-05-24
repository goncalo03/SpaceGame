using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

public class Game1 : Game
{
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    Player player;
    List<Enemy> enemies;
    List<Bullet> bullets;

    public static Texture2D playerTexture;
    public static Texture2D enemyTexture;
    public static Texture2D bulletTexture;
    private Texture2D pixelTexture;

    private Song backgroundMusic;
    private SoundEffect explosionSound;
    private SoundEffect playerDeathSound;
    private SoundEffect shootSound;

    private List<ParallaxLayer> parallaxLayers;
    private float[] layerSpeeds = { 20f, 40f };

    SpriteFont font;
    int score;
    float enemySpawnTimer;
    float spawnInterval = 1.5f;

    public static int ScreenWidth => 800;
    public static int ScreenHeight => 600;

    public static Game1 Instance { get; private set; }

    private Texture2D heartTexture;
    private Vector2 heartPosition = new Vector2(ScreenWidth - 100, 10);
    private int heartSpacing = 40;

    public int Lives { get; set; } = 3;
    
    // Estados do jogo
    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver
    }
    
    public GameState CurrentGameState { get; set; } = GameState.MainMenu;
    
    // Opções do menu
    private string[] mainMenuOptions = { "Start Game", "Exit" };
    private string[] gameOverOptions = { "Restart", "Main Menu", "Exit" };
    private int selectedMainMenuOption = 0;
    private int selectedGameOverOption = 0;

    // Controles do menu
    private KeyboardState currentKeyboardState;
    private KeyboardState previousKeyboardState;
    private float menuCooldown = 0.15f;
    private float currentCooldown = 0f;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Instance = this;
    }

    protected override void Initialize()
    {
        graphics.PreferredBackBufferWidth = ScreenWidth;
        graphics.PreferredBackBufferHeight = ScreenHeight;
        graphics.ApplyChanges();

        player = new Player();
        enemies = new List<Enemy>();
        bullets = new List<Bullet>();
        parallaxLayers = new List<ParallaxLayer>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        playerTexture = Content.Load<Texture2D>("player");
        enemyTexture = Content.Load<Texture2D>("enemy");
        bulletTexture = Content.Load<Texture2D>("bullet");
        heartTexture = Content.Load<Texture2D>("heart");
        font = Content.Load<SpriteFont>("font");
        backgroundMusic = Content.Load<Song>("background_music");

        MediaPlayer.Play(backgroundMusic);
        MediaPlayer.IsRepeating = true;

        explosionSound = Content.Load<SoundEffect>("explosion");
        playerDeathSound = Content.Load<SoundEffect>("explosion");
        shootSound = Content.Load<SoundEffect>("laser");

        Texture2D layer1 = Content.Load<Texture2D>("bg1");
        Texture2D layer2 = Content.Load<Texture2D>("bg2");

        parallaxLayers.Add(new ParallaxLayer(layer1, layerSpeeds[0], 0.9f));
        parallaxLayers.Add(new ParallaxLayer(layer2, layerSpeeds[1], 0.8f));

        pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        pixelTexture.SetData(new[] { Color.White });

        player.Texture = playerTexture;
        player.Position = new Vector2(400, 500);
    }

    public void ShootBullet(Vector2 position)
    {
        bullets.Add(new Bullet(bulletTexture, position, 8f, -1));
        shootSound.Play();
    }

    protected override void Update(GameTime gameTime)
    {
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();

        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Atualiza o cooldown do menu
        if (currentCooldown > 0)
        {
            currentCooldown -= elapsed;
        }

        foreach (var layer in parallaxLayers)
        {
            layer.Update(gameTime);
        }

        // Atualização baseada no estado atual do jogo
        switch (CurrentGameState)
        {
            case GameState.MainMenu:
                UpdateMainMenu(elapsed);
                break;
                
            case GameState.Playing:
                UpdateGame(gameTime);
                break;
                
            case GameState.GameOver:
                UpdateGameOverMenu(elapsed);
                break;
        }

        base.Update(gameTime);
    }

    private void UpdateMainMenu(float elapsed)
    {
        if (currentCooldown <= 0)
        {
            // Navegação no menu principal
            if (IsKeyPressed(Keys.Down))
            {
                selectedMainMenuOption = (selectedMainMenuOption + 1) % mainMenuOptions.Length;
                currentCooldown = menuCooldown;
            }
            else if (IsKeyPressed(Keys.Up))
            {
                selectedMainMenuOption = (selectedMainMenuOption - 1 + mainMenuOptions.Length) % mainMenuOptions.Length;
                currentCooldown = menuCooldown;
            }

            // Seleção de opção
            if (IsKeyPressed(Keys.Enter))
            {
                currentCooldown = menuCooldown;
                
                if (selectedMainMenuOption == 0) // Start Game
                {
                    StartNewGame();
                }
                else if (selectedMainMenuOption == 1) // Exit
                {
                    Exit();
                }
            }
        }
    }

    private void UpdateGameOverMenu(float elapsed)
    {
        if (currentCooldown <= 0)
        {
            // Navegação no menu de game over
            if (IsKeyPressed(Keys.Down))
            {
                selectedGameOverOption = (selectedGameOverOption + 1) % gameOverOptions.Length;
                currentCooldown = menuCooldown;
            }
            else if (IsKeyPressed(Keys.Up))
            {
                selectedGameOverOption = (selectedGameOverOption - 1 + gameOverOptions.Length) % gameOverOptions.Length;
                currentCooldown = menuCooldown;
            }

            // Seleção de opção
            if (IsKeyPressed(Keys.Enter))
            {
                currentCooldown = menuCooldown;
                
                if (selectedGameOverOption == 0) // Restart
                {
                    StartNewGame();
                }
                else if (selectedGameOverOption == 1) // Main Menu
                {
                    // Apenas muda para o menu principal sem reiniciar o jogo
                    CurrentGameState = GameState.MainMenu;
                    selectedMainMenuOption = 0;
                }
                else if (selectedGameOverOption == 2) // Exit
                {
                    Exit();
                }
            }
        }
    }

    private bool IsKeyPressed(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }

    private void StartNewGame()
    {
        score = 0;
        Lives = 3;
        enemies.Clear();
        bullets.Clear();
        player.Position = new Vector2(400, 500);
        enemySpawnTimer = spawnInterval;
        CurrentGameState = GameState.Playing;
    }

    private void UpdateGame(GameTime gameTime)
    {
        if (IsKeyPressed(Keys.Escape))
        {
            CurrentGameState = GameState.MainMenu;
            selectedMainMenuOption = 0;
            currentCooldown = menuCooldown;
            return;
        }

        player.Update(gameTime);

        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        enemySpawnTimer -= elapsed;
        if (enemySpawnTimer <= 0)
        {
            enemies.Add(new Enemy(enemyTexture,
                new Vector2(
                    new System.Random().Next(0, ScreenWidth - enemyTexture.Width),
                    -enemyTexture.Height),
                2f));
            enemySpawnTimer = spawnInterval;
        }

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            enemies[i].Update(gameTime);

            if (enemies[i].Position.Y > ScreenHeight)
            {
                Lives = Math.Max(0, Lives - 1);
                enemies.RemoveAt(i);
                playerDeathSound.Play();

                if (Lives <= 0)
                {
                    CurrentGameState = GameState.GameOver;
                    selectedGameOverOption = 0;
                    foreach (var enemy in enemies)
                        enemy.IsActive = false;
                    foreach (var bullet in bullets)
                        bullet.IsActive = false;
                }
                continue;
            }

            if (enemies[i].Hitbox.Intersects(player.Hitbox))
            {
                Lives = Math.Max(0, Lives - 1);
                enemies[i].IsActive = false;
                playerDeathSound.Play();

                if (Lives <= 0)
                {
                    CurrentGameState = GameState.GameOver;
                    selectedGameOverOption = 0;
                    foreach (var enemy in enemies)
                        enemy.IsActive = false;
                    foreach (var bullet in bullets)
                        bullet.IsActive = false;
                }
            }
        }

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            bullets[i].Update(gameTime);

            if (bullets[i].Position.Y < 0 || !bullets[i].IsActive)
            {
                bullets.RemoveAt(i);
                continue;
            }

            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                if (enemies[j].IsActive && bullets[i].IsActive &&
                    bullets[i].Hitbox.Intersects(enemies[j].Hitbox))
                {
                    bullets[i].IsActive = false;
                    enemies[j].IsActive = false;
                    score += 100;
                    explosionSound.Play();
                    break;
                }
            }
        }

        enemies.RemoveAll(e => !e.IsActive);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise);

        // Desenha o fundo em todos os estados
        foreach (var layer in parallaxLayers)
        {
            layer.Draw(spriteBatch);
        }

        switch (CurrentGameState)
        {
            case GameState.MainMenu:
                DrawMainMenu();
                break;
                
            case GameState.Playing:
                DrawGame();
                break;
                
            case GameState.GameOver:
                DrawGame();
                DrawGameOverMenu();
                break;
        }

        spriteBatch.End();
    }

    private void DrawMainMenu()
    {
        // Título do jogo
        string title = "SPACE SHOOTER";
        Vector2 titleSize = font.MeasureString(title);
        spriteBatch.DrawString(font, title, 
            new Vector2(ScreenWidth / 2 - titleSize.X / 2, ScreenHeight / 4), 
            Color.White);

        // Opções do menu
        for (int i = 0; i < mainMenuOptions.Length; i++)
        {
            Color color = (i == selectedMainMenuOption) ? Color.Yellow : Color.White;
            Vector2 optionSize = font.MeasureString(mainMenuOptions[i]);
            spriteBatch.DrawString(font, mainMenuOptions[i],
                new Vector2(ScreenWidth / 2 - optionSize.X / 2, ScreenHeight / 2 + i * 50),
                color);
        }

        // Instruções
        string instructions = "Use UP/DOWN to navigate, ENTER to select";
        Vector2 instructionsSize = font.MeasureString(instructions);
        spriteBatch.DrawString(font, instructions,
            new Vector2(ScreenWidth / 2 - instructionsSize.X / 2, ScreenHeight - 50),
            Color.Gray);
    }

    private void DrawGame()
    {
        foreach (var bullet in bullets)
            bullet.Draw(spriteBatch);

        foreach (var enemy in enemies)
            enemy.Draw(spriteBatch);

        player.Draw(spriteBatch);

        spriteBatch.DrawString(font, $"Score: {score}", new Vector2(10, 10), Color.White);

        for (int i = 0; i < Lives; i++)
        {
            spriteBatch.Draw(heartTexture,
                new Vector2(heartPosition.X - (i * heartSpacing), heartPosition.Y),
                Color.White);
        }
    }

    private void DrawGameOverMenu()
    {
        // Sobreposição semi-transparente
        Rectangle overlay = new Rectangle(0, 0, ScreenWidth, ScreenHeight);
        spriteBatch.Draw(pixelTexture, overlay, new Color(0, 0, 0, 150));

        // Título "Game Over"
        string gameOverText = "GAME OVER";
        Vector2 gameOverSize = font.MeasureString(gameOverText);
        spriteBatch.DrawString(font, gameOverText,
            new Vector2(ScreenWidth / 2 - gameOverSize.X / 2, ScreenHeight / 4),
            Color.Red);

        // Pontuação
        string scoreText = $"Score: {score}";
        Vector2 scoreSize = font.MeasureString(scoreText);
        spriteBatch.DrawString(font, scoreText,
            new Vector2(ScreenWidth / 2 - scoreSize.X / 2, ScreenHeight / 4 + 60),
            Color.White);

        // Opções do menu
        for (int i = 0; i < gameOverOptions.Length; i++)
        {
            Color color = (i == selectedGameOverOption) ? Color.Yellow : Color.White;
            Vector2 optionSize = font.MeasureString(gameOverOptions[i]);
            spriteBatch.DrawString(font, gameOverOptions[i],
                new Vector2(ScreenWidth / 2 - optionSize.X / 2, ScreenHeight / 2 + i * 50),
                color);
        }
    }
}