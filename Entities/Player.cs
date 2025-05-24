using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Player
{
    public Texture2D Texture { get; set; }
    public Vector2 Position { get; set; }
    public Rectangle Hitbox => new Rectangle(
        (int)Position.X + Texture.Width / 4,
        (int)Position.Y + Texture.Height / 4,
        Texture.Width / 2,
        Texture.Height / 2);
    public bool IsActive { get; set; } = true;

    private float shootCooldown = 0.3f;
    private float currentCooldown = 0f;
    private bool canShoot = true;

    public void Update(GameTime gameTime)
    {
        if (!IsActive) return;

        // Atualiza o cooldown
        if (!canShoot)
        {
            currentCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (currentCooldown <= 0)
            {
                canShoot = true;
            }
        }

        var keyboardState = Keyboard.GetState();
        float speed = 300f * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            Position = new Vector2(MathHelper.Clamp(Position.X - speed, 0, Game1.ScreenWidth - Texture.Width), Position.Y);

        if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            Position = new Vector2(MathHelper.Clamp(Position.X + speed, 0, Game1.ScreenWidth - Texture.Width), Position.Y);

        if (keyboardState.IsKeyDown(Keys.Space) && canShoot)
        {
            Vector2 bulletPosition = new Vector2(
                Position.X + (Texture.Width / 2) - (Game1.bulletTexture.Width / 2),
                Position.Y);

            Game1.Instance.ShootBullet(bulletPosition);
            canShoot = false;
            currentCooldown = shootCooldown;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsActive)
            spriteBatch.Draw(Texture, Position, Color.White);
    }
}