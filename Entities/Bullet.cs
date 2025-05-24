using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Bullet
{
    public Texture2D Texture { get; }
    public Vector2 Position { get; set; }
    public float Speed { get; }
    public int Direction { get; }
    public bool IsActive { get; set; } = true;
    public Rectangle Hitbox => new Rectangle(
        (int)Position.X + Texture.Width / 3,
        (int)Position.Y + Texture.Height / 3,
        Texture.Width / 3,
        Texture.Height / 3);

    public Bullet(Texture2D texture, Vector2 position, float speed, int direction)
    {
        Texture = texture;
        Position = position;
        Speed = speed;
        Direction = direction;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsActive) return;

        Position = new Vector2(
            Position.X,
            Position.Y + Speed * Direction * (float)gameTime.ElapsedGameTime.TotalSeconds * 60);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsActive)
            spriteBatch.Draw(Texture, Position, Color.White);
    }
}