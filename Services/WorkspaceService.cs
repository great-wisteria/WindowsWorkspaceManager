using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace WindowsWorkspaceManager.Services
{
    /// <summary>
    /// 機能概要
    /// ZIP解凍およびファイル操作を担当するサービス
    /// 来歴
    /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
    /// </summary>
    public class WorkspaceService
    {
        /// <summary>
        /// 機能概要
        /// 指定されたZIPテンプレートを展開先ルートフォルダに作成する
        /// 来歴
        /// - [VR001ID001-01] 作業フォルダ（テンプレート）の管理および展開機能
        /// </summary>
        /// <param name="zipPath">展開するZIPファイルのパス</param>
        /// <param name="destinationRoot">展開先のルートフォルダパス</param>
        /// <param name="workspaceName">作成するワークスペース名（フォルダ名）</param>
        /// <param name="isDateChecked">日付（yyyymmdd）をプレフィックスに付与するか</param>
        /// <param name="isTimeChecked">時間（hhmmss）をプレフィックスに付与するか</param>
        /// <returns>作成された最終的な展開先フォルダの絶対パス</returns>
        public string ExtractTemplate(string zipPath, string destinationRoot, string workspaceName, bool isDateChecked, bool isTimeChecked)
        {
            // エラーチェック 1: 有効なフォルダか
            if (string.IsNullOrWhiteSpace(destinationRoot) || !Directory.Exists(destinationRoot))
            {
                throw new Exception("有効なフォルダが指定されていません");
            }

            // エラーチェック 2: ZIPファイルが存在するか
            if (!File.Exists(zipPath))
            {
                throw new Exception("登録済みのテンプレートファイルが移動または削除されています");
            }

            // プレフィックスの作成（yyyymmdd_hhmmss_ の順序）
            string prefix = "";
            DateTime now = DateTime.Now;
            if (isDateChecked) prefix += now.ToString("yyyyMMdd") + "_";
            if (isTimeChecked) prefix += now.ToString("HHmmss") + "_";

            string finalFolderName = prefix + workspaceName;
            string finalDestinationPath = Path.Combine(destinationRoot, finalFolderName);

            // パス長オーバーの簡易チェック
            if (finalDestinationPath.Length >= 260)
            {
                throw new Exception("フォルダーパスが長すぎるため作業フォルダの作成に失敗しました");
            }

            // 出力先が既に存在する場合のチェック
            if (Directory.Exists(finalDestinationPath))
            {
                if (Directory.EnumerateFileSystemEntries(finalDestinationPath).Any())
                {
                    // 中身が空でなければエラー
                    throw new Exception("作成先のフォルダがすでに存在しています");
                }
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(finalDestinationPath);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new Exception("書き込み権限がありません");
                }
                catch (IOException ex) when ((ex.HResult & 0x0000FFFF) == 112) // ERROR_DISK_FULL
                {
                    throw new Exception("ディスクの空き容量が足りません");
                }
                catch (Exception)
                {
                    throw new Exception("ファイルアクセスエラー");
                }
            }

            // ZIPの展開
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    // トップレベルフォルダの除去判定
                    // zipと同じ名前（拡張子なし）のフォルダが一番上にある場合はそれを取り除く
                    string zipFileNameWithoutExt = Path.GetFileNameWithoutExtension(zipPath);
                    string targetTopLevelFolder = zipFileNameWithoutExt + "/";

                    bool hasSingleTopLevelFolder = archive.Entries.All(e => e.FullName.StartsWith(targetTopLevelFolder, StringComparison.OrdinalIgnoreCase));

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        // 抽出先の相対パスを決定
                        string relativePath = entry.FullName;
                        if (hasSingleTopLevelFolder)
                        {
                            relativePath = relativePath.Substring(targetTopLevelFolder.Length);
                        }

                        if (string.IsNullOrEmpty(relativePath)) continue; // フォルダ自体のエントリ

                        string destinationPath = Path.GetFullPath(Path.Combine(finalDestinationPath, relativePath));

                        // 展開先の検証（ディレクトリトラバーサル防止）
                        if (!destinationPath.StartsWith(finalDestinationPath, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                        {
                            // フォルダ
                            Directory.CreateDirectory(destinationPath);
                        }
                        else
                        {
                            // ファイル
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                            entry.ExtractToFile(destinationPath, overwrite: true);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 細かいエラーはひとまとめにしてしまう
                throw new Exception("ファイルアクセスエラー");
            }

            return finalDestinationPath;
        }
    }
}
