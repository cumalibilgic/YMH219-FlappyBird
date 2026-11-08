using YMH219.FlappyBird.Models;

namespace YMH219.FlappyBird;

public sealed class GameEngine
{
    public const float WorldWidth = 420f;
    public const float WorldHeight = 520f;
    public const float GroundY = 478f;
    public const float GapSize = 154f;

    private const float Gravity = 1150f;
    private const float FlapImpulse = -390f;
    private const float PipeSpeed = 190f;
    private const float PipeSpacing = 205f;
    private readonly string _bestScorePath;

    public Bird Bird { get; } = new();
    public List<PipePair> Pipes { get; } = [];
    public GameState State { get; private set; } = GameState.Ready;
    public int Score { get; private set; }
    public int BestScore { get; private set; }

    public GameEngine()
    {
        _bestScorePath = Path.Combine(Application.LocalUserAppDataPath, "best-score.txt");
        LoadBestScore();
        ResetWorld();
    }

    public void Flap()
    {
        if (State is GameState.Ready or GameState.GameOver)
        {
            ResetWorld();
            State = GameState.Playing;
        }

        if (State == GameState.Playing)
            Bird.VelocityY = FlapImpulse;
    }

    public void TogglePause()
    {
        if (State == GameState.Playing)
            State = GameState.Paused;
        else if (State == GameState.Paused)
            State = GameState.Playing;
    }

    public void Restart()
    {
        ResetWorld();
        State = GameState.Ready;
    }

    public void Update(float deltaSeconds)
    {
        if (State != GameState.Playing)
            return;

        // Uzun bir kare gecikmesinde fiziğin sıçramasını engelle.
        var delta = Math.Clamp(deltaSeconds, 0f, 0.04f);
        Bird.VelocityY += Gravity * delta;
        Bird.Y += Bird.VelocityY * delta;

        foreach (var pipe in Pipes)
        {
            pipe.X -= PipeSpeed * delta;
            if (!pipe.WasPassed && pipe.X + PipePair.Width < Bird.X)
            {
                pipe.WasPassed = true;
                Score++;
                if (Score > BestScore)
                {
                    BestScore = Score;
                    SaveBestScore();
                }
            }
        }

        RecyclePipes();
        CheckCollisions();
    }

    private void ResetWorld()
    {
        Bird.X = 118f;
        Bird.Y = 255f;
        Bird.VelocityY = 0f;
        Score = 0;
        Pipes.Clear();
        Pipes.Add(CreatePipe(300f));
        Pipes.Add(CreatePipe(505f));
    }

    private static PipePair CreatePipe(float x) => new()
    {
        X = x,
        GapCenterY = Random.Shared.Next(155, 330)
    };

    private void RecyclePipes()
    {
        foreach (var pipe in Pipes.Where(p => p.X + PipePair.Width < 0))
        {
            pipe.X = Pipes.Max(p => p.X) + PipeSpacing;
            pipe.GapCenterY = Random.Shared.Next(155, 330);
            pipe.WasPassed = false;
        }
    }

    private void CheckCollisions()
    {
        if (Bird.Y - Bird.Size / 2 <= 0 || Bird.Y + Bird.Size / 2 >= GroundY)
        {
            EndGame();
            return;
        }

        foreach (var pipe in Pipes)
        {
            if (Bird.Bounds.IntersectsWith(pipe.TopBounds(GapSize)) ||
                Bird.Bounds.IntersectsWith(pipe.BottomBounds(GapSize, GroundY)))
            {
                EndGame();
                return;
            }
        }
    }

    private void EndGame() => State = GameState.GameOver;

    private void LoadBestScore()
    {
        try
        {
            if (File.Exists(_bestScorePath) &&
                int.TryParse(File.ReadAllText(_bestScorePath), out var best))
                BestScore = Math.Max(0, best);
        }
        catch (IOException)
        {
            BestScore = 0;
        }
        catch (UnauthorizedAccessException)
        {
            BestScore = 0;
        }
    }

    private void SaveBestScore()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_bestScorePath)!);
            File.WriteAllText(_bestScorePath, BestScore.ToString());
        }
        catch (IOException)
        {
            // Skor kaydı başarısız olsa da oyun devam edebilir.
        }
        catch (UnauthorizedAccessException)
        {
            // Kısıtlı profillerde yerel kayıt kullanılamayabilir.
        }
    }
}
