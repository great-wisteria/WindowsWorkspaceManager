using WindowsWorkspaceManager.Models;

namespace WindowsWorkspaceManager.ViewModels
{
    /// <summary>
    /// 機能概要
    /// DataGridに表示するためのテンプレート項目のViewModel（No列を含む）
    /// 来歴
    /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
    /// </summary>
    public class TemplateItemViewModel
    {
        public int No { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string TemplatePath { get; set; } = string.Empty;
        public string? LastCreatedDate { get; set; }

        /// <summary>
        /// 機能概要
        /// コンストラクタ
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="no">表示用の連番（ソート時には不変）</param>
        /// <param name="model">データベースから取得したテンプレートモデル</param>
        public TemplateItemViewModel(int no, WorkspaceTemplate model)
        {
            No = no;
            TemplateName = model.TemplateName;
            TemplatePath = model.TemplatePath;
            LastCreatedDate = model.LastCreatedDate;
        }
    }
}
