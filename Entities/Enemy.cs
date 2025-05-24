using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Enemy
{
    public Texture2D Texture { get; }
    public Vector2 Position { get; set; }
    public float Speed { get; }
    public bool IsActive { get; set; } = true;
    public Rectangle Hitbox => new Rectangle(
        (int)Position.X + Texture.Width / 4,
        (int)Position.Y + Texture.Height / 4,
        Texture.Width / 2,
        Texture.Height / 2);

    public Enemy(Texture2D texture, Vector2 position, float speed)
    {
        Texture = texture;
        Position = position;
        Speed = speed;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsActive) return;

        Position = new Vector2(
            Position.X,
            Position.Y + Speed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsActive)
            spriteBatch.Draw(Texture, Position, Color.White);
    }
}