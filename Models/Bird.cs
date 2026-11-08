namespace YMH219.FlappyBird.Models;

public sealed class Bird
{
    public const float Size = 34f;
    public float X { get; set; } = 118f;
    public float Y { get; set; } = 285f;
    public float VelocityY { get; set; }
    public RectangleF Bounds => new(X - Size / 2, Y - Size / 2, Size, Size);
}
