using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodexLauncher
{
    public class CodexLauncherForm : Form
    {
        private TextBox _workDirTextBox = null!;
        private Button _browseButton = null!;
        private Button _startButton = null!;
        private TextBox _logTextBox = null!;
        private CheckBox _bypassApprovalsCheckBox = null!;

        // Constants
        private const string NODE_WINGET_ID = "OpenJS.NodeJS";
        private const string CODEX_NPM_PACKAGE = "@openai/codex";
        private const int RESTART_DELAY_MS = 3000;
        private const int UI_DELAY_MS = 1000;
        private const int PATH_DISPLAY_LENGTH = 200;

        // Language detection
        private static readonly bool IsJapanese = CultureInfo.CurrentUICulture.Name.StartsWith("ja");

        private readonly bool _autoStart;
        private readonly string? _savedWorkDir;

        public CodexLauncherForm(bool autoStart = false, string? savedWorkDir = null)
        {
            _autoStart = autoStart;
            _savedWorkDir = savedWorkDir;
            InitializeComponent();
            _workDirTextBox.Text = _savedWorkDir ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _bypassApprovalsCheckBox.Checked = true; // Default ON
        }

        private void InitializeComponent()
        {
            Text = "Codex Launcher";
            Size = new System.Drawing.Size(600, 400);
            StartPosition = FormStartPosition.CenterScreen;

            var mainTable = CreateMainLayout();
            Controls.Add(mainTable);

            // Work directory controls
            var workDirLabel = new Label 
            { 
                Text = IsJapanese ? "作業ディレクトリ:" : "Work Directory:", 
                Anchor = AnchorStyles.Left, 
                AutoSize = true, 
                Margin = new Padding(0, 5, 0, 0) 
            };
            mainTable.Controls.Add(workDirLabel, 0, 0);

            _workDirTextBox = new TextBox { Dock = DockStyle.Fill };
            mainTable.Controls.Add(_workDirTextBox, 1, 0);

            _browseButton = new Button { Text = "Browse...", Anchor = AnchorStyles.Right };
            _browseButton.Click += BrowseButton_Click;
            mainTable.Controls.Add(_browseButton, 2, 0);

            // Bypass approvals checkbox
            _bypassApprovalsCheckBox = new CheckBox 
            { 
                Text = IsJapanese ? "承認とサンドボックスをバイパス (--dangerously-bypass-approvals-and-sandbox)" : "Bypass approvals and sandbox (--dangerously-bypass-approvals-and-sandbox)",
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5)
            };
            mainTable.SetColumnSpan(_bypassApprovalsCheckBox, 3);
            mainTable.Controls.Add(_bypassApprovalsCheckBox, 0, 1);

            // Log text box
            _logTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Consolas", 9.75F)
            };
            mainTable.SetColumnSpan(_logTextBox, 3);
            mainTable.Controls.Add(_logTextBox, 0, 2);

            // Start button
            _startButton = new Button { Text = "Start Codex", Dock = DockStyle.Fill, Height = 40 };
            _startButton.Click += StartButton_Click;
            mainTable.SetColumnSpan(_startButton, 3);
            mainTable.Controls.Add(_startButton, 0, 3);
        }

        private TableLayoutPanel CreateMainLayout()
        {
            var mainTable = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                ColumnCount = 3, 
                RowCount = 4, 
                Padding = new Padding(10) 
            };

            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            return mainTable;
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _workDirTextBox.Text = dialog.SelectedPath;
            }
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            if (_autoStart)
            {
                Log(IsJapanese ? "再起動後の自動実行を開始します..." : "Starting auto-execution after restart...");
                await Task.Delay(1000);
                await PerformStartOperation();
            }
        }

        private async void StartButton_Click(object? sender, EventArgs e)
        {
            await PerformStartOperation();
        }
        
        private async Task PerformStartOperation()
        {
            _startButton.Enabled = false;
            if (!_autoStart) _logTextBox.Clear();

            try
            {
                var workDir = _workDirTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(workDir))
                {
                    ShowError(IsJapanese ? "作業ディレクトリを選択してください。" : "Please select a working directory.");
                    return;
                }

                await EnsureWorkingDirectoryExists(workDir);
                Log(IsJapanese ? $"作業ディレクトリ: {workDir}" : $"Working directory: {workDir}");

                var installationResult = await CheckAndInstallDependencies(workDir);
                if (installationResult.ShouldRestart)
                {
                    await HandleApplicationRestart(workDir);
                    return;
                }

                await StartCodexCli(workDir);
            }
            catch (Exception ex)
            {
                Log(IsJapanese ? $"エラーが発生しました: {ex.Message}" : $"An error occurred: {ex.Message}");
            }
            finally
            {
                _startButton.Enabled = true;
            }
        }

        private static void ShowError(string message)
        {
            var title = IsJapanese ? "エラー" : "Error";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private Task EnsureWorkingDirectoryExists(string workDir)
        {
            if (!Directory.Exists(workDir))
            {
                Log(IsJapanese ? "作業ディレクトリが存在しません。作成中..." : "Working directory does not exist. Creating...");
                Directory.CreateDirectory(workDir);
            }
            return Task.CompletedTask;
        }

        private async Task<(bool ShouldRestart, bool NodeInstalled, bool CodexInstalled)> CheckAndInstallDependencies(string workDir)
        {
            var nodeInstalled = await CheckAndInstallNodeAsync(workDir);
            var codexInstalled = await CheckAndInstallCodexCliAsync(workDir);
            
            return (nodeInstalled || codexInstalled, nodeInstalled, codexInstalled);
        }

        private async Task HandleApplicationRestart(string workDir)
        {
            Log(IsJapanese ? "\n新しいインストールが完了しました。" : "\nNew installations completed.");
            Log(IsJapanese ? $"環境変数を更新するため、{RESTART_DELAY_MS / 1000}秒後にアプリケーションを自動再起動します..." : $"Restarting application in {RESTART_DELAY_MS / 1000} seconds to update environment variables...");
            await Task.Delay(RESTART_DELAY_MS);
            Log(IsJapanese ? "アプリケーションを再起動中..." : "Restarting application...");
            RestartApplication(workDir);
        }

        private async Task StartCodexCli(string workDir)
        {
            Log(IsJapanese ? "\nCodex CLIを起動中..." : "\nStarting Codex CLI...");
            LogCurrentPath();

            var workingCommand = await FindWorkingCodexCommand(workDir);
            if (workingCommand == null)
            {
                Log(IsJapanese ? "エラー: 利用可能なCodex CLIコマンドが見つかりません。" : "ERROR: No working Codex CLI command found.");
                Log(IsJapanese ? "npmグローバルインストールを確認中..." : "Trying to check npm global installations...");
                await RunProcessAsync("npm", "list -g --depth=0", workDir);
                return;
            }

            LaunchCodexInNewWindow(workDir, workingCommand, _bypassApprovalsCheckBox.Checked);
            Log(IsJapanese ? "Codex CLIが正常に起動しました。ランチャーを終了します..." : "Codex CLI started successfully. Closing launcher...");
            await Task.Delay(UI_DELAY_MS);
            Application.Exit();
        }

        private void LogCurrentPath()
        {
            var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            var displayPath = currentPath.Length > PATH_DISPLAY_LENGTH 
                ? currentPath.Substring(0, PATH_DISPLAY_LENGTH) + "..." 
                : currentPath;
            Log(IsJapanese ? $"現在のPATH: {displayPath}" : $"Current PATH: {displayPath}");
        }

        private async Task<string?> FindWorkingCodexCommand(string workDir)
        {
            string[] codexCommands = { "codex.cmd", "codex" };
            
            foreach (var cmd in codexCommands)
            {
                Log(IsJapanese ? $"コマンドをテスト中: {cmd}" : $"Testing command: {cmd}");
                if (await IsCommandAvailableAsync(cmd, "--version", workDir))
                {
                    Log(IsJapanese ? $"動作するコマンドを発見: {cmd}" : $"Found working command: {cmd}");
                    return cmd;
                }
            }
            
            return null;
        }

        private static void LaunchCodexInNewWindow(string workDir, string command, bool bypassApprovalsAndSandbox)
        {
            var codexCommand = bypassApprovalsAndSandbox ? $"{command} --dangerously-bypass-approvals-and-sandbox" : command;
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start cmd.exe /k \"cd /d \"{workDir}\" && {codexCommand}\"",
                UseShellExecute = true,
            });
        }

        private async Task<bool> CheckAndInstallNodeAsync(string workDir)
        {
            Log(IsJapanese ? "\nNode.jsを確認中..." : "\nChecking for Node.js...");
            if (await IsCommandAvailableAsync("node", "--version", workDir))
            {
                Log(IsJapanese ? "Node.jsは既にインストールされています。" : "Node.js is already installed.");
                return false;
            }

            return await InstallNodeJs(workDir);
        }

        private async Task<bool> InstallNodeJs(string workDir)
        {
            Log(IsJapanese ? "Node.jsがインストールされていません。winget経由でインストール中..." : "Node.js is not installed. Installing via winget...");
            try
            {
                await RunProcessAsync("winget", $"install -e --id {NODE_WINGET_ID}", workDir);
                Log(IsJapanese ? "Node.jsのインストールが完了しました。" : "Node.js installation completed.");
                
                return await VerifyNodeInstallation(workDir);
            }
            catch (Exception ex)
            {
                Log(IsJapanese ? $"Node.jsのインストールに失敗しました: {ex.Message}" : $"Failed to install Node.js: {ex.Message}");
                Log(IsJapanese ? "https://nodejs.org/ からNode.jsを手動でインストールしてください。" : "Please install Node.js manually from https://nodejs.org/");
                return false;
            }
        }

        private async Task<bool> VerifyNodeInstallation(string workDir)
        {
            if (await IsCommandAvailableAsync("node", "--version", workDir))
            {
                Log(IsJapanese ? "Node.jsのインストールが正常に確認されました。" : "Node.js installation verified successfully.");
                return true;
            }
            
            Log(IsJapanese ? "警告: Node.jsのインストールを確認できませんでした。再起動またはNode.jsの手動インストールが必要な場合があります。" : "WARNING: Node.js installation could not be verified. You may need to restart or manually install Node.js.");
            return true; // Still recommend restart
        }

        private async Task<bool> CheckAndInstallCodexCliAsync(string workDir)
        {
            Log(IsJapanese ? "\nCodex CLIを確認中..." : "\nChecking for Codex CLI...");
            if (await IsCommandAvailableAsync("codex.cmd", "--version", workDir))
            {
                Log(IsJapanese ? "Codex CLIは既にインストールされています。" : "Codex CLI is already installed.");
                return false;
            }

            return await InstallCodexCli(workDir);
        }

        private async Task<bool> InstallCodexCli(string workDir)
        {
            Log(IsJapanese ? "Codex CLIがインストールされていません。npm経由でインストール中..." : "Codex CLI is not installed. Installing via npm...");
            try
            {
                await RunProcessAsync("npm", $"install -g {CODEX_NPM_PACKAGE}", workDir);
                Log(IsJapanese ? "Codex CLIのインストールが完了しました。" : "Codex CLI installation completed.");
                
                return await VerifyCodexInstallation(workDir);
            }
            catch (Exception ex)
            {
                Log(IsJapanese ? $"Codex CLIのインストールに失敗しました: {ex.Message}" : $"Failed to install Codex CLI: {ex.Message}");
                Log(IsJapanese ? "Node.js/npmが正しくインストールされていることを確認して、再試行してください。" : "Please ensure Node.js/npm is properly installed and try again.");
                return false;
            }
        }

        private async Task<bool> VerifyCodexInstallation(string workDir)
        {
            if (await IsCommandAvailableAsync("codex.cmd", "--version", workDir))
            {
                Log(IsJapanese ? "Codex CLIのインストールが正常に確認されました。" : "Codex CLI installation verified successfully.");
                return true;
            }
            
            if (await IsCommandAvailableAsync("codex", "--version", workDir))
            {
                Log(IsJapanese ? "Codex CLIのインストールが正常に確認されました（'codex'コマンドを使用）。" : "Codex CLI installation verified successfully (using 'codex' command).");
                return true;
            }
            
            Log(IsJapanese ? "警告: Codex CLIのインストールを確認できませんでした。再起動またはPATHの確認が必要な場合があります。" : "WARNING: Codex CLI installation could not be verified. You may need to restart or check your PATH.");
            return true; // Still recommend restart
        }

        private async Task<bool> IsCommandAvailableAsync(string command, string args, string workDir)
        {
            try
            {
                using var process = new Process { StartInfo = CreateProcessStartInfo(command, args, workDir) };
                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task RunProcessAsync(string fileName, string arguments, string workDir)
        {
            using var process = new Process
            {
                StartInfo = CreateProcessStartInfo(fileName, arguments, workDir),
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (_, e) => { if (e.Data != null) Log(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) Log((IsJapanese ? "エラー: " : "ERROR: ") + e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
        }

        private ProcessStartInfo CreateProcessStartInfo(string fileName, string arguments, string workDir)
        {
            // For npm and node commands, use cmd.exe wrapper to ensure proper environment
            if (fileName is "npm" or "node")
            {
                return CreateCmdProcessStartInfo(fileName, arguments, workDir);
            }

            var startInfo = CreateBasicProcessStartInfo(fileName, arguments, workDir);
            EnhanceProcessEnvironment(startInfo);
            return startInfo;
        }

        private static ProcessStartInfo CreateCmdProcessStartInfo(string fileName, string arguments, string workDir)
        {
            return new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {fileName} {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workDir
            };
        }

        private static ProcessStartInfo CreateBasicProcessStartInfo(string fileName, string arguments, string workDir)
        {
            return new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workDir
            };
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
                    pathBuilder.Insert(0, nodeJsPath + Path.PathSeparator);
                }
            }
        }

        private static void AddNpmPaths(System.Text.StringBuilder pathBuilder)
        {
            var appData = Environment.GetEnvironmentVariable("APPDATA");
            var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");

            if (appData != null)
            {
                var npmPath = Path.Combine(appData, "npm");
                if (Directory.Exists(npmPath))
                {
                    pathBuilder.Insert(0, npmPath + Path.PathSeparator);
                }
            }

            if (userProfile != null)
            {
                var npmGlobalPath = Path.Combine(userProfile, "AppData", "Roaming", "npm");
                if (Directory.Exists(npmGlobalPath))
                {
                    pathBuilder.Insert(0, npmGlobalPath + Path.PathSeparator);
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

        private void RestartApplication(string? workDir = null)
        {
            try
            {
                RefreshEnvironmentVariables();
                
                var currentExePath = Application.ExecutablePath;
                var arguments = string.Empty;
                
                if (!string.IsNullOrEmpty(workDir))
                {
                    arguments = $"--auto-start --work-dir \"{workDir}\"";
                }
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = currentExePath,
                    Arguments = arguments,
                    UseShellExecute = true
                });
                Application.Exit();
            }
            catch (Exception ex)
            {
                Log(IsJapanese ? $"アプリケーションの再起動に失敗しました: {ex.Message}" : $"Failed to restart application: {ex.Message}");
            }
        }
        
        private static void RefreshEnvironmentVariables()
        {
            try
            {
                TryRefreshenvCommand();
                TryRegistryRefresh();
            }
            catch
            {
                // If all methods fail, continue anyway
            }
        }

        private static void TryRefreshenvCommand()
        {
            try
            {
                using var refreshProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c refreshenv",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                refreshProcess.Start();
                refreshProcess.WaitForExit();
            }
            catch { /* Continue to next method */ }
        }

        private static void TryRegistryRefresh()
        {
            try
            {
                RefreshSystemPath();
                RefreshUserPath();
            }
            catch { /* Registry access might fail */ }
        }

        private static void RefreshSystemPath()
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
            var systemPath = key?.GetValue("PATH")?.ToString();
            if (!string.IsNullOrEmpty(systemPath))
            {
                Environment.SetEnvironmentVariable("PATH", systemPath, EnvironmentVariableTarget.Process);
            }
        }

        private static void RefreshUserPath()
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Environment");
            var userPath = key?.GetValue("PATH")?.ToString();
            if (!string.IsNullOrEmpty(userPath))
            {
                var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) ?? "";
                Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + userPath, EnvironmentVariableTarget.Process);
            }
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var (autoStart, workDir) = ParseCommandLineArgs(args);
            Application.Run(new CodexLauncherForm(autoStart, workDir));
        }
        
        private static (bool AutoStart, string? WorkDir) ParseCommandLineArgs(string[] args)
        {
            var autoStart = false;
            string? workDir = null;
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--auto-start":
                        autoStart = true;
                        break;
                    case "--work-dir":
                        if (i + 1 < args.Length)
                        {
                            workDir = args[i + 1];
                            i++;
                        }
                        break;
                }
            }
            
            return (autoStart, workDir);
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