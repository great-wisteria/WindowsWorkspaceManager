using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WindowsWorkspaceManager.Models;
using WindowsWorkspaceManager.Services;

using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace WindowsWorkspaceManager.ViewModels
{
    /// <summary>
    /// 機能概要
    /// メイン画面(MainWindow)のビューモデル。画面のロジックとバインディングを担当する。
    /// 来歴
    /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private readonly WorkspaceService _workspaceService;
        private List<WorkspaceTemplate> _allTemplates = new List<WorkspaceTemplate>();

        /// <summary>
        /// 機能概要
        /// コンストラクタ。サービスの初期化、コマンドの設定、データの読み込みを行う。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public MainViewModel()
        {
            _dbService = new DatabaseService();
            _workspaceService = new WorkspaceService();
            _dbService.InitializeDatabase();

            FilteredTemplates = new ObservableCollection<TemplateItemViewModel>();

            AddCommand = new RelayCommand(ExecuteAdd);
            RemoveCommand = new RelayCommand(ExecuteRemove, CanExecuteRemove);
            CreateCommand = new RelayCommand(ExecuteCreate, CanExecuteCreate);
            SelectFolderCommand = new RelayCommand(ExecuteSelectFolder);

            LoadConfig();
            LoadTemplates();
        }

        #region Properties

        private string _searchKeyword = string.Empty;
        /// <summary>
        /// 機能概要
        /// 検索キーワード
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    FilterTemplates();
                }
            }
        }

        /// <summary>
        /// 機能概要
        /// 画面に表示するテンプレートのリスト（検索による絞り込み結果）
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public ObservableCollection<TemplateItemViewModel> FilteredTemplates { get; }

        private TemplateItemViewModel? _selectedTemplate;
        /// <summary>
        /// 機能概要
        /// DataGrid上で選択されているテンプレート
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public TemplateItemViewModel? SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                if (SetProperty(ref _selectedTemplate, value))
                {
                    // 選択状態が変わるとCreate/Removeボタンの活性状態が変わる可能性があるため評価
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private string _targetFolder = string.Empty;
        /// <summary>
        /// 機能概要
        /// 展開先のルートフォルダパス
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string TargetFolder
        {
            get => _targetFolder;
            set
            {
                string sanitized = RemoveInvalidPathChars(value);
                SetProperty(ref _targetFolder, sanitized);
            }
        }

        private string _workspaceName = string.Empty;
        /// <summary>
        /// 機能概要
        /// 作成するワークスペース（フォルダ）名
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public string WorkspaceName
        {
            get => _workspaceName;
            set
            {
                string sanitized = RemoveInvalidChars(value);
                if (SetProperty(ref _workspaceName, sanitized))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private bool _isDateChecked;
        /// <summary>
        /// 機能概要
        /// 日付(yyyymmdd)をプレフィックスに付与するかのチェック状態
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public bool IsDateChecked
        {
            get => _isDateChecked;
            set => SetProperty(ref _isDateChecked, value);
        }

        private bool _isTimeChecked;
        /// <summary>
        /// 機能概要
        /// 時間(hhmmss)をプレフィックスに付与するかのチェック状態
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public bool IsTimeChecked
        {
            get => _isTimeChecked;
            set => SetProperty(ref _isTimeChecked, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// 機能概要
        /// 新規テンプレート追加コマンド
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// 機能概要
        /// 選択テンプレート削除コマンド
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public ICommand RemoveCommand { get; }

        /// <summary>
        /// 機能概要
        /// ワークスペース作成コマンド
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public ICommand CreateCommand { get; }

        /// <summary>
        /// 機能概要
        /// 展開先フォルダ選択コマンド
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public ICommand SelectFolderCommand { get; }

        /// <summary>
        /// 機能概要
        /// Addボタン押下時のコマンド実行処理。ZIPファイルを選択し、テンプレートとして登録する。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="parameter">コマンドパラメータ（未使用）</param>
        private void ExecuteAdd(object? parameter)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "ZIP Files (*.zip)|*.zip|All Files (*.*)|*.*",
                Title = "テンプレートZIPファイルを選択してください"
            };

            if (dialog.ShowDialog() == true)
            {
                string zipPath = dialog.FileName;
                string templateName = Path.GetFileNameWithoutExtension(zipPath);

                if (_dbService.IsTemplateExists(templateName, zipPath))
                {
                    MessageBox.Show(Properties.Resources.Msg_DuplicateTemplate, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _dbService.AddTemplate(new WorkspaceTemplate
                {
                    TemplateName = templateName,
                    TemplatePath = zipPath
                });

                LoadTemplates();
            }
        }

        /// <summary>
        /// 機能概要
        /// Removeボタンが実行可能かどうかを判定する。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="parameter">コマンドパラメータ</param>
        /// <returns>実行可能な場合は true</returns>
        private bool CanExecuteRemove(object? parameter)
        {
            // parameterにSelectedItemsが渡ってくる場合があるが、とりあえずSelectedTemplateがあればOKとする
            return SelectedTemplate != null;
        }

        /// <summary>
        /// 機能概要
        /// Removeボタン押下時のコマンド実行処理。選択されたテンプレートを削除する。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="parameter">DataGridのSelectedItemsリスト</param>
        private void ExecuteRemove(object? parameter)
        {
            var selectedItems = parameter as System.Collections.IList;
            if (selectedItems == null || selectedItems.Count == 0) return;

            var result = MessageBox.Show(Properties.Resources.Msg_ConfirmRemoveTemplate, Properties.Resources.Title_Confirm, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var templateNames = selectedItems.Cast<TemplateItemViewModel>().Select(x => x.TemplateName).ToList();
                _dbService.RemoveTemplates(templateNames);
                LoadTemplates();
            }
        }

        /// <summary>
        /// 機能概要
        /// Createボタンが実行可能かどうかを判定する。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="parameter">コマンドパラメータ</param>
        /// <returns>実行可能な場合は true</returns>
        private bool CanExecuteCreate(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(WorkspaceName) && SelectedTemplate != null;
        }

        /// <summary>
        /// 機能概要
        /// Createボタン押下時のコマンド実行処理。選択されたテンプレートを展開先に作成する。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="parameter">コマンドパラメータ（未使用）</param>
        private void ExecuteCreate(object? parameter)
        {
            if (SelectedTemplate == null) return;

            try
            {
                string createdFolderPath = _workspaceService.ExtractTemplate(
                    SelectedTemplate.TemplatePath,
                    TargetFolder,
                    WorkspaceName,
                    IsDateChecked,
                    IsTimeChecked
                );

                string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                _dbService.UpdateLastCreatedDate(SelectedTemplate.TemplateName, now);
                
                MessageBox.Show(Properties.Resources.Msg_WorkspaceCreated, Properties.Resources.Title_Complete, MessageBoxButton.OK, MessageBoxImage.Information);
                
                // エクスプローラで作成したフォルダを開く
                if (Directory.Exists(createdFolderPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", createdFolderPath);
                }

                LoadTemplates();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.Title_Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 機能概要
        /// Selectボタン押下時のコマンド実行処理。フォルダ選択ダイアログを表示して展開先ルートフォルダを指定する。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="parameter">コマンドパラメータ（未使用）</param>
        private void ExecuteSelectFolder(object? parameter)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "展開先のルートフォルダを選択してください";
                dialog.ShowNewFolderButton = true;
                
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TargetFolder = dialog.SelectedPath;
                }
            }
        }

        #endregion

        #region Logic

        /// <summary>
        /// 機能概要
        /// データベースから設定を読み込み、プロパティに反映する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public void LoadConfig()
        {
            TargetFolder = _dbService.GetAppConfig("MainWindow", "TargetFolder") ?? AppConfigDefaults.DefaultTargetFolder;
            if (string.IsNullOrWhiteSpace(TargetFolder))
            {
                TargetFolder = AppConfigDefaults.DefaultTargetFolder;
            }

            WorkspaceName = _dbService.GetAppConfig("MainWindow", "WorkspaceName") ?? AppConfigDefaults.DefaultWorkspaceName;
            
            string? dateStr = _dbService.GetAppConfig("MainWindow", "IsDateChecked");
            IsDateChecked = dateStr != null && bool.TryParse(dateStr, out bool d) ? d : AppConfigDefaults.DefaultIsDateChecked;

            string? timeStr = _dbService.GetAppConfig("MainWindow", "IsTimeChecked");
            IsTimeChecked = timeStr != null && bool.TryParse(timeStr, out bool t) ? t : AppConfigDefaults.DefaultIsTimeChecked;
        }

        /// <summary>
        /// 機能概要
        /// 現在のプロパティの値をデータベースに保存する（終了時などに呼び出し）
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public void SaveConfig()
        {
            _dbService.SetAppConfig("MainWindow", "TargetFolder", TargetFolder);
            _dbService.SetAppConfig("MainWindow", "WorkspaceName", WorkspaceName);
            _dbService.SetAppConfig("MainWindow", "IsDateChecked", IsDateChecked.ToString());
            _dbService.SetAppConfig("MainWindow", "IsTimeChecked", IsTimeChecked.ToString());
        }

        /// <summary>
        /// 機能概要
        /// DBからすべてのテンプレートを取得し、フィルタリングを適用して画面に表示する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public void LoadTemplates()
        {
            _allTemplates = _dbService.GetAllTemplates();
            FilterTemplates();
        }

        /// <summary>
        /// 機能概要
        /// 検索キーワードでテンプレートをフィルタリングし、No列を1から再採番してObservableCollectionに設定する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        private void FilterTemplates()
        {
            FilteredTemplates.Clear();

            var filtered = _allTemplates.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                filtered = filtered.Where(t => 
                    t.TemplateName.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase));
            }

            // 日付（降順）でソート。nullや空文字は一番下に回す
            var sorted = filtered.OrderByDescending(t => t.LastCreatedDate ?? string.Empty);

            int no = 1;
            foreach (var item in sorted)
            {
                FilteredTemplates.Add(new TemplateItemViewModel(no++, item));
            }

            // テンプレートが一つでも存在する場合は、一番上の項目を選択状態にする
            if (FilteredTemplates.Count > 0)
            {
                SelectedTemplate = FilteredTemplates[0];
            }
        }

        /// <summary>
        /// 機能概要
        /// ワークスペース名からファイル名として使用できない禁則文字を削除して返す
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="input">対象の文字列</param>
        /// <returns>禁則文字が削除された文字列</returns>
        private string RemoveInvalidChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // 禁則文字: \ / : * ? " < > |
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(input, invalidRegStr, "");
        }

        /// <summary>
        /// 機能概要
        /// パスとして使用できない禁則文字（制御文字など）を削除して返す。コロン(:)や円記号(\)は許容する。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="input">対象の文字列</param>
        /// <returns>禁則文字が削除された文字列</returns>
        private string RemoveInvalidPathChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()));
            string invalidRegStr = string.Format(@"([{0}]+)", invalidChars);
            return Regex.Replace(input, invalidRegStr, "");
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// プロパティ変更通知イベント
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 機能概要
        /// プロパティの値を設定し、変更があった場合に通知イベントを発行する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="storage">フィールドの参照</param>
        /// <param name="value">設定する値</param>
        /// <param name="propertyName">プロパティ名（自動取得）</param>
        /// <returns>値が変更された場合は true</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 機能概要
        /// プロパティ変更通知イベントを発行する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="propertyName">変更されたプロパティ名</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
