using System.Diagnostics;
using System.Text.Json;

namespace TrialsRisingHelper;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class MainForm : Form
{
    private const string CreatorName = "xxxEMUxxx";
    private const string AppVersion = "1.1.0";

    private const string TrialsRisingUri = "uplay://launch/3601/0";
    private const int SafetyTimeoutSeconds = 300;

    private const string EmailAddress = "Dev_Emu_2026@hotmail.com";
    private const string KoFiUrl = "https://ko-fi.com/xxxemuxxx";

    private readonly NumericUpDown triggerInterval = null!;
    private readonly NumericUpDown postGameDuration = null!;

    private readonly Label statusLabel = null!;
    private readonly Label detectedLabel = null!;
    private readonly Label timerLabel = null!;

    private readonly Button startButton = null!;
    private readonly Button stopButton = null!;

    private CancellationTokenSource? cancellationTokenSource;

    private readonly string configDirectory;
    private readonly string configFile;

    private HelperConfig config = new();

    private static readonly string[] UbisoftConnectPaths =
    {
        @"C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\UbisoftConnect.exe",
        @"C:\Program Files\Ubisoft\Ubisoft Game Launcher\UbisoftConnect.exe"
    };

    public MainForm()
    {
        configDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "TrialsRisingHelper");

        configFile = Path.Combine(
            configDirectory,
            "config.json");

        Text = "Trials Rising Startup Helper";
        ClientSize = new Size(760, 650);
        MinimumSize = new Size(760, 650);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(10, 18, 32);
        ForeColor = Color.White;

        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(28),
            BackColor = Color.FromArgb(10, 18, 32)
        };

        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // ============================================================
        // HEADER
        // ============================================================

        var header = new Panel
        {
            Height = 75,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 27, 47),
            Margin = new Padding(0, 0, 0, 18)
        };

        var title = new Label
        {
            Text = "TRIALS RISING",
            AutoSize = true,
            Font = new Font("Segoe UI", 23, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(18, 10)
        };

        var subtitle = new Label
        {
            Text = "STARTUP HELPER",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 211, 200),
            Location = new Point(25, 50)
        };

        var version = new Label
        {
            Text = $"v{AppVersion}",
            AutoSize = true,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(130, 150, 175),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        version.Location = new Point(
            header.Width - 60,
            15);

        header.Resize += (_, _) =>
        {
            version.Location = new Point(
                header.Width - version.Width - 18,
                15);
        };

        header.Controls.Add(title);
        header.Controls.Add(subtitle);
        header.Controls.Add(version);

        main.Controls.Add(header);

        // ============================================================
        // INFO CARD
        // ============================================================

        var infoCard = CreateCard();
        infoCard.Height = 125;
        var infoTitle = new Label
        {
            Text = "BEFORE YOU START",
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 211, 200),
            Location = new Point(18, 10)
        };

        var infoText = new Label
        {
            Text =
                "• Ubisoft Connect must already be signed in.\r\n" +
                "• Enable \"Remember me\" / automatic sign-in if available.\r\n" +
                "• Steam will be closed automatically before launch.\r\n" +
                "• Your Ubisoft credentials are never stored or handled by this helper.",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(205, 215, 230),
            Location = new Point(18, 35)
        };

        infoCard.Controls.Add(infoTitle);
        infoCard.Controls.Add(infoText);

        main.Controls.Add(infoCard);

        // ============================================================
        // SETTINGS
        // ============================================================

        var settingsTitle = new Label
        {
            Text = "SETTINGS",
            AutoSize = true,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            Margin = new Padding(0, 15, 0, 8)
        };

        main.Controls.Add(settingsTitle);

        var settingsCard = CreateCard();
        settingsCard.Height = 110;

        var triggerText = new Label
        {
            Text = "Ubisoft trigger interval",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(205, 215, 230),
            Location = new Point(18, 18)
        };

        triggerInterval = new NumericUpDown
        {
            Minimum = 0.5M,
            Maximum = 3.0M,
            Increment = 0.1M,
            DecimalPlaces = 1,
            Value = 1.0M,
            Width = 85,
            Height = 30,
            Location = new Point(250, 13),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(23, 38, 61),
            ForeColor = Color.White
        };

        var triggerUnit = new Label
        {
            Text = "seconds",
            AutoSize = true,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(130, 150, 175),
            Location = new Point(345, 19)
        };

        var durationText = new Label
        {
            Text = "Continue after game detection",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(205, 215, 230),
            Location = new Point(18, 67)
        };

        postGameDuration = new NumericUpDown
        {
            Minimum = 10,
            Maximum = 300,
            Increment = 10,
            DecimalPlaces = 0,
            Value = 60,
            Width = 85,
            Height = 30,
            Location = new Point(250, 62),
            Font = new Font("Segoe UI", 10),
            BackColor = Color.FromArgb(23, 38, 61),
            ForeColor = Color.White
        };

        var durationUnit = new Label
        {
            Text = "seconds",
            AutoSize = true,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(130, 150, 175),
            Location = new Point(345, 68)
        };

        settingsCard.Controls.Add(triggerText);
        settingsCard.Controls.Add(triggerInterval);
        settingsCard.Controls.Add(triggerUnit);
        settingsCard.Controls.Add(durationText);
        settingsCard.Controls.Add(postGameDuration);
        settingsCard.Controls.Add(durationUnit);

        main.Controls.Add(settingsCard);

        // ============================================================
        // STATUS
        // ============================================================

        var statusCard = CreateCard();

        statusCard.Dock = DockStyle.Fill;
        statusCard.Margin = new Padding(0, 15, 0, 15);

        var statusTitle = new Label
        {
            Text = "STATUS",
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 211, 200),
            Location = new Point(18, 14)
        };

        statusLabel = new Label
        {
            Text = "Ready to start.",
            AutoSize = true,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(18, 42)
        };

        detectedLabel = new Label
        {
            Text = "●  Trials Rising not detected",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(130, 150, 175),
            Location = new Point(18, 78)
        };

        timerLabel = new Label
        {
            Text = "Safety timeout: 5:00",
            AutoSize = true,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(130, 150, 175),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        statusCard.Resize += (_, _) =>
        {
            timerLabel.Location = new Point(
                statusCard.Width - timerLabel.Width - 18,
                17);
        };

        statusCard.Controls.Add(statusTitle);
        statusCard.Controls.Add(statusLabel);
        statusCard.Controls.Add(detectedLabel);
        statusCard.Controls.Add(timerLabel);

        main.Controls.Add(statusCard);

        // ============================================================
        // START / STOP BUTTONS
        // ============================================================

        var buttonPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 0, 0, 5)
        };

        startButton = CreateButton(
            "▶  START",
            Color.FromArgb(52, 211, 200),
            Color.FromArgb(7, 25, 39));

        stopButton = CreateButton(
            "■  STOP",
            Color.FromArgb(35, 49, 72),
            Color.White);

        stopButton.Enabled = false;

        startButton.Click += StartButton_Click;
        stopButton.Click += StopButton_Click;

        buttonPanel.Controls.Add(startButton);
        buttonPanel.Controls.Add(stopButton);

        main.Controls.Add(buttonPanel);

        // ============================================================
        // FOOTER
        // ============================================================

        var footerPanel = new Panel
        {
            Height = 52,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 0)
        };

        var footerText = new Label
        {
            Text = $"Made by {CreatorName}  •  Trials Rising Startup Helper",
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(90, 110, 135),
            Location = new Point(0, 4)
        };

        footerPanel.Controls.Add(footerText);

        // Email button
        var emailButton = CreateFooterButton(
            "✉  EMAIL ME",
            Color.FromArgb(25, 42, 64));

        emailButton.Click += (_, _) =>
        {
            OpenUrl(
                $"mailto:{EmailAddress}");
        };

        // Donate button
        var donateButton = CreateFooterButton(
            "♥  DONATE",
            Color.FromArgb(52, 211, 200));

        donateButton.ForeColor =
            Color.FromArgb(7, 25, 39);

        donateButton.Click += (_, _) =>
        {
            OpenUrl(KoFiUrl);
        };

        footerPanel.Controls.Add(emailButton);
        footerPanel.Controls.Add(donateButton);

        footerPanel.Resize += (_, _) =>
        {
            donateButton.Location = new Point(
                footerPanel.Width - donateButton.Width,
                0);

            emailButton.Location = new Point(
                footerPanel.Width -
                donateButton.Width -
                emailButton.Width -
                8,
                0);
        };

        main.Controls.Add(footerPanel);

        Controls.Add(main);

        LoadConfig();
    }

    // ================================================================
    // UI HELPERS
    // ================================================================

    private static Panel CreateCard()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(17, 30, 51),
            Padding = new Padding(15),
            Margin = new Padding(0)
        };
    }

    private static Button CreateButton(
        string text,
        Color backColor,
        Color foreColor)
    {
        var button = new Button
        {
            Text = text,
            Width = 160,
            Height = 45,
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = foreColor,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 12, 0)
        };

        button.FlatAppearance.BorderSize = 0;

        return button;
    }

    private static Button CreateFooterButton(
        string text,
        Color backColor)
    {
        var button = new Button
        {
            Text = text,
            Width = 110,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White,
            Font = new Font(
                "Segoe UI",
                8,
                FontStyle.Bold),
            Cursor = Cursors.Hand,
            FlatAppearance =
            {
                BorderSize = 0
            }
        };

        return button;
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
        }
        catch
        {
            // Ignore browser/email client errors.
        }
    }

    // ================================================================
    // CONFIG
    // ================================================================

    private void LoadConfig()
    {
        try
        {
            if (!File.Exists(configFile))
                return;

            string json =
                File.ReadAllText(configFile);

            HelperConfig? loaded =
                JsonSerializer.Deserialize<HelperConfig>(
                    json);

            if (loaded == null)
                return;

            if (loaded.TriggerInterval >= 0.5 &&
                loaded.TriggerInterval <= 3.0)
            {
                triggerInterval.Value =
                    (decimal)Math.Round(
                        loaded.TriggerInterval,
                        1);
            }

            if (loaded.PostGameDuration >= 10 &&
                loaded.PostGameDuration <= 300)
            {
                postGameDuration.Value =
                    loaded.PostGameDuration;
            }

            config = loaded;
        }
        catch
        {
            // Defaults are used.
        }
    }

    private void SaveConfig()
    {
        try
        {
            Directory.CreateDirectory(
                configDirectory);

            config.TriggerInterval =
                (double)triggerInterval.Value;

            config.PostGameDuration =
                (int)postGameDuration.Value;

            string json =
                JsonSerializer.Serialize(
                    config,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                configFile,
                json);
        }
        catch
        {
            // Configuration saving is non-critical.
        }
    }

    // ================================================================
    // START / STOP
    // ================================================================

    private async void StartButton_Click(
        object? sender,
        EventArgs e)
    {
        if (cancellationTokenSource != null)
            return;

        SaveConfig();

        startButton.Enabled = false;
        stopButton.Enabled = true;

        triggerInterval.Enabled = false;
        postGameDuration.Enabled = false;

        SetGameDetected(false);
        SetStatus("Preparing...");
        SetTimer("Safety timeout: 5:00");

        cancellationTokenSource =
            new CancellationTokenSource();

        try
        {
            await RunHelperAsync(
                cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            SetStatus("Stopped.");
            SetTimer("Stopped");
        }
        catch (Exception ex)
        {
            SetStatus(
                "Error: " + ex.Message);

            SetTimer("Error");
        }
        finally
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            startButton.Enabled = true;
            stopButton.Enabled = false;

            triggerInterval.Enabled = true;
            postGameDuration.Enabled = true;
        }
    }

    private void StopButton_Click(
        object? sender,
        EventArgs e)
    {
        cancellationTokenSource?.Cancel();
    }

    // ================================================================
    // HELPER LOGIC
    // ================================================================

    private async Task RunHelperAsync(
        CancellationToken cancellationToken)
    {
        SetStatus("Closing Steam...");
        SetTimer("Preparing");

        CloseSteam();

        await Task.Delay(
            1500,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        string? ubisoftPath =
            FindUbisoftConnect();

        if (ubisoftPath == null)
        {
            throw new FileNotFoundException(
                "UbisoftConnect.exe could not be found.");
        }

        SetStatus(
            "Starting Ubisoft Connect...");

        StartExecutable(
            ubisoftPath);

        await Task.Delay(
            3000,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        SetStatus(
            "Starting Trials Rising...");

        if (!TryLaunchTrialsRising())
        {
            throw new Exception(
                "Could not start Trials Rising through Ubisoft Connect.");
        }

        SetStatus(
            "Trials Rising is starting...");

        Stopwatch stopwatch =
            Stopwatch.StartNew();

        bool gameDetected = false;
        DateTime? gameDetectedAt = null;

        double intervalSeconds =
            (double)triggerInterval.Value;

        int postDurationSeconds =
            (int)postGameDuration.Value;

        while (
            stopwatch.Elapsed <
            TimeSpan.FromSeconds(
                SafetyTimeoutSeconds))
        {
            cancellationToken.ThrowIfCancellationRequested();

            int safetyRemaining =
                SafetyTimeoutSeconds -
                (int)stopwatch.Elapsed.TotalSeconds;

            TimeSpan safetyTime =
                TimeSpan.FromSeconds(
                    Math.Max(
                        0,
                        safetyRemaining));

            SetTimer(
                $"Safety timeout: {safetyTime:mm\\:ss}");

            bool gameRunning =
                IsTrialsRisingRunning();

            if (gameRunning &&
                !gameDetected)
            {
                gameDetected = true;

                gameDetectedAt =
                    DateTime.UtcNow;

                SetGameDetected(true);

                SetStatus(
                    $"Game detected — continuing for {postDurationSeconds}s");
            }

            if (gameDetected &&
                gameDetectedAt.HasValue)
            {
                TimeSpan afterDetection =
                    DateTime.UtcNow -
                    gameDetectedAt.Value;

                if (afterDetection >=
                    TimeSpan.FromSeconds(
                        postDurationSeconds))
                {
                    SetStatus(
                        "Startup sequence completed.");

                    SetTimer("Completed");

                    return;
                }

                int remaining =
                    Math.Max(
                        0,
                        postDurationSeconds -
                        (int)afterDetection.TotalSeconds);

                SetStatus(
                    $"Game detected — trigger active ({remaining}s)");
            }
            else
            {
                SetStatus(
                    "Waiting for Trials Rising...");
            }

            TriggerUbisoftConnect(
                ubisoftPath);

            await Task.Delay(
                TimeSpan.FromSeconds(
                    intervalSeconds),
                cancellationToken);
        }

        SetStatus(
            "Safety timeout reached.");

        SetTimer(
            "5:00 timeout reached");
    }

    // ================================================================
    // UBISOFT / TRIALS
    // ================================================================

    private static bool TryLaunchTrialsRising()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = TrialsRisingUri,
                UseShellExecute = true
            };

            Process.Start(psi);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void TriggerUbisoftConnect(
        string path)
    {
        try
        {
            StartExecutable(path);
        }
        catch
        {
            // Next interval will try again.
        }
    }

    private static void StartExecutable(
        string path)
    {
        var psi = new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        };

        Process.Start(psi);
    }

    private static string? FindUbisoftConnect()
    {
        foreach (string path in UbisoftConnectPaths)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private static bool IsTrialsRisingRunning()
    {
        try
        {
            return Process
                .GetProcessesByName(
                    "trialsrising")
                .Length > 0;
        }
        catch
        {
            return false;
        }
    }

    // ================================================================
    // STEAM
    // ================================================================

    private static void CloseSteam()
    {
        Process[] processes;

        try
        {
            processes =
                Process.GetProcessesByName(
                    "steam");
        }
        catch
        {
            return;
        }

        foreach (Process process in processes)
        {
            try
            {
                if (!process.HasExited)
                    process.CloseMainWindow();
            }
            catch
            {
                // Ignore individual process errors.
            }
            finally
            {
                process.Dispose();
            }
        }

        Thread.Sleep(2000);

        try
        {
            processes =
                Process.GetProcessesByName(
                    "steam");
        }
        catch
        {
            return;
        }

        foreach (Process process in processes)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
            }
            catch
            {
                // Ignore individual process errors.
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    // ================================================================
    // UI STATUS
    // ================================================================

    private void SetStatus(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(
                new Action(
                    () => SetStatus(text)));

            return;
        }

        statusLabel.Text = text;
    }

    private void SetGameDetected(bool detected)
    {
        if (InvokeRequired)
        {
            BeginInvoke(
                new Action(
                    () => SetGameDetected(detected)));

            return;
        }

        detectedLabel.Text = detected
            ? "●  Trials Rising detected"
            : "●  Trials Rising not detected";

        detectedLabel.ForeColor = detected
            ? Color.FromArgb(52, 211, 200)
            : Color.FromArgb(130, 150, 175);
    }

    private void SetTimer(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(
                new Action(
                    () => SetTimer(text)));

            return;
        }

        timerLabel.Text = text;
    }
}

// ================================================================
// CONFIG MODEL
// ================================================================

public sealed class HelperConfig
{
    public double TriggerInterval { get; set; } = 1.0;

    public int PostGameDuration { get; set; } = 90;
}