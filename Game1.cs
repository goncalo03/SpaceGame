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

    public bool GameOver { get; set; }
    public int Lives { get; set; } = 3;

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
        GameOver = false;
    }

    public void ShootBullet(Vector2 position)
    {
        bullets.Add(new Bullet(bulletTexture, position, 8f, -1));
        shootSound.Play();
    }

    protected override void Update(GameTime gameTime)
    {
        foreach (var layer in parallaxLayers)
        {
            layer.Update(gameTime);
        }

        if (GameOver)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                score = 0;
                Lives = 3;
                enemies.Clear();
                bullets.Clear();
                player.Position = new Vector2(400, 500);
                GameOver = false;
                enemySpawnTimer = spawnInterval;
            }
            return;
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        player.Update(gameTime);

        enemySpawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
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
                    GameOver = true;
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
                    GameOver = true;
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

        foreach (var layer in parallaxLayers)
        {
            layer.Draw(spriteBatch);
        }

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

        if (GameOver)
        {
            string gameOverText = $"GAME OVER\nScore: {score}\nPress ENTER to Restart";
            Vector2 textSize = font.MeasureString(gameOverText);

            spriteBatch.DrawString(font, gameOverText,
                new Vector2(ScreenWidth / 2 - textSize.X / 2, ScreenHeight / 2 - textSize.Y / 2),
                Color.White);
        }

        spriteBatch.End();
    }
}