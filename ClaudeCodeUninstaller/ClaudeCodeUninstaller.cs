using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClaudeCodeUninstaller
{
    public class ClaudeCodeUninstallerForm : Form
    {
        private RadioButton _claudeCodeOnlyRadio = null!;
        private RadioButton _claudeCodeAndNodeRadio = null!;
        private Button _uninstallButton = null!;
        private TextBox _logTextBox = null!;

        // Constants
        private const string CLAUDE_CODE_NPM_PACKAGE = "@anthropic/claude-code";
        private const string NODE_WINGET_ID = "OpenJS.NodeJS.LTS";

        // Language detection
        private static readonly bool IsJapanese = CultureInfo.CurrentUICulture.Name.StartsWith("ja");

        public ClaudeCodeUninstallerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Claude Code Uninstaller";
            Size = new System.Drawing.Size(600, 450);
            StartPosition = FormStartPosition.CenterScreen;

            var mainTable = CreateMainLayout();
            Controls.Add(mainTable);

            // Options panel
            var optionsPanel = CreateOptionsPanel();
            mainTable.Controls.Add(optionsPanel, 0, 0);

            // Log text box
            _logTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Consolas", 9.75F)
            };
            mainTable.Controls.Add(_logTextBox, 0, 1);

            // Uninstall button
            _uninstallButton = new Button { Text = IsJapanese ? "アンインストール実行" : "Execute Uninstall", Dock = DockStyle.Fill, Height = 40 };
            _uninstallButton.Click += UninstallButton_Click;
            mainTable.Controls.Add(_uninstallButton, 0, 2);
        }

        private TableLayoutPanel CreateMainLayout()
        {
            var mainTable = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                RowCount = 3, 
                Padding = new Padding(10) 
            };

            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            return mainTable;
        }

        private Panel CreateOptionsPanel()
        {
            var optionsPanel = new Panel { Height = 80, Dock = DockStyle.Fill };
            
            var optionsLabel = new Label 
            { 
                Text = IsJapanese ? "アンインストールオプション:" : "Uninstall Options:", 
                Location = new System.Drawing.Point(0, 0), 
                AutoSize = true 
            };
            optionsPanel.Controls.Add(optionsLabel);

            _claudeCodeOnlyRadio = new RadioButton 
            { 
                Text = IsJapanese ? "Claude Codeのみアンインストール" : "Uninstall Claude Code only", 
                Location = new System.Drawing.Point(0, 25), 
                AutoSize = true, 
                Checked = true 
            };
            
            _claudeCodeAndNodeRadio = new RadioButton 
            { 
                Text = IsJapanese ? "Claude CodeとNode.jsをアンインストール" : "Uninstall Claude Code and Node.js", 
                Location = new System.Drawing.Point(0, 50), 
                AutoSize = true 
            };

            optionsPanel.Controls.Add(_claudeCodeOnlyRadio);
            optionsPanel.Controls.Add(_claudeCodeAndNodeRadio);

            return optionsPanel;
        }

        private async void UninstallButton_Click(object? sender, EventArgs e)
        {
            _uninstallButton.Enabled = false;
            _logTextBox.Clear();

            try
            {
                if (!ConfirmUninstallation())
                {
                    Log(IsJapanese ? "アンインストールがキャンセルされました。" : "Uninstall cancelled.");
                    return;
                }

                await PerformUninstallation();
                ShowCompletionMessage();
            }
            catch (Exception ex)
            {
                Log(IsJapanese ? $"\nエラーが発生しました: {ex.Message}" : $"\nAn error occurred: {ex.Message}");
            }
            finally
            {
                _uninstallButton.Enabled = true;
            }
        }

        private bool ConfirmUninstallation()
        {
            var confirmMessage = _claudeCodeOnlyRadio.Checked
                ? (IsJapanese ? "Claude Codeをアンインストールします。よろしいですか？" : "Are you sure you want to uninstall Claude Code?")
                : (IsJapanese ? "Claude CodeとNode.jsをアンインストールします。よろしいですか？" : "Are you sure you want to uninstall Claude Code and Node.js?");

            var title = IsJapanese ? "確認" : "Confirmation";
            var result = MessageBox.Show(confirmMessage, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }

        private async Task PerformUninstallation()
        {
            await UninstallClaudeCodeAsync();

            if (_claudeCodeAndNodeRadio.Checked)
            {
                await UninstallNodeAsync();
            }

            Log(IsJapanese ? "\nアンインストール処理が完了しました。" : "\nUninstallation process finished.");
        }

        private static void ShowCompletionMessage()
        {
            var message = IsJapanese ? "アンインストール処理が完了しました。" : "Uninstallation process completed.";
            var title = IsJapanese ? "完了" : "Completed";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task UninstallClaudeCodeAsync()
        {
            Log(IsJapanese ? "Claude Codeをアンインストール中..." : "Uninstalling Claude Code...");
            await RunProcessAsync("npm.cmd", $"uninstall -g {CLAUDE_CODE_NPM_PACKAGE}");
            Log(IsJapanese ? "Claude Codeのアンインストールコマンドが完了しました。" : "Claude Code uninstall command finished.");
        }

        private async Task UninstallNodeAsync()
        {
            Log(IsJapanese ? "\nNode.jsをアンインストール中..." : "\nUninstalling Node.js...");
            Log(IsJapanese ? "この処理には管理者権限が必要で、確認プロンプトが表示される場合があります。" : "This may require administrator privileges and a confirmation prompt.");
            await RunProcessAsync("winget", $"uninstall --id {NODE_WINGET_ID} --accept-source-agreements");
            Log(IsJapanese ? "Node.jsのアンインストールコマンドが完了しました。" : "Node.js uninstall command finished.");
        }

        private async Task RunProcessAsync(string fileName, string arguments)
        {
            using var process = new Process
            {
                StartInfo = CreateProcessStartInfo(fileName, arguments),
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (_, e) => { if (e.Data != null) Log(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) Log((IsJapanese ? "エラー: " : "ERROR: ") + e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
        }

        private static ProcessStartInfo CreateProcessStartInfo(string fileName, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            EnhanceProcessEnvironment(startInfo);
            return startInfo;
        }

        private static void EnhanceProcessEnvironment(ProcessStartInfo startInfo)
        {
            var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            var enhancedPath = BuildEnhancedPath(currentPath);
            startInfo.EnvironmentVariables["PATH"] = enhancedPath;
        }

        private static string BuildEnhancedPath(string currentPath)
        {
            var pathBuilder = new System.Text.StringBuilder(currentPath);
            AddNodeJsPath(pathBuilder);
            AddNpmPaths(pathBuilder);
            return pathBuilder.ToString();
        }

        private static void AddNodeJsPath(System.Text.StringBuilder pathBuilder)
        {
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            if (programFiles != null)
            {
                var nodeJsPath = Path.Combine(programFiles, "nodejs");
                if (Directory.Exists(nodeJsPath))
                {
                    pathBuilder.Append(Path.PathSeparator).Append(nodeJsPath);
                }
            }
        }

        private static void AddNpmPaths(System.Text.StringBuilder pathBuilder)
        {
            var appData = Environment.GetEnvironmentVariable("APPDATA");
            if (appData != null)
            {
                var npmPath = Path.Combine(appData, "npm");
                if (Directory.Exists(npmPath))
                {
                    pathBuilder.Append(Path.PathSeparator).Append(npmPath);
                }
            }

            var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            if (userProfile != null)
            {
                var npmGlobalPath = Path.Combine(userProfile, "AppData", "Roaming", "npm");
                if (Directory.Exists(npmGlobalPath))
                {
                    pathBuilder.Append(Path.PathSeparator).Append(npmGlobalPath);
                }
            }
        }

        private void Log(string message)
        {
            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(new Action<string>(Log), message);
            }
            else
            {
                _logTextBox.AppendText(message + Environment.NewLine);
                _logTextBox.ScrollToCaret();
            }
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClaudeCodeUninstallerForm());
        }
    }

    public static class ProcessExtensions
    {
        public static Task<int> WaitForExitAsync(this Process process)
        {
            var tcs = new TaskCompletionSource<int>();
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) => tcs.TrySetResult(process.ExitCode);
            
            if (process.HasExited)
            {
                tcs.TrySetResult(process.ExitCode);
            }
            
            return tcs.Task;
        }
    }
}