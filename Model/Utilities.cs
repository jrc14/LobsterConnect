using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.Model
{
    static public class Utilities
    {
        public static string InstallationId
        {
            get
            {
                if (_installationId != null)
                    return _installationId;
                else
                {
                    if (Microsoft.Maui.Storage.Preferences.ContainsKey("InstallationId"))
                    {
                        _installationId = Microsoft.Maui.Storage.Preferences.Get("InstallationId", "");
                    }

                    if(string.IsNullOrEmpty(_installationId))
                    {
                        int r = System.Random.Shared.Next();
                        _installationId = String.Format("{0:X8}", r);

                        Microsoft.Maui.Storage.Preferences.Set("InstallationId", _installationId);
                    }

                    return _installationId;
                }
            }
        }
        private static string _installationId = null;

        public static string PasswordHash(string p)
        {
            string h = "";

            for(int i=0; i<p.Length;i++)
            {
                if (i % 2 == 0)
                    h = h + p[i];
            }

            string hashed= h.GetHashCode().ToString("X8");

            return hashed;
        }

        public static bool FileExists(string path)
        {

            try
            {
                return File.Exists(path);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR,"Utilities.FileExists", ex);
                return false;
            }
        }

        public static bool FileExistsWithNonZeroSize(string path)
        {
            if (!File.Exists(path))
                return false;
            else
            {
                try
                {
                    using (Stream fs = OpenFileForRead(path))
                    {
                        fs.Seek(0, SeekOrigin.End);

                        if (fs.Position == 0)
                            return false;
                    }

                    return true;
                }
                catch (FileNotFoundException )
                {
                    return false;

                }
                catch (Exception ex)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Utilities.FileExistsWithNonZeroSize: ", ex);
                    return false;
                }
            }

        }

        public static bool FileDeleteIfExists(string path)
        {

            try
            {
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception )
                    {
                        string dpath = path + "-" + Guid.NewGuid().ToString("N") + ".deleted";
                        try
                        {
                            if (File.Exists(dpath))
                            {
                                try
                                {
                                    File.Delete(dpath);
                                }
                                catch (Exception )
                                {

                                }
                            }
                            if (File.Exists(dpath))
                            {
                                File.Copy(path, dpath, true);
                                File.Delete(path);
                            }
                            else
                                File.Move(path, dpath);
                        }
                        catch (Exception ex1)
                        {
                            Logger.LogMessage(Logger.Level.ERROR, "Utilities.FileDeleteIfExists",ex1," deleting: " + path + ": ");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Utilities.FileDeleteIfExists", ex);
                return false;
            }
        }

        public static Stream OpenFileForWrite(string filePath)
        {
            Stream fileStream = System.IO.File.Open(filePath, FileMode.Create, FileAccess.Write);

            return fileStream;
        }


        public static Stream OpenFileForRead(string filePath)
        {
            Stream fileStream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return fileStream;
        }

        public static bool FileMove(string pathFrom, string pathTo)
        {
            if (File.Exists(pathTo))
            {
                File.Copy(pathFrom, pathTo, true);
                File.Delete(pathFrom);
            }
            else
                File.Move(pathFrom, pathTo);

            return true;
        }

        public static bool FileRename(string existingFilePath, string newName)
        {
            string folderPath = System.IO.Path.GetDirectoryName(existingFilePath);
            string pathTo = System.IO.Path.Combine(folderPath, newName);

            if (File.Exists(pathTo))
            {
                File.Copy(existingFilePath, pathTo, true);
                File.Delete(existingFilePath);
            }
            else
                File.Move(existingFilePath, pathTo);

            return true;
        }

        public static bool FileCopy(string pathFrom, string pathTo)
        {
            File.Copy(pathFrom, pathTo, true);

            return true;
        }

        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }


        public static bool DirectoryRename(string folderPath, string newName)
        {
            string pathTo = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(folderPath), newName);
            Directory.Move(folderPath, pathTo);

            return true;
        }

        public static List<string> DirectoryListFileNames(string folderPath)
        {
            try
            {
                List<string> retVal = new List<string>();

                foreach (string s in Directory.EnumerateFiles(folderPath))
                {
                    retVal.Add(System.IO.Path.GetFileName(s));
                }

                return retVal;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Utilities.DirectoryListFileNames", ex);

                return new List<string>();
            }
        }

        public static bool DirectoryCreate(string path)
        {
            Directory.CreateDirectory(path);

            return true;
        }



#pragma warning disable 1998 // I want to keep an async signature for these methods in case I change the implementation later
        public async static Task<bool> FileExistsAsync(string path)
        {
            return FileExists(path);
        }


        public async static Task<bool> FileExistsWithNonZeroSizeAsync(string path)
        {
            if (!File.Exists(path))
                return false;
            else
            {
                try
                {
                    using (Stream fs = OpenFileForRead(path))
                    {
                        fs.Seek(0, SeekOrigin.End);

                        if (fs.Position == 0)
                            return false;
                    }

                    return true;
                }
                catch (FileNotFoundException )
                {
                    return false;

                }
                catch (Exception ex)
                {
                    Logger.LogMessage(Logger.Level.ERROR, "Utilities.FileExistsWithNonZeroSizeAsync", ex);
                    return false;
                }
            }

        }

        public async static Task<bool> FileDeleteIfExistsAsync(string path)
        {

            return FileDeleteIfExists(path);

        }



        public async static Task<Stream> OpenFileForWriteAsync(string filePath)
        {
            return OpenFileForWrite(filePath);
        }

        public async static Task<Stream> OpenFileForReadAsync(string filePath)
        {
            return OpenFileForRead(filePath);
        }

        public static async Task<bool> FileMoveAsync(string pathFrom, string pathTo)
        {
            return FileMove(pathFrom, pathTo);

        }

        public static async Task<bool> FileRenameAsync(string existingFilePath, string newName)
        {
            string folderPath = System.IO.Path.GetDirectoryName(existingFilePath);
            string pathTo = System.IO.Path.Combine(folderPath, newName);

            if (File.Exists(pathTo))
            {
                File.Copy(existingFilePath, pathTo, true);
                File.Delete(existingFilePath);
            }
            else
                File.Move(existingFilePath, pathTo);

            return true;
        }

        public static async Task<bool> FileCopyAsync(string pathFrom, string pathTo)
        {
            return FileCopy(pathFrom, pathTo);
        }

        public async static Task<bool> DirectoryExistsAsync(string folderPath)
        {
            return DirectoryExists(folderPath);
        }


        public async static Task<List<string>> DirectoryListFileNamesAsync(string folderPath)
        {
            try
            {
                List<string> retVal = new List<string>();

                foreach (string s in Directory.EnumerateFiles(folderPath))
                {
                    retVal.Add(System.IO.Path.GetFileName(s));
                }

                return retVal;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Utilities.DirectoryListFileNamesAsync", ex);

                return new List<string>();
            }
        }

        public async static Task<bool> DirectoryCreateAsync(string parentFolderPath, string childFolderName)
        {
            return DirectoryCreate(System.IO.Path.Combine(parentFolderPath, childFolderName));

        }

#pragma warning restore 1998
    }
}
