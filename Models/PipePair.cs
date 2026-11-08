namespace YMH219.FlappyBird.Models;

public sealed class PipePair
{
    public const float Width = 66f;
    public float X { get; set; }
    public float GapCenterY { get; set; }
    public bool WasPassed { get; set; }

    public RectangleF TopBounds(float gapSize) => new(X, 0, Width, GapCenterY - gapSize / 2);
    public RectangleF BottomBounds(float gapSize, float groundY) =>
        new(X, GapCenterY + gapSize / 2, Width, groundY - (GapCenterY + gapSize / 2));
}
