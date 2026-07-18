# データベース仕様

## 1. 概要 (Overview)

### 1.1 背景・課題
テンプレート作成ツールにおいて、ユーザーが登録した作業フォルダ（テンプレート）の一覧情報や、アプリケーションの画面設定値（展開先パス、チェックボックスの状態など）を永続化し、次回起動時にも復元できる仕組みが必要である。

### 1.2 目的・解決策
データの管理・保存に SQLite データベースを採用する。
これにより、テンプレート情報と画面の各種設定を構造化されたデータとして同一のファイルで管理でき、将来的な項目の増減や検索にも柔軟に対応可能となる。

## 2. 要求項目・制約事項 (Requirements & Constraints)

| 要求ID                          | 要求項目                                           |
| ------------------------------- | -------------------------------------------------- |
| [VR001ID001-01](#vr001id001-01) | `作業フォルダ（テンプレート）の管理および展開機能` |

### 2.1 VR001ID001-01
アプリケーションのテンプレート情報および画面設定値を SQLite データベースにて永続化し、次回の起動時に状態を復元する仕組みを提供する。

## 3. データベース構成 (Database Architecture)

データの管理には 1つの SQLite データベースファイルを使用する。
- **データベースファイル名の例**: `TemplateManager.sqlite`
- **テーブル構成**:
  - `WorkspaceTemplates`: テンプレート一覧の管理用テーブル
  - `AppConfig`: 各画面の設定値保存用テーブル

役割の異なるデータを明確に分離するため、上記2つのテーブルで構成する。

### 3.1 フェイルセーフ仕様 (Failsafe)
- データベースファイルが破壊されている、または読み書きに失敗した場合は、既存のファイルを破棄して SQLite データベースを再作成する。
- データベース初期化時（AppConfigの読み込みエラー時など）、画面設定値は以下のデフォルト値を使用する。
  - 検索（Search）キーワード: 空欄
  - Date / Time チェックボックス: いずれも OFF
  - 展開先フォルダパス: Windows標準の「ドキュメント」フォルダ（`Environment.SpecialFolder.MyDocuments`）

## 4. テーブル定義 (Table Definitions)

### 4.1 WorkspaceTemplates テーブル
登録されたテンプレートのリストを保持する。ユーザーの登録（Add）および削除（Remove）操作によってレコードが増減する。

| カラム論理名 | カラム物理名 | データ型 | 説明 |
|---|---|---|---|
| **テンプレート名** | TemplateName | TEXT | ZIPのファイル名（拡張子なし）。メイン画面の「Templates」列に表示。 |
| **テンプレートのパス** | TemplatePath | TEXT | 選択されたZIPファイルの物理的なフルパス。 |
| **最終作成日** | LastCreatedDate | TEXT | Createボタンでフォルダを作成した最終日付（`yyyy/mm/dd`）。初期値は NULL（または空文字）。 |

### 4.2 AppConfig テーブル
各画面の前回入力値やUI設定（チェック状態など）を保持する。
今後の画面追加による項目名の重複（かぶり）を防ぎ、柔軟な拡張を可能にするため、「キー・バリュー形式」で設定値を保存する。

| カラム論理名 | カラム物理名 | データ型 | 説明 |
|---|---|---|---|
| **画面名** | ScreenName | TEXT | どの画面の設定かを示す識別子。本画面では `WorkspaceManager` とする。 |
| **設定項目名** | SettingKey | TEXT | 設定のキー名。 |
| **設定値** | SettingValue | TEXT | 実際の値。数値やON/OFF等の論理値もTEXTとして保存し、C#側で変換して使用する。 |

**【WorkspaceManager画面の設定レコード例（初期値）】**
| ScreenName | SettingKey | SettingValue | 備考 |
|---|---|---|---|
| WorkspaceManager | `SearchKeyword` | (空文字列) | 検索テキストボックスの値 |
| WorkspaceManager | `IsDateChecked` | `False` | DateチェックボックスのON/OFF状態 |
| WorkspaceManager | `IsTimeChecked` | `False` | TimeチェックボックスのON/OFF状態 |
| WorkspaceManager | `TargetFolderPath` | `C:\Users\Username\Documents` (※環境による) | 展開先フォルダパスの値 |

## 5. 影響範囲・変更対象ファイル (Affected Files)
- `WindowsWorkspaceManager/Services/DatabaseService.cs` (対応: VR001ID001-01)
- `WindowsWorkspaceManager/Models/AppConfig.cs` (対応: VR001ID001-01)
- `WindowsWorkspaceManager/Models/WorkspaceTemplate.cs` (対応: VR001ID001-01)
