# AI実装計画書: VR001ID001-01 作業フォルダ（テンプレート）の管理および展開機能

## 1. 実装の目的
`メイン機能/README.md` および `データベース仕様/README.md` の仕様に基づき、ZIP化されたテンプレートの管理・展開を行うWindows WPFアプリケーション（C# .NET）を実装する。データの永続化には SQLite を使用する。

## 2. 影響範囲・対象ファイル
- `WindowsWorkspaceManager/MainWindow.xaml` (UI定義)
- `WindowsWorkspaceManager/MainWindow.xaml.cs` (UIビハインド)
- `WindowsWorkspaceManager/ViewModels/MainViewModel.cs` (ビューモデル、画面のロジックとバインディング)
- `WindowsWorkspaceManager/Models/WorkspaceTemplate.cs` (テンプレートモデル)
- `WindowsWorkspaceManager/Models/AppConfig.cs` (設定モデル)
- `WindowsWorkspaceManager/Services/DatabaseService.cs` (SQLiteデータベース接続・管理)
- `WindowsWorkspaceManager/Services/WorkspaceService.cs` (ZIP解凍およびファイル操作)

## 3. 実装ステップとチェックリスト

### STEP 1: データモデルとデータベースの準備
- [x] `WorkspaceTemplate.cs` を作成し、`TemplateName`, `TemplatePath`, `LastCreatedDate` を定義する。
- [x] `AppConfig.cs` を作成し、`ScreenName`, `SettingKey`, `SettingValue` を定義する。
- [x] `DatabaseService.cs` を実装する。
  - [x] NuGetで `Microsoft.Data.Sqlite` などを導入する。
  - [x] データベースファイルの作成と初期化処理を実装。
    - ※保存先パスは、実行ファイルと同一階層（`AppDomain.CurrentDomain.BaseDirectory` 等）の `TemplateManager.sqlite` とし、ポータブル運用（ライト版）を可能とする。
  - [x] `WorkspaceTemplates` テーブルと `AppConfig` テーブルの CREATE 処理を実装。
  - [x] データベース破損時のフェイルセーフ処理（DB削除と再生成、デフォルト値の適用）を実装。

### STEP 2: UIの構築 (MainWindow.xaml)
- [x] 既存の `MainWindow.xaml` をそのまま利用（配置変更・UI追加は行わない）。
  - ※ただし、ViewModelと連携するための `Binding` プロパティのみ追記を行う。

### STEP 3: ViewModel の実装 (MainViewModel.cs)
- [x] INotifyPropertyChanged を実装し、各UIコントロール用のプロパティをバインディングする。
- [x] **起動・終了処理**: 起動時にDBからAppConfig設定を読み込み、終了時に設定を保存する処理。
- [x] **検索機能**: Searchテキストボックスの変更検知でDataGridを部分一致フィルタリング。絞り込み・追加・削除時にNo列を1から再採番するロジック（ソート時は行わない）。
- [x] **入力制限処理**: ワークスペース名に対し禁則文字（`\ / : * ? " < > |`）を受け付けない処理。パス（TargetFolder）に対してはパス区切り文字（`\` や `:`）を許容するサニタイズ処理。
- [x] **ボタン活性/非活性制御**:
  - [x] `Create`ボタン: DataGridで1つだけ選択中、かつワークスペース名が入力されている時のみ活性化。
  - [x] `Remove`ボタン: DataGridで選択されているアイテムがある時のみ活性化。

### STEP 4: コマンドとサービスロジックの実装
- [x] **Add（追加）処理**:
  - [x] ZIPファイルを選択。
  - [x] 同名のテンプレート、または同一のZIPパスが既に登録されている場合はエラーダイアログを表示（メッセージ: 「同一のテンプレートフォルダ名が登録されています。名前を変更して登録するか登録済みのテンプレートフォルダを削除して再度登録してください」）。
  - [x] 問題なければDBに登録し、一覧を更新。
- [x] **Remove（削除）処理**:
  - [x] 複数選択対応。
  - [x] 削除前に確認ダイアログ（「登録を解除しますか？」）を表示。
  - [x] DBからのみ削除（実ファイルは削除しない）。
- [x] **Create（作成・展開）処理**:
  - [x] エラーチェック: ルートフォルダ空欄/無効、空き容量不足、パス長オーバー、書き込み権限等の確認。
  - [x] プレフィックスの結合（`yyyymmdd_hhmmss_` の順序を厳守）。
  - [x] 出力先パスが既に存在する場合、空フォルダなら続行、空でなければエラーダイアログ。
  - [x] `WorkspaceService` を呼び出し、ZIPを展開。
  - [x] ZIP展開時、トップレベルのフォルダ名がZIPファイル名と同じ場合は、そのフォルダを取り除いて中身だけを展開する。
  - [x] 展開完了後、DBのLastCreatedDateを現在日付に更新し、完了通知を表示。
  - [x] 成功時、展開されたフォルダをエクスプローラ（explorer.exe）で自動的に開く。

## 4. エラーメッセージ定義要件
実装コード内に以下のエラーメッセージ（またはそれに準ずる定数）を定義し、条件に応じて正確に表示すること。
- 「同一のテンプレートフォルダ名が登録されています。名前を変更して登録するか登録済みのテンプレートフォルダを削除して再度登録してください」
- 「ファイルアクセスエラー」
- 「有効なフォルダが指定されていません」
- 「作成先のフォルダがすでに存在しています」
- 「登録済みのテンプレートファイルが移動または削除されています」
- 「書き込み権限がありません」
- 「ディスクの空き容量が足りません」
- 「フォルダーパスが長すぎるため作業フォルダの作成に失敗しました」
- 「設定の保存に失敗しました」

## 5. 注意事項・AIエージェントへの指示
- 実装を開始する前に、必ず本計画書と `メイン機能/README.md`, `データベース仕様/README.md` を読み込み、矛盾のないようにコードを生成すること。
- コードの追加や修正を行う際は、対象ファイルの構造や依存関係を十分に確認しながら実施すること。
