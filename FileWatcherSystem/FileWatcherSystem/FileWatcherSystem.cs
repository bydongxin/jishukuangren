using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileWatcherSystem.Properties;
using WatcherHelper;
using static System.Console;

namespace FileWatcherSystem
{
    public partial class FileWatcherSystem : Form
    {
        readonly MyFileSystemWatcher myWather = new MyFileSystemWatcher(WatcherConfiguration.watcherPath, WatcherConfiguration.watcherFilter);

        public FileWatcherSystem()
        {
            InitializeComponent();
        }

        private void FileWatcherSystem_Load(object sender, EventArgs e)
        {
            myWather.OnChanged += OnChanged;
            myWather.OnCreated += OnCreated;
            myWather.OnRenamed += OnRenamed;
            myWather.OnDeleted += OnDeleted;
            myWather.Start();
        }
        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            SyncFile(e);
            WriteLine("文件新建事件处理逻辑");
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            SyncFile(e);
            WriteLine("文件改变事件处理逻辑");
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            SyncFile(e);
            WriteLine("文件删除事件处理逻辑");
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            SyncFile(e);
            WriteLine("文件重命名事件处理逻辑");
        }


        /// <summary>
        /// 同步文件
        /// </summary>
        /// <param name="e"></param>
        private static void SyncFile(FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Renamed:
                    /*
                    1.判断老文件是否存在，如果不存在，先删除同步文件，再拷贝
                    2.
                    */
                    RenamedEventArgs rn = e as RenamedEventArgs;
                    if (rn != null)
                    {
                        string oldFullPath = rn.OldFullPath;
                        string newFullPath = rn.FullPath;
                        if (!File.Exists(oldFullPath) && File.Exists(newFullPath))
                        {
                            string watcherPath = oldFullPath;
                            string syncPath = watcherPath.Replace(WatcherConfiguration.watcherPath, WatcherConfiguration.syncPath);
                            string targetPath = newFullPath.Replace(WatcherConfiguration.watcherPath, WatcherConfiguration.syncPath);
                            if (File.Exists(syncPath))
                            {
                                File.Delete(syncPath);
                                FileInfo changeFile = new FileInfo(newFullPath);
                                changeFile.CopyTo(targetPath, true);
                            }
                        }
                        else if (!Directory.Exists(oldFullPath) && Directory.Exists(newFullPath))
                        {
                            string watcherPath = oldFullPath;
                            string syncPath = watcherPath.Replace(WatcherConfiguration.watcherPath, WatcherConfiguration.syncPath);
                            string targetPath = newFullPath.Replace(WatcherConfiguration.watcherPath, WatcherConfiguration.syncPath);
                            if (!Directory.Exists(targetPath))
                            {
                                CopyUpdateFile(newFullPath, targetPath);
                                if (Directory.Exists(syncPath))
                                {
                                    Directory.Delete(syncPath, true);
                                }
                            }
                        }
                    }
                    break;
                case WatcherChangeTypes.Deleted:
                    string fullPath = e.FullPath;
                    string targetDeletePath = fullPath.Replace(WatcherConfiguration.watcherPath, WatcherConfiguration.syncPath);
                    if (File.Exists(targetDeletePath))
                    {
                        File.Delete(targetDeletePath);
                    }
                    else if (Directory.Exists(targetDeletePath))
                    {
                        Directory.Delete(targetDeletePath, true);
                    }
                    break;
                default:
                    DEFAULT:
                    try
                    {

                        string path = e.FullPath;
                        if (File.Exists(path))
                        {
                            string watcherPath = path;
                            string syncPath = path.Replace(WatcherConfiguration.watcherPath, WatcherConfiguration.syncPath);
                            FileInfo changeFile = new FileInfo(watcherPath);
                            changeFile.CopyTo(syncPath, true);
                        }
                        else if (Directory.Exists(path))
                        {
                            string targetPath = path.Replace(WatcherConfiguration.watcherPath, WatcherConfiguration.syncPath);
                            if (!Directory.Exists(targetPath))
                            {
                                CopyUpdateFile(path, targetPath);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        goto DEFAULT;
                    }

                    break;
            }

        }


        private static void CopyUpdateFile(string srcPath, string aimPath)
        {
            if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                aimPath += Path.DirectorySeparatorChar;
            if (!Directory.Exists(aimPath))
                Directory.CreateDirectory(aimPath);
            if (!Directory.Exists(srcPath))
                return;
            string[] fileList = Directory.GetFileSystemEntries(srcPath);

            foreach (string file in fileList)
            {
                if (Directory.Exists(file))
                    CopyUpdateFile(file, aimPath + Path.GetFileName(file));
                else
                {
                    try
                    {
                        File.Copy(file, aimPath + Path.GetFileName(file), true);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("拷贝出错", e);
                    }
                }
            }
        }


    }
}
