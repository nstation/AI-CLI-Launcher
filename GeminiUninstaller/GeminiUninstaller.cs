using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeminiUninstaller
{
    public class UninstallerForm : Form
    {
        private RadioButton _geminiOnlyRadio = null!;
        private RadioButton _geminiAndNodeRadio = null!;
        private Button _uninstallButton = null!;
        private TextBox _logTextBox = null!;

        // Constants
        private const string GEMINI_NPM_PACKAGE = "@google/gemini-cli";
        private const string GEMINI_COMMAND_NAME = "gemini";
        private const string GEMINI_CONFIG_FOLDER = ".gemini";
        private const string NODE_WINGET_ID = "OpenJS.NodeJS";

        // Language detection
        private static readonly bool IsJapanese = CultureInfo.CurrentUICulture.Name.StartsWith("ja");

        public UninstallerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Gemini Uninstaller";
            Size = new System.Drawing.Size(900, 675);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

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
                Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point),
                BackColor = System.Drawing.Color.FromArgb(248, 248, 248)
            };
            mainTable.Controls.Add(_logTextBox, 0, 1);

            // Uninstall button
            _uninstallButton = new Button 
            { 
                Text = IsJapanese ? "アンインストール実行" : "Execute Uninstall", 
                Dock = DockStyle.Fill, 
                Height = 40,
                Font = new System.Drawing.Font("Yu Gothic UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
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
            var optionsPanel = new Panel { Height = 100, Dock = DockStyle.Fill };
            
            var optionsLabel = new Label 
            { 
                Text = IsJapanese ? "アンインストールオプション:" : "Uninstall Options:", 
                Location = new System.Drawing.Point(0, 0), 
                AutoSize = true,
                Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            optionsPanel.Controls.Add(optionsLabel);

            _geminiOnlyRadio = new RadioButton 
            { 
                Text = IsJapanese ? "Gemini CLIのみアンインストール" : "Uninstall Gemini CLI only", 
                Location = new System.Drawing.Point(0, 30), 
                AutoSize = true, 
                Checked = true,
                Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };
            
            _geminiAndNodeRadio = new RadioButton 
            { 
                Text = IsJapanese ? "Gemini CLIとNode.jsをアンインストール" : "Uninstall Gemini CLI and Node.js", 
                Location = new System.Drawing.Point(0, 65), 
                AutoSize = true,
                Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
            };

            optionsPanel.Controls.Add(_geminiOnlyRadio);
            optionsPanel.Controls.Add(_geminiAndNodeRadio);

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
            var confirmMessage = _geminiOnlyRadio.Checked
                ? (IsJapanese ? "Gemini CLIをアンインストールします。よろしいですか？" : "Are you sure you want to uninstall Gemini CLI?")
                : (IsJapanese ? "Gemini CLIとNode.jsをアンインストールします。よろしいですか？" : "Are you sure you want to uninstall Gemini CLI and Node.js?");

            var title = IsJapanese ? "確認" : "Confirmation";
            var result = MessageBox.Show(confirmMessage, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }

        private async Task PerformUninstallation()
        {
            await UninstallGeminiCliAsync();

            if (_geminiAndNodeRadio.Checked)
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

        private async Task UninstallGeminiCliAsync()
        {
            Log(IsJapanese ? "Gemini CLIの存在確認中..." : "Checking Gemini CLI installation...");
            var isInstalled = await CheckPackageInstalledAsync(GEMINI_NPM_PACKAGE);
            
            if (!isInstalled)
            {
                Log(IsJapanese ? "NPMパッケージが見つかりませんでした。直接ファイルを確認します..." : "NPM package not found. Checking files directly...");
            }
            else
            {
                Log(IsJapanese ? "NPMパッケージをアンインストール中..." : "Uninstalling NPM package...");
                var exitCode = await RunProcessAsync("npm.cmd", $"uninstall -g {GEMINI_NPM_PACKAGE}");
                
                if (exitCode == 0)
                {
                    Log(IsJapanese ? "NPMパッケージのアンインストールが完了しました。" : "NPM package uninstalled successfully.");
                }
                else
                {
                    Log(IsJapanese ? $"NPMパッケージのアンインストールでエラーが発生しました。（終了コード: {exitCode}）" : $"Error occurred during NPM package uninstallation. (Exit code: {exitCode})");
                }
            }
            
            await RemoveCommandFilesAsync(GEMINI_COMMAND_NAME);
            await RemoveConfigFolderAsync(GEMINI_CONFIG_FOLDER);
        }

        private async Task UninstallNodeAsync()
        {
            Log(IsJapanese ? "\nNode.jsの存在確認中..." : "\nChecking Node.js installation...");
            var isInstalled = await CheckNodeInstalledAsync();
            
            if (!isInstalled)
            {
                Log(IsJapanese ? "Node.jsはインストールされていません。" : "Node.js is not installed.");
                return;
            }
            
            Log(IsJapanese ? "Node.jsをアンインストール中..." : "Uninstalling Node.js...");
            Log(IsJapanese ? "この処理には管理者権限が必要で、確認プロンプトが表示される場合があります。" : "This may require administrator privileges and a confirmation prompt.");
            
            var exitCode = await RunProcessAsync("winget", $"uninstall --id {NODE_WINGET_ID} --accept-source-agreements --silent", requiresElevation: true);
            
            if (exitCode == 0)
            {
                Log(IsJapanese ? "Node.jsのアンインストールが完了しました。" : "Node.js uninstalled successfully.");
            }
            else
            {
                Log(IsJapanese ? $"Node.jsのアンインストールでエラーが発生しました。（終了コード: {exitCode}）" : $"Error occurred during Node.js uninstallation. (Exit code: {exitCode})");
            }
        }

        private async Task<int> RunProcessAsync(string fileName, string arguments, bool requiresElevation = false)
        {
            using var process = new Process
            {
                StartInfo = CreateProcessStartInfo(fileName, arguments, requiresElevation),
                EnableRaisingEvents = true
            };

            if (!requiresElevation)
            {
                process.OutputDataReceived += (_, e) => { if (e.Data != null) Log(e.Data); };
                process.ErrorDataReceived += (_, e) => { if (e.Data != null) Log((IsJapanese ? "エラー: " : "ERROR: ") + e.Data); };
            }

            try
            {
                process.Start();
                
                if (!requiresElevation)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                else
                {
                    Log(IsJapanese ? "管理者権限でプロセスを実行中..." : "Running process with administrator privileges...");
                }
                
                await process.WaitForExitAsync();
                return process.ExitCode;
            }
            catch (Exception ex)
            {
                Log(IsJapanese ? $"プロセス実行エラー: {ex.Message}" : $"Process execution error: {ex.Message}");
                return -1;
            }
        }

        private static ProcessStartInfo CreateProcessStartInfo(string fileName, string arguments, bool requiresElevation = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = !requiresElevation,
                RedirectStandardError = !requiresElevation,
                UseShellExecute = requiresElevation,
                CreateNoWindow = !requiresElevation,
            };

            if (!requiresElevation)
            {
                startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                startInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                EnhanceProcessEnvironment(startInfo);
            }
            else
            {
                startInfo.Verb = "runas";
            }
            
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
        
        private async Task<bool> CheckPackageInstalledAsync(string packageName)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = CreateProcessStartInfo("npm.cmd", "list -g --depth=0"),
                    EnableRaisingEvents = true
                };

                var output = new System.Text.StringBuilder();
                process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                await process.WaitForExitAsync();

                return output.ToString().Contains(packageName);
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<bool> CheckNodeInstalledAsync()
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = CreateProcessStartInfo("node", "--version"),
                    EnableRaisingEvents = true
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task RemoveCommandFilesAsync(string commandName)
        {
            Log(IsJapanese ? $"{commandName}コマンドファイルを直接削除中..." : $"Removing {commandName} command files directly...");
            
            var appDataPath = Environment.GetEnvironmentVariable("APPDATA");
            if (appDataPath != null)
            {
                var npmPath = Path.Combine(appDataPath, "npm");
                var commandPaths = new[]
                {
                    Path.Combine(npmPath, commandName),
                    Path.Combine(npmPath, $"{commandName}.cmd"),
                    Path.Combine(npmPath, $"{commandName}.ps1")
                };

                foreach (var filePath in commandPaths)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            Log(IsJapanese ? $"削除成功: {filePath}" : $"Deleted: {filePath}");
                        }
                        else
                        {
                            Log(IsJapanese ? $"ファイルが存在しません: {filePath}" : $"File not found: {filePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(IsJapanese ? $"ファイル削除エラー {filePath}: {ex.Message}" : $"File deletion error {filePath}: {ex.Message}");
                    }
                }
            }
            
            var isStillAvailable = await CheckCommandAvailableAsync(commandName);
            if (isStillAvailable)
            {
                Log(IsJapanese ? $"警告: {commandName}コマンドがまだ実行可能です。手動で確認してください。" : $"Warning: {commandName} command is still available. Please check manually.");
            }
            else
            {
                Log(IsJapanese ? $"{commandName}コマンドの削除が完了しました。" : $"{commandName} command has been successfully removed.");
            }
        }
        
        private async Task<bool> CheckCommandAvailableAsync(string commandName)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = CreateProcessStartInfo("cmd", $"/c where {commandName}"),
                    EnableRaisingEvents = true
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task RemoveConfigFolderAsync(string configFolderName)
        {
            await Task.Run(() =>
            {
                Log(IsJapanese ? $"{configFolderName}設定フォルダを削除中..." : $"Removing {configFolderName} configuration folder...");
                
                var homePath = Environment.GetEnvironmentVariable("HOMEPATH");
                var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
                
                string? configPath = null;
                if (!string.IsNullOrEmpty(userProfile))
                {
                    configPath = Path.Combine(userProfile, configFolderName);
                }
                else if (!string.IsNullOrEmpty(homePath))
                {
                    var systemDrive = Environment.GetEnvironmentVariable("HOMEDRIVE") ?? "C:";
                    configPath = Path.Combine(systemDrive, homePath, configFolderName);
                }
                
                if (configPath != null && Directory.Exists(configPath))
                {
                    try
                    {
                        Directory.Delete(configPath, true);
                        Log(IsJapanese ? $"設定フォルダを削除しました: {configPath}" : $"Configuration folder deleted: {configPath}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log(IsJapanese ? $"アクセスが拒否されました: {configPath} - {ex.Message}" : $"Access denied: {configPath} - {ex.Message}");
                        Log(IsJapanese ? "使用中のファイルがある可能性があります。手動で削除してください。" : "There might be files in use. Please delete manually.");
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Log(IsJapanese ? $"フォルダが見つかりません: {configPath}" : $"Folder not found: {configPath}");
                    }
                    catch (Exception ex)
                    {
                        Log(IsJapanese ? $"設定フォルダ削除エラー {configPath}: {ex.Message}" : $"Configuration folder deletion error {configPath}: {ex.Message}");
                        Log(IsJapanese ? "手動で削除してください。" : "Please delete manually.");
                    }
                }
                else if (configPath != null)
                {
                    Log(IsJapanese ? $"設定フォルダが存在しません: {configPath}" : $"Configuration folder does not exist: {configPath}");
                }
                else
                {
                    Log(IsJapanese ? "ホームパスを取得できませんでした。" : "Could not determine home path.");
                }
            });
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
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.Run(new UninstallerForm());
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