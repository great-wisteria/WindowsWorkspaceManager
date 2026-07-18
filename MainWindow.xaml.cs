using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowsWorkspaceManager
{
    /// <summary>
    /// 機能概要
    /// メインウィンドウのコードビハインド
    /// 来歴
    /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModels.MainViewModel _viewModel;

        /// <summary>
        /// 機能概要
        /// コンストラクタ。ViewModelの初期化とバインディングを行う。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new ViewModels.MainViewModel();
            this.DataContext = _viewModel;
            this.Closing += MainWindow_Closing;
        }

        /// <summary>
        /// 機能概要
        /// ウィンドウを閉じる際のイベント。設定情報をデータベースに保存する。
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="sender">イベント送信元</param>
        /// <param name="e">イベント引数</param>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.SaveConfig();
        }
    }
}