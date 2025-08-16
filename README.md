# AI CLI Launcher & Uninstaller

A comprehensive GUI application suite for easy AI CLI tools management on Windows.

Windows向けの AI CLI ツールを簡単に利用するための包括的な GUI アプリケーションセットです。

## 🌟 Features

- **One-Click Environment Setup**: Automatic installation of Node.js and AI CLI tools
- **Multi-Language Support**: Japanese for Japanese locale, English for others
- **Selective Uninstallation**: Remove only necessary components
- **Lightweight**: Ultra-compact executables (~180KB each)
- **Safe Design**: Confirmation dialogs and real-time logs for transparency

## 📦 Included Applications

| Tool | Launcher | Uninstaller | Purpose |
|------|----------|-------------|---------|
| **Google Gemini** | GeminiLauncher.exe | GeminiUninstaller.exe | Google's AI CLI tool |
| **Anthropic Claude Code** | ClaudeCodeLauncher.exe | ClaudeCodeUninstaller.exe | Anthropic's AI coding assistant |
| **OpenAI Codex** | CodexLauncher.exe | CodexUninstaller.exe | OpenAI's AI coding tool |

## 🌟 特徴

- **ワンクリック環境構築**: Node.js と AI CLI ツールを自動インストール
- **多言語対応**: 日本語環境では日本語、それ以外では英語で表示
- **選択制アンインストール**: 必要なコンポーネントのみを削除可能
- **軽量**: 超コンパクトな実行ファイル（各約180KB）
- **安全設計**: 確認ダイアログとリアルタイムログで透明性を確保

## 📦 含まれるアプリケーション

| ツール | ランチャー | アンインストーラー | 用途 |
|--------|------------|-------------------|------|
| **Google Gemini** | GeminiLauncher.exe | GeminiUninstaller.exe | Google の AI CLI ツール |
| **Anthropic Claude Code** | ClaudeCodeLauncher.exe | ClaudeCodeUninstaller.exe | Anthropic の AI コーディングアシスタント |
| **OpenAI Codex** | CodexLauncher.exe | CodexUninstaller.exe | OpenAI の AI コーディングツール |

## 🚀 Quick Start

### Using the Launchers

1. **Launch**: Double-click any launcher executable (e.g., `GeminiLauncher.exe`)
2. **Set Work Directory**: Use Browse button to select folder (default: Desktop)
3. **Start**: Click the respective "Start" button

#### First-time Execution Auto-Process
- ✅ Node.js verification and auto-installation (using winget)
- ✅ AI CLI tool verification and auto-installation (using npm)
- ✅ Automatic restart after 3 seconds to update environment variables
- ✅ Launch AI CLI in specified work folder

### Using the Uninstallers

1. **Launch**: Double-click any uninstaller executable
2. **Select Options**:
   - 🔸 **Uninstall AI CLI only** (recommended)
   - 🔸 **Uninstall AI CLI and Node.js**
3. **Execute**: Click "Execute Uninstall" button
4. **Confirm**: Select "Yes" in confirmation dialog

## 🚀 クイックスタート

### ランチャーの使用方法

1. **起動**: 任意のランチャー実行ファイル（例：`GeminiLauncher.exe`）をダブルクリック
2. **作業フォルダ設定**: Browseボタンでフォルダを選択（デフォルト: デスクトップ）
3. **起動**: 対応する「Start」ボタンをクリック

#### 初回実行時の自動処理
- ✅ Node.js の確認・自動インストール（winget使用）
- ✅ AI CLI ツールの確認・自動インストール（npm使用）
- ✅ 環境変数更新のため3秒後に自動再起動
- ✅ 指定した作業フォルダで AI CLI 起動

### アンインストーラーの使用方法

1. **起動**: 任意のアンインストーラー実行ファイルをダブルクリック
2. **オプション選択**:
   - 🔸 **AI CLI のみアンインストール** (推奨)
   - 🔸 **AI CLI と Node.js をアンインストール**
3. **実行**: `アンインストール実行` ボタンをクリック
4. **確認**: 確認ダイアログで「はい」を選択

## 💻 System Requirements

### Operating Environment
- **OS**: Windows 10 / Windows 11
- **Architecture**: x64
- **Runtime**: .NET 8.0 Runtime required (framework-dependent)
- **Internet**: Required only for initial installation

### Required Permissions
- **Standard User Rights**: Basic operations
- **Administrator Rights**: When running winget (automatic elevation prompt as needed)

## 💻 システム要件

### 動作環境
- **OS**: Windows 10 / Windows 11
- **アーキテクチャ**: x64
- **ランタイム**: .NET 8.0 Runtime必要（フレームワーク依存）
- **インターネット**: 初回インストール時のみ必要

### 必要な権限
- **標準ユーザー権限**: 基本動作
- **管理者権限**: winget実行時（必要に応じて自動で昇格要求）

## 🛠️ Technical Specifications

### Development Technologies
- **.NET 8.0** - Latest .NET platform
- **Windows Forms** - Native Windows UI
- **C# 12** - Latest language features
- **Framework-Dependent Deployment** - Requires .NET 8.0 Runtime

### External Tools
- **winget** - Node.js automatic installation
- **npm** - AI CLI tools automatic installation

### Installation Targets
- **Node.js**: `OpenJS.NodeJS` (Latest LTS)
- **Google Gemini CLI**: `@google/gemini-cli` (Latest)
- **Anthropic Claude Code**: `@anthropic/claude-code` (Latest)
- **OpenAI Codex**: `@openai/codex` (Latest)

## 🛠️ 技術仕様

### 開発技術
- **.NET 8.0** - 最新の.NETプラットフォーム
- **Windows Forms** - ネイティブWindowsUI
- **C# 12** - 最新言語機能を活用
- **Framework-Dependent Deployment** - .NET 8.0 Runtime が必要

### 外部ツール
- **winget** - Node.js自動インストール
- **npm** - AI CLI ツール自動インストール

### インストール対象
- **Node.js**: `OpenJS.NodeJS` (最新LTS)
- **Google Gemini CLI**: `@google/gemini-cli` (最新版)
- **Anthropic Claude Code**: `@anthropic/claude-code` (最新版)
- **OpenAI Codex**: `@openai/codex` (最新版)

## 📁 Project Structure

```
📁 Project Root/
├── 📄 README.md                    # This file
├── 📁 GeminiLauncher/
│   ├── 💻 GeminiLauncher.cs        # Optimized source code
│   └── ⚙️ GeminiLauncher.csproj    # Project configuration
├── 📁 GeminiUninstaller/
│   ├── 💻 GeminiUninstaller.cs     # Optimized source code
│   └── ⚙️ GeminiUninstaller.csproj # Project configuration
├── 📁 ClaudeCodeLauncher/
│   ├── 💻 ClaudeCodeLauncher.cs    # Optimized source code
│   └── ⚙️ ClaudeCodeLauncher.csproj # Project configuration
├── 📁 ClaudeCodeUninstaller/
│   ├── 💻 ClaudeCodeUninstaller.cs # Optimized source code
│   └── ⚙️ ClaudeCodeUninstaller.csproj # Project configuration
├── 📁 CodexLauncher/
│   ├── 💻 CodexLauncher.cs         # Optimized source code
│   └── ⚙️ CodexLauncher.csproj     # Project configuration
└── 📁 CodexUninstaller/
    ├── 💻 CodexUninstaller.cs      # Optimized source code
    └── ⚙️ CodexUninstaller.csproj  # Project configuration
```

## 📁 プロジェクト構成

```
📁 プロジェクトルート/
├── 📄 README.md                    # このファイル
├── 📁 GeminiLauncher/
│   ├── 💻 GeminiLauncher.cs        # 最適化されたソースコード
│   └── ⚙️ GeminiLauncher.csproj    # プロジェクト設定
├── 📁 GeminiUninstaller/
│   ├── 💻 GeminiUninstaller.cs     # 最適化されたソースコード
│   └── ⚙️ GeminiUninstaller.csproj # プロジェクト設定
├── 📁 ClaudeCodeLauncher/
│   ├── 💻 ClaudeCodeLauncher.cs    # 最適化されたソースコード
│   └── ⚙️ ClaudeCodeLauncher.csproj # プロジェクト設定
├── 📁 ClaudeCodeUninstaller/
│   ├── 💻 ClaudeCodeUninstaller.cs # 最適化されたソースコード
│   └── ⚙️ ClaudeCodeUninstaller.csproj # プロジェクト設定
├── 📁 CodexLauncher/
│   ├── 💻 CodexLauncher.cs         # 最適化されたソースコード
│   └── ⚙️ CodexLauncher.csproj     # プロジェクト設定
└── 📁 CodexUninstaller/
    ├── 💻 CodexUninstaller.cs      # 最適化されたソースコード
    └── ⚙️ CodexUninstaller.csproj  # プロジェクト設定
```

## 🔧 Developer Information

### Build Requirements
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

### Build Commands

**Development Build:**
```bash
dotnet build "GeminiLauncher/GeminiLauncher.csproj"
dotnet build "GeminiUninstaller/GeminiUninstaller.csproj"
dotnet build "ClaudeCodeLauncher/ClaudeCodeLauncher.csproj"
dotnet build "ClaudeCodeUninstaller/ClaudeCodeUninstaller.csproj"
dotnet build "CodexLauncher/CodexLauncher.csproj"
dotnet build "CodexUninstaller/CodexUninstaller.csproj"
```

**Release Publication:**
```bash
dotnet publish GeminiLauncher -c Release
dotnet publish GeminiUninstaller -c Release
dotnet publish ClaudeCodeLauncher -c Release
dotnet publish ClaudeCodeUninstaller -c Release
dotnet publish CodexLauncher -c Release
dotnet publish CodexUninstaller -c Release
```

## 🔧 開発者向け情報

### ビルド要件
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

### ビルドコマンド

**開発用ビルド:**
```bash
dotnet build "GeminiLauncher/GeminiLauncher.csproj"
dotnet build "GeminiUninstaller/GeminiUninstaller.csproj"
dotnet build "ClaudeCodeLauncher/ClaudeCodeLauncher.csproj"
dotnet build "ClaudeCodeUninstaller/ClaudeCodeUninstaller.csproj"
dotnet build "CodexLauncher/CodexLauncher.csproj"
dotnet build "CodexUninstaller/CodexUninstaller.csproj"
```

**リリース用発行:**
```bash
dotnet publish GeminiLauncher -c Release
dotnet publish GeminiUninstaller -c Release
dotnet publish ClaudeCodeLauncher -c Release
dotnet publish ClaudeCodeUninstaller -c Release
dotnet publish CodexLauncher -c Release
dotnet publish CodexUninstaller -c Release
```

### コード品質
- ✅ **名前空間**: 適切な組織化
- ✅ **非同期処理**: Task-based Asynchronous Pattern
- ✅ **エラーハンドリング**: 包括的な例外処理
- ✅ **リソース管理**: using文による確実な解放
- ✅ **多言語対応**: システムロケールに基づく自動言語切り替え
- ✅ **ユーザビリティ**: 直感的UI・自動スクロール

## 🔒 セキュリティについて

### 安全性の確保
- ✅ **透明性**: ソースコード公開により動作内容を確認可能
- ✅ **最小権限**: 必要最小限の権限のみを要求
- ✅ **ローカル処理**: すべての処理はローカルマシン上で実行
- ✅ **データ保護**: 個人情報や設定情報の外部送信なし

### 署名について
- ⚠️ **未署名**: 個人開発のため、デジタル署名はありません
- 💡 **対策**: Windows Defenderで初回実行時に警告が表示される場合があります

## 🎯 使用上の注意

### 推奨事項
1. **初回実行**: インストール完了後は自動再起動を待つ
2. **権限**: winget実行時の管理者権限要求に応じる
3. **ネットワーク**: インストール時のインターネット接続を確保

### 制限事項
1. **プラットフォーム**: Windows専用（macOS/Linux非対応）
2. **アーキテクチャ**: x64のみ（x86/ARM未対応）
3. **言語**: 日本語・英語対応（その他の言語は英語で表示）

## 📞 サポート

### 問題解決の手順
1. **ログ確認**: アプリ内のログエリアでエラー詳細を確認
2. **手動実行**: 問題のあるコマンドを手動で実行
3. **クリーンインストール**: Uninstallerで削除後、再インストール

### フィードバック
技術的な問題や改善提案は開発者までお問い合わせください。

---

## 📄 ライセンス

このプロジェクトは個人利用・学習目的で作成されています。
商用利用や再配布については開発者にお問い合わせください。

---

**📅 最終更新**: 2025年8月17日  
**🔖 バージョン**: 軽量最適化版  
**📊 ファイルサイズ**: 各実行ファイル約180KB（99.8%削減達成）  
**👨‍💻 開発**: Claude Code supported
