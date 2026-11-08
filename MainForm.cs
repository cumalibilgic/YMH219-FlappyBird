using System.Drawing.Drawing2D;

namespace YMH219.FlappyBird;

public sealed class MainForm : Form
{
    private readonly Label _title = new();
    private readonly Label _hint = new();

    public MainForm()
    {
        Text = "Flappy Bird | YMH 219";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(480, 640);
        MinimumSize = new Size(420, 560);
        BackColor = Color.FromArgb(15, 27, 43);
        DoubleBuffered = true;
        BuildShell();
    }

    private void BuildShell()
    {
        _title.Text = "FLAPPY BIRD";
        _title.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        _title.ForeColor = Color.White;
        _title.AutoSize = true;
        _title.Location = new Point(24, 20);
        Controls.Add(_title);

        _hint.Text = "SPACE / ↑  Kanat çırp   •   P  Duraklat";
        _hint.Font = new Font("Segoe UI", 9);
        _hint.ForeColor = Color.FromArgb(174, 194, 208);
        _hint.AutoSize = true;
        _hint.Location = new Point(26, 58);
        Controls.Add(_hint);
    }
}
