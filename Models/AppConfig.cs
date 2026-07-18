using System;

namespace WindowsWorkspaceManager.Models
{
    /// <summary>
    /// 機能概要
    /// 各画面の設定値保存用テーブルのデータモデル
    /// 来歴
    /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 機能概要
        /// 設定が紐づく画面名
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string ScreenName { get; set; } = string.Empty;

        /// <summary>
        /// 機能概要
        /// 設定を識別するキー名
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string SettingKey { get; set; } = string.Empty;

        /// <summary>
        /// 機能概要
        /// 保存する設定値（文字列）
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string SettingValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// アプリケーション設定のデフォルト値を一元管理するクラス
    /// </summary>
    public static class AppConfigDefaults
    {
        /// <summary>
        /// 機能概要
        /// 展開先フォルダパスのデフォルト値（マイドキュメント）
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public static string DefaultTargetFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// 機能概要
        /// ワークスペース名のデフォルト値（空文字列）
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public static string DefaultWorkspaceName => string.Empty;

        /// <summary>
        /// 機能概要
        /// 日付(yyyymmdd)チェックボックスのデフォルト値
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public static bool DefaultIsDateChecked => false;

        /// <summary>
        /// 機能概要
        /// 時間(hhmmss)チェックボックスのデフォルト値
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public static bool DefaultIsTimeChecked => false;

        /// <summary>
        /// 機能概要
        /// 検索キーワードのデフォルト値（空文字列）
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public static string DefaultSearchKeyword => string.Empty;
    }
}
