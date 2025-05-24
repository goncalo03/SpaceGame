using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ParallaxLayer
{
    public Texture2D Texture { get; set; }
    public float Speed { get; set; }
    public Vector2[] Positions { get; private set; }
    public float LayerDepth { get; set; }

    public ParallaxLayer(Texture2D texture, float speed, float depth)
    {
        Texture = texture;
        Speed = speed;
        LayerDepth = depth;
        Positions = new Vector2[2];
        ResetPositions();
    }

    private void ResetPositions()
    {
        Positions[0] = Vector2.Zero;
        Positions[1] = new Vector2(Texture.Width, 0);
    }

    public void Update(GameTime gameTime)
    {
        float movement = Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = 0; i < Positions.Length; i++)
        {
            Positions[i].X -= movement;

            if (Positions[i].X <= -Texture.Width)
            {
                Positions[i].X = Positions[(i + 1) % Positions.Length].X + Texture.Width;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var position in Positions)
        {
            spriteBatch.Draw(Texture, position, null, Color.White, 0f,
                Vector2.Zero, 1f, SpriteEffects.None, LayerDepth);
        }
    }
}