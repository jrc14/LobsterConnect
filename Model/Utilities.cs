/*
    Copyright (C) 2025 Turnipsoft Ltd, Jim Chapman

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.Model
{
    /// <summary>
    /// Utility methods for getting an id that uniquely represents this installation, and for hashing strings,
    /// and hashing the password in particular, and for doing various file management operations.
    /// </summary>
    static public class Utilities
    {
        /// <summary>
        /// Get a key that uniquely identifies this device, so that the cloud sync process knows which updates came from
        /// which devices. Note that, strictly, it identifies an 'installation of the app'.  If you uninstall and 
        /// re-install the app, you'll get a different id.  That's the right behaviour because it will mean that
        /// a fresh installation gets the whole cloud journal sent to it to load initially.
        /// </summary>
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
                        //int r = System.Random.Shared.Next();
                        //_installationId = String.Format("{0:X8}", r);
                        _installationId = Guid.NewGuid().ToString("N");

                        Microsoft.Maui.Storage.Preferences.Set("InstallationId", _installationId);
                    }

                    return _installationId;
                }
            }
        }
        private static string _installationId = null;

        /// <summary>
        /// Call this method to make the app create a new installation id for this device.  This will have the effect
        /// of making it think that all existing journal entries that it previously recognised as coming from this
        /// device are now coming from some other device and not this one.
        /// </summary>
        public static void ResetInstallationId()
        {
            Microsoft.Maui.Storage.Preferences.Remove("InstallationId");
            _installationId = null;
        }

        /// <summary>
        /// Turn a password into a very irreversible hash.  The method makes a string using only the even
        /// numbered characters of the password, then makes a hash value by doing 
        /// for (int i = 0; i &lt; s.Length; i++) { h = 31 * h + s[i];}, then formatting the resultant Int32
        /// as a hex string.  It's a crappy hash, but it does mean there isn't much chance of some black-hatted
        /// person using entries in the password database to figure out the original passwords.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string PasswordHash(string p)
        {
            if (string.IsNullOrEmpty(p))
                return "";

            string h = "";

            for(int i=0; i<p.Length;i++)
            {
                if (i % 2 == 0)
                    h = h + p[i];
            }

            string hashed= GetHashCodeForString(h).ToString("X8");

            return hashed;
        }

        /// <summary>
        /// Generate a hash code for strings that is consistent between instances and launches of the app (because lately String.GetHashCode() does not seem 
        /// to be doing that for me.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Int32 GetHashCodeForString(string s)
        {
            try
            {
                // 
                int h = 0;
                if (!string.IsNullOrEmpty(s))
                {
                    for (int i = 0; i < s.Length; i++)
                    {
                        h = 31 * h + s[i];
                    }
                }
                return h;
            }
            catch (Exception e)
            {
                Logger.LogMessage(Logger.Level.ERROR, "Utilities.GetHashCodeForString: Exception: ", e);
                return 0;
            }
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
