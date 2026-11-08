using System.Diagnostics;
using YMH219.FlappyBird.Models;

namespace YMH219.FlappyBird;

public sealed class MainForm : Form
{
    private readonly GameEngine _engine = new();
    private readonly GameCanvas _canvas;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly Label _score = new();
    private readonly Label _best = new();
    private readonly Label _state = new();
    private readonly Button _pause = new();
    private readonly Button _restart = new();

    public MainForm()
    {
        Text = "Flappy Bird | YMH 219";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(440, 600);
        MinimumSize = new Size(380, 500);
        BackColor = Color.FromArgb(15, 27, 43);
        Font = new Font("Segoe UI", 9f);
        KeyPreview = true;
        _canvas = new GameCanvas(_engine) { Dock = DockStyle.Fill, Margin = Padding.Empty };
        BuildLayout();
        WireEvents();
        RefreshHud();
        _timer.Start();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackColor,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18, 14, 18, 14)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));

        var header = new Panel { Dock = DockStyle.Fill };
        var title = new Label
        {
            Text = "FLAPPY BIRD",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(2, 0)
        };
        var subtitle = new Label
        {
            Text = "Bir boşluk bul, kanat çır ve devam et.",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(166, 185, 200),
            AutoSize = true,
            Location = new Point(4, 37)
        };
        _state.TextAlign = ContentAlignment.MiddleRight;
        _state.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        _state.ForeColor = Color.FromArgb(151, 218, 184);
        _state.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _state.Size = new Size(112, 28);
        _state.Location = new Point(Width - 160, 12);
        header.Controls.AddRange([title, subtitle, _state]);

        var stage = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4), BackColor = Color.FromArgb(39, 57, 73) };
        stage.Controls.Add(_canvas);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(0, 10, 0, 0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29));
        var stats = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        _score.ForeColor = Color.White;
        _score.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        _score.AutoSize = true;
        _best.ForeColor = Color.FromArgb(166, 185, 200);
        _best.Font = new Font("Segoe UI", 9);
        _best.AutoSize = true;
        stats.Controls.AddRange([_score, _best]);
        StyleButton(_pause, "Duraklat", Color.FromArgb(49, 71, 91));
        StyleButton(_restart, "Yeni Oyun", Color.FromArgb(56, 156, 119));
        footer.Controls.Add(stats, 0, 0);
        footer.Controls.Add(_pause, 1, 0);
        footer.Controls.Add(_restart, 2, 0);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(stage, 0, 1);
        root.Controls.Add(footer, 0, 2);
        Controls.Add(root);
    }

    private static void StyleButton(Button button, string text, Color color)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(5, 5, 0, 5);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.TabStop = false;
    }

    private void WireEvents()
    {
        _timer.Tick += (_, _) =>
        {
            var delta = (float)_clock.Elapsed.TotalSeconds;
            _clock.Restart();
            _engine.Update(delta);
            _canvas.Invalidate();
            RefreshHud();
        };
        _pause.Click += (_, _) =>
        {
            _engine.TogglePause();
            _canvas.Invalidate();
            RefreshHud();
            Focus();
        };
        _restart.Click += (_, _) =>
        {
            _engine.Restart();
            _canvas.Invalidate();
            RefreshHud();
            Focus();
        };
        KeyDown += HandleKeyDown;
        FormClosed += (_, _) => _timer.Dispose();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Space or Keys.Up)
        {
            _engine.Flap();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.P)
        {
            _engine.TogglePause();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Enter)
        {
            _engine.Restart();
            e.Handled = true;
        }
        else
        {
            return;
        }
        _canvas.Invalidate();
        RefreshHud();
    }

    private void RefreshHud()
    {
        _score.Text = $"SKOR  {_engine.Score:00}";
        _best.Text = $"REKOR  {_engine.BestScore:00}";
        _state.Text = _engine.State switch
        {
            GameState.Ready => "HAZIR",
            GameState.Playing => "OYUNDA",
            GameState.Paused => "DURAKLADI",
            GameState.GameOver => "OYUN BİTTİ",
            _ => string.Empty
        };
        _state.ForeColor = _engine.State switch
        {
            GameState.Playing => Color.FromArgb(151, 218, 184),
            GameState.GameOver => Color.FromArgb(255, 154, 135),
            GameState.Paused => Color.FromArgb(250, 212, 132),
            _ => Color.FromArgb(166, 185, 200)
        };
        _pause.Text = _engine.State == GameState.Paused ? "Devam" : "Duraklat";
        _pause.Enabled = _engine.State is GameState.Playing or GameState.Paused;
    }
}


