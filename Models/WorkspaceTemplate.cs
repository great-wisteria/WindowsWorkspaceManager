using System;

namespace WindowsWorkspaceManager.Models
{
    /// <summary>
    /// 機能概要
    /// 登録された作業フォルダの情報を保持するデータモデル
    /// 来歴
    /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
    /// </summary>
    public class WorkspaceTemplate
    {
        /// <summary>
        /// 機能概要
        /// 登録されているテンプレート名
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 機能概要
        /// 参照元となるZIPファイルのパス
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string TemplatePath { get; set; } = string.Empty;

        /// <summary>
        /// 機能概要
        /// 最後にこのテンプレートからワークスペースを作成した日時
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string? LastCreatedDate { get; set; }
    }
}
