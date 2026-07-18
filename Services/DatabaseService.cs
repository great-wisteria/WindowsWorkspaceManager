using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using WindowsWorkspaceManager.Models;

namespace WindowsWorkspaceManager.Services
{
    /// <summary>
    /// 機能概要
    /// SQLiteデータベースへのアクセスと管理を担当するサービス
    /// 来歴
    /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
    /// </summary>
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        /// <summary>
        /// 機能概要
        /// コンストラクタ。DBの保存先パスと接続文字列を初期化する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public DatabaseService()
        {
            // ポータブル運用（ライト版）のため、実行ファイルと同階層に作成
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _dbPath = Path.Combine(baseDir, "TemplateManager.sqlite");
            _connectionString = $"Data Source={_dbPath}";
        }

        /// <summary>
        /// 機能概要
        /// データベースの初期化。破損時などのフェイルセーフ処理も含む
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        public void InitializeDatabase()
        {
            try
            {
                EnsureTablesExist();
            }
            catch (SqliteException)
            {
                // DBファイルが破壊されているとみなして再作成
                RecreateDatabase();
            }
        }

        private void RecreateDatabase()
        {
            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
                EnsureTablesExist();
            }
            catch (Exception ex)
            {
                // ここでのエラーは重大なアクセス権エラー等
                System.Diagnostics.Debug.WriteLine($"DB再作成に失敗しました: {ex.Message}");
                throw;
            }
        }

        private void EnsureTablesExist()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS WorkspaceTemplates (
                        TemplateName TEXT PRIMARY KEY,
                        TemplatePath TEXT NOT NULL,
                        LastCreatedDate TEXT
                    );
                    
                    CREATE TABLE IF NOT EXISTS AppConfig (
                        ScreenName TEXT NOT NULL,
                        SettingKey TEXT NOT NULL,
                        SettingValue TEXT,
                        PRIMARY KEY (ScreenName, SettingKey)
                    );
                ";
                command.ExecuteNonQuery();
            }
        }

        #region WorkspaceTemplates CRUD
        /// <summary>
        /// 機能概要
        /// 全てのテンプレート情報を取得する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <returns>登録されている WorkspaceTemplate のリスト</returns>
        public List<WorkspaceTemplate> GetAllTemplates()
        {
            var list = new List<WorkspaceTemplate>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT TemplateName, TemplatePath, LastCreatedDate FROM WorkspaceTemplates";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new WorkspaceTemplate
                        {
                            TemplateName = reader.GetString(0),
                            TemplatePath = reader.GetString(1),
                            LastCreatedDate = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 機能概要
        /// テンプレート情報を追加する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="template">追加するテンプレート情報</param>
        public void AddTemplate(WorkspaceTemplate template)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO WorkspaceTemplates (TemplateName, TemplatePath, LastCreatedDate)
                    VALUES ($name, $path, $date)";
                command.Parameters.AddWithValue("$name", template.TemplateName);
                command.Parameters.AddWithValue("$path", template.TemplatePath);
                command.Parameters.AddWithValue("$date", template.LastCreatedDate ?? (object)DBNull.Value);
                
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 機能概要
        /// 複数のテンプレート情報を削除する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="templateNames">削除対象となるテンプレート名のリスト</param>
        public void RemoveTemplates(IEnumerable<string> templateNames)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM WorkspaceTemplates WHERE TemplateName = $name";
                    var param = command.Parameters.Add("$name", SqliteType.Text);

                    foreach (var name in templateNames)
                    {
                        param.Value = name;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// 機能概要
        /// 指定したテンプレートの最終作成日を更新する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="templateName">対象のテンプレート名</param>
        /// <param name="date">設定する日付文字列（yyyy/mm/ddなど）</param>
        public void UpdateLastCreatedDate(string templateName, string date)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE WorkspaceTemplates 
                    SET LastCreatedDate = $date 
                    WHERE TemplateName = $name";
                command.Parameters.AddWithValue("$date", date);
                command.Parameters.AddWithValue("$name", templateName);
                
                command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 機能概要
        /// 指定した名前またはパスのテンプレートがすでに登録されているか確認する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="templateName">確認するテンプレート名</param>
        /// <param name="templatePath">確認するZIPパス</param>
        /// <returns>存在する場合は true, そうでない場合は false</returns>
        public bool IsTemplateExists(string templateName, string templatePath)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // 仕様：同名のテンプレート、または同一パスのZIPファイルがすでに登録されている場合
                command.CommandText = @"
                    SELECT COUNT(1) FROM WorkspaceTemplates 
                    WHERE TemplateName = $name OR TemplatePath = $path";
                command.Parameters.AddWithValue("$name", templateName);
                command.Parameters.AddWithValue("$path", templatePath);
                
                var result = (long)command.ExecuteScalar();
                return result > 0;
            }
        }

        #endregion

        #region AppConfig CRUD
        /// <summary>
        /// 機能概要
        /// 指定した画面とキーに対応する設定値を取得する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="screenName">対象の画面名</param>
        /// <param name="settingKey">取得したい設定のキー</param>
        /// <returns>設定値の文字列。存在しない場合は null</returns>
        public string? GetAppConfig(string screenName, string settingKey)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT SettingValue FROM AppConfig 
                    WHERE ScreenName = $screenName AND SettingKey = $settingKey";
                command.Parameters.AddWithValue("$screenName", screenName);
                command.Parameters.AddWithValue("$settingKey", settingKey);

                var result = command.ExecuteScalar();
                return result == DBNull.Value ? null : (string?)result;
            }
        }
        /// <summary>
        /// 機能概要
        /// 指定した画面とキーに設定値を保存（追加または更新）する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="screenName">対象の画面名</param>
        /// <param name="settingKey">保存する設定のキー</param>
        /// <param name="settingValue">保存する設定値</param>
        public void SetAppConfig(string screenName, string settingKey, string settingValue)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                // UPSERT (INSERT OR REPLACE)
                command.CommandText = @"
                    INSERT INTO AppConfig (ScreenName, SettingKey, SettingValue)
                    VALUES ($screenName, $settingKey, $settingValue)
                    ON CONFLICT(ScreenName, SettingKey) DO UPDATE SET
                        SettingValue = excluded.SettingValue;";
                command.Parameters.AddWithValue("$screenName", screenName);
                command.Parameters.AddWithValue("$settingKey", settingKey);
                command.Parameters.AddWithValue("$settingValue", settingValue ?? (object)DBNull.Value);
                
                command.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
