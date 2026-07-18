using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using WindowsWorkspaceManager.Models;
using WindowsWorkspaceManager.Services;

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
        private List<WorkspaceTemplate> _allTemplates = new List<WorkspaceTemplate>();

        public MainViewModel()
        {
            _dbService = new DatabaseService();
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

        public ObservableCollection<TemplateItemViewModel> FilteredTemplates { get; }

        private TemplateItemViewModel? _selectedTemplate;
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
        public string TargetFolder
        {
            get => _targetFolder;
            set
            {
                string sanitized = RemoveInvalidChars(value);
                SetProperty(ref _targetFolder, sanitized);
            }
        }

        private string _workspaceName = string.Empty;
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
        public bool IsDateChecked
        {
            get => _isDateChecked;
            set => SetProperty(ref _isDateChecked, value);
        }

        private bool _isTimeChecked;
        public bool IsTimeChecked
        {
            get => _isTimeChecked;
            set => SetProperty(ref _isTimeChecked, value);
        }

        #endregion

        #region Commands

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand SelectFolderCommand { get; }

        private void ExecuteAdd(object? parameter)
        {
            // TODO: STEP 4 で実装
        }

        private bool CanExecuteRemove(object? parameter)
        {
            return SelectedTemplate != null;
        }

        private void ExecuteRemove(object? parameter)
        {
            // TODO: STEP 4 で実装
        }

        private bool CanExecuteCreate(object? parameter)
        {
            // WorkspaceNameが入力されており、かつ1つのテンプレートが選択されていること
            return !string.IsNullOrWhiteSpace(WorkspaceName) && SelectedTemplate != null;
        }

        private void ExecuteCreate(object? parameter)
        {
            // TODO: STEP 4 で実装
        }

        private void ExecuteSelectFolder(object? parameter)
        {
            // TODO: STEP 4 で実装
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
            TargetFolder = _dbService.GetAppConfig("MainWindow", "TargetFolder") ?? string.Empty;
            WorkspaceName = _dbService.GetAppConfig("MainWindow", "WorkspaceName") ?? string.Empty;
            IsDateChecked = bool.TryParse(_dbService.GetAppConfig("MainWindow", "IsDateChecked"), out bool d) && d;
            IsTimeChecked = bool.TryParse(_dbService.GetAppConfig("MainWindow", "IsTimeChecked"), out bool t) && t;
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

            int no = 1;
            foreach (var item in filtered)
            {
                FilteredTemplates.Add(new TemplateItemViewModel(no++, item));
            }
        }

        /// <summary>
        /// 機能概要
        /// 入力文字列からWindowsのパスとして使用できない禁則文字を削除して返す
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="input">対象の文字列</param>
        /// <returns>禁則文字が削除された文字列</returns>
        private string RemoveInvalidChars(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // 禁則文字: \ / : * ? " < > |
            string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(input, invalidRegStr, "");
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
