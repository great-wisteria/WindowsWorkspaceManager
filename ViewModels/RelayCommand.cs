using System;
using System.Windows.Input;

namespace WindowsWorkspaceManager.ViewModels
{
    /// <summary>
    /// 機能概要
    /// ICommandの標準的な簡易実装クラス。ボタン等のコマンドバインディングに使用する。
    /// 来歴
    /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// 機能概要
        /// コンストラクタ
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="execute">実行するアクション</param>
        /// <param name="canExecute">実行可能かどうかを判定する関数（省略可）</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 機能概要
        /// コマンドが現在の状態で実行可能かどうかを判定する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="parameter">コマンドパラメータ</param>
        /// <returns>実行可能な場合は true</returns>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// 機能概要
        /// コマンドを実行する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="parameter">コマンドパラメータ</param>
        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>
        /// 機能概要
        /// 実行可能状態が変化したときに発生するイベント
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
