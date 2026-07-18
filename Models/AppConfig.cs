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
        public string ScreenName { get; set; } = string.Empty;
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
    }
}
