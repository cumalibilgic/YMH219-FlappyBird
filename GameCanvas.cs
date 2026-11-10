using System.Drawing.Drawing2D;
using YMH219.FlappyBird.Models;

namespace YMH219.FlappyBird;

public sealed class GameCanvas : Control
{
    private readonly GameEngine _engine;

    public GameCanvas(GameEngine engine)
    {
        _engine = engine;
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(18, 31, 47);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var scale = Math.Min(ClientSize.Width / GameEngine.WorldWidth, ClientSize.Height / GameEngine.WorldHeight);
        if (scale <= 0)
            return;

        var offsetX = (ClientSize.Width - GameEngine.WorldWidth * scale) / 2;
        var offsetY = (ClientSize.Height - GameEngine.WorldHeight * scale) / 2;
        e.Graphics.TranslateTransform(offsetX, offsetY);
        e.Graphics.ScaleTransform(scale, scale);
        // Tepe ve bulut çizimleri oyun alanının dışına taşmasın.
        e.Graphics.SetClip(new RectangleF(0, 0, GameEngine.WorldWidth, GameEngine.WorldHeight));

        DrawBackground(e.Graphics);
        DrawPipes(e.Graphics);
        DrawGround(e.Graphics);
        DrawBird(e.Graphics);
        DrawScore(e.Graphics);
        DrawOverlay(e.Graphics);
    }

    private static void DrawBackground(Graphics g)
    {
        using var sky = new LinearGradientBrush(new RectangleF(0, 0, GameEngine.WorldWidth, GameEngine.GroundY),
            Color.FromArgb(100, 205, 231), Color.FromArgb(223, 244, 234), LinearGradientMode.Vertical);
        g.FillRectangle(sky, 0, 0, GameEngine.WorldWidth, GameEngine.GroundY);

        using var sun = new SolidBrush(Color.FromArgb(110, 255, 255, 235));
        g.FillEllipse(sun, 318, 45, 56, 56);
        DrawCloud(g, 48, 95, 1f);
        DrawCloud(g, 253, 150, .72f);
        DrawCloud(g, 181, 46, .52f);

        using var hillBack = new SolidBrush(Color.FromArgb(150, 198, 164));
        using var hillFront = new SolidBrush(Color.FromArgb(113, 176, 146));
        g.FillEllipse(hillBack, -95, 350, 310, 185);
        g.FillEllipse(hillBack, 150, 365, 360, 155);
        g.FillEllipse(hillFront, -110, 390, 340, 150);
        g.FillEllipse(hillFront, 205, 400, 300, 145);
    }

    private static void DrawCloud(Graphics g, float x, float y, float scale)
    {
        using var brush = new SolidBrush(Color.FromArgb(180, 255, 255, 255));
        var state = g.Save();
        g.TranslateTransform(x, y);
        g.ScaleTransform(scale, scale);
        g.FillEllipse(brush, 0, 13, 52, 27);
        g.FillEllipse(brush, 15, 0, 36, 35);
        g.FillEllipse(brush, 35, 9, 43, 30);
        g.Restore(state);
    }

    private void DrawPipes(Graphics g)
    {
        foreach (var pipe in _engine.Pipes)
        {
            DrawPipe(g, pipe.TopBounds(GameEngine.GapSize), true);
            DrawPipe(g, pipe.BottomBounds(GameEngine.GapSize, GameEngine.GroundY), false);
        }
    }

    private static void DrawPipe(Graphics g, RectangleF body, bool top)
    {
        using var shadow = new SolidBrush(Color.FromArgb(28, 27, 71, 67));
        g.FillRectangle(shadow, body.X + 5, body.Y + 4, body.Width, body.Height);
        using var fill = new LinearGradientBrush(body, Color.FromArgb(68, 188, 117), Color.FromArgb(25, 142, 98), LinearGradientMode.Horizontal);
        g.FillRectangle(fill, body);
        using var edge = new Pen(Color.FromArgb(20, 111, 78), 3);
        g.DrawRectangle(edge, body.X, body.Y, body.Width, body.Height);

        var capHeight = 20f;
        var capY = top ? body.Bottom - capHeight : body.Y;
        var cap = new RectangleF(body.X - 5, capY, body.Width + 10, capHeight);
        using var capFill = new LinearGradientBrush(cap, Color.FromArgb(98, 209, 126), Color.FromArgb(31, 151, 101), LinearGradientMode.Horizontal);
        g.FillRectangle(capFill, cap);
        g.DrawRectangle(edge, cap.X, cap.Y, cap.Width, cap.Height);
        using var highlight = new Pen(Color.FromArgb(110, 255, 255, 255), 3);
        g.DrawLine(highlight, body.X + 9, body.Y + (top ? 7 : 27), body.X + 9, Math.Max(body.Y + 8, body.Bottom - 8));
    }

    private void DrawGround(Graphics g)
    {
        using var dirt = new LinearGradientBrush(new RectangleF(0, GameEngine.GroundY, GameEngine.WorldWidth, 42),
            Color.FromArgb(233, 192, 110), Color.FromArgb(183, 130, 74), LinearGradientMode.Vertical);
        g.FillRectangle(dirt, 0, GameEngine.GroundY, GameEngine.WorldWidth, 42);
        using var grass = new SolidBrush(Color.FromArgb(107, 186, 105));
        g.FillRectangle(grass, 0, GameEngine.GroundY, GameEngine.WorldWidth, 9);
        using var stripe = new Pen(Color.FromArgb(70, 255, 255, 255), 2);
        for (var x = -10; x < GameEngine.WorldWidth; x += 28)
            g.DrawLine(stripe, x, GameEngine.GroundY + 22, x + 15, GameEngine.GroundY + 38);
    }

    private void DrawBird(Graphics g)
    {
        var angle = Math.Clamp(_engine.Bird.VelocityY / 12f, -24f, 55f);
        var state = g.Save();
        g.TranslateTransform(_engine.Bird.X, _engine.Bird.Y);
        g.RotateTransform(angle);
        using var shadow = new SolidBrush(Color.FromArgb(35, 35, 50, 55));
        g.FillEllipse(shadow, -13, -12, 33, 33);
        using var body = new SolidBrush(Color.FromArgb(255, 194, 72));
        using var outline = new Pen(Color.FromArgb(194, 119, 42), 2.4f);
        g.FillEllipse(body, -17, -17, 34, 34);
        g.DrawEllipse(outline, -17, -17, 34, 34);
        using var wing = new SolidBrush(Color.FromArgb(245, 159, 53));
        g.FillEllipse(wing, -13, 1, 18, 11);
        using var beak = new SolidBrush(Color.FromArgb(239, 113, 63));
        g.FillPolygon(beak, [new PointF(12, -1), new PointF(24, 3), new PointF(12, 7)]);
        using var eye = new SolidBrush(Color.White);
        g.FillEllipse(eye, 3, -12, 10, 11);
        using var pupil = new SolidBrush(Color.FromArgb(36, 48, 57));
        g.FillEllipse(pupil, 8, -9, 4, 5);
        g.Restore(state);
    }

    private void DrawScore(Graphics g)
    {
        using var font = new Font("Segoe UI", 25, FontStyle.Bold, GraphicsUnit.Pixel);
        using var shadow = new SolidBrush(Color.FromArgb(90, 30, 62, 74));
        using var white = new SolidBrush(Color.White);
        var text = _engine.Score.ToString();
        var size = g.MeasureString(text, font);
        var x = (GameEngine.WorldWidth - size.Width) / 2;
        g.DrawString(text, font, shadow, x + 2, 20 + 3);
        g.DrawString(text, font, white, x, 20);
    }

    private void DrawOverlay(Graphics g)
    {
        var heading = _engine.State switch
        {
            GameState.Ready => "HAZIR MISIN?",
            GameState.Paused => "DURAKLATILDI",
            GameState.GameOver => "OYUN BİTTİ",
            _ => string.Empty
        };
        if (heading.Length == 0)
            return;

        using var shade = new SolidBrush(Color.FromArgb(125, 17, 36, 49));
        g.FillRectangle(shade, 0, 0, GameEngine.WorldWidth, GameEngine.GroundY);
        using var card = new SolidBrush(Color.FromArgb(238, 255, 255, 255));
        using var border = new Pen(Color.FromArgb(40, 72, 91), 2);
        var box = new RectangleF(45, 174, GameEngine.WorldWidth - 90, 145);
        g.FillRoundedRectangle(card, Rectangle.Round(box), new Size(18, 18));
        g.DrawRoundedRectangle(border, Rectangle.Round(box), new Size(18, 18));
        using var title = new Font("Segoe UI", 23, FontStyle.Bold, GraphicsUnit.Pixel);
        using var detail = new Font("Segoe UI", 12, FontStyle.Regular, GraphicsUnit.Pixel);
        using var ink = new SolidBrush(Color.FromArgb(30, 52, 66));
        using var muted = new SolidBrush(Color.FromArgb(91, 112, 122));
        DrawCentered(g, heading, title, ink, 197);
        var line = _engine.State == GameState.GameOver
            ? $"Skor {_engine.Score}     •     Rekor {_engine.BestScore}\nBoşluk ile tekrar dene"
            : _engine.State == GameState.Paused ? "Devam etmek için P tuşuna bas" : "Başlamak için boşluk tuşuna bas";
        DrawCentered(g, line, detail, muted, 239);
    }

    private static void DrawCentered(Graphics g, string text, Font font, Brush brush, float y)
    {
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(text, font, brush, new RectangleF(55, y, GameEngine.WorldWidth - 110, 54), format);
    }
}

