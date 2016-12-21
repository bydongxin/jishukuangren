using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileWatcherSystem.Properties;
using WatcherHelper;
using static System.Console;

namespace FileWatcherSystem
{
    public partial class FileWatcherSystem : Form
    {

        ///这里在窗体上没有拖拽一个NotifyIcon控件，而是在这里定义了一个变量
        private NotifyIcon notifyIcon = null;
        public FileWatcherSystem()
        {
            InitializeComponent();
        }

        private void FileWatcherSystem_Load(object sender, EventArgs e)
        {
            Run();
            //调用初始化托盘显示函数  
            InitialTray();
        }

        private void Run()
        {
            foreach (var config in WatcherConfigHelper.WatcherModelList)
            {
                MyFileSystemWatcher myWather = new MyFileSystemWatcher(config.WatcherPath, config.SyncPath, config.WatcherFilter);
                myWather.OnChanged += OnChanged;
                myWather.OnCreated += OnCreated;
                myWather.OnRenamed += OnRenamed;
                myWather.OnDeleted += OnDeleted;
                myWather.Start();
            }
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            if (CheckPath(e))
            {
                SyncFile(e);
                WriteLine(@"新建事件处理逻辑：" + e.FullPath);
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            if (CheckPath(e))
            {
                SyncFile(e);
                WriteLine(@"改变事件处理逻辑：" + e.FullPath);
            }
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {
            if (CheckPath(e))
            {
                SyncFile(e);
                WriteLine(@"删除事件处理逻辑：" + e.FullPath);
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            if (CheckPath(e))
            {
                SyncFile(e);
                WriteLine(@"重命名事件处理逻辑：" + e.FullPath);
            }
        }


        /// <summary>
        /// 同步
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
                            string syncPath = watcherPath.Replace(watcherPath, WatcherConfigHelper.GetSyncPathByWatcherPath(watcherPath));
                            string targetPath = newFullPath.Replace(watcherPath, WatcherConfigHelper.GetSyncPathByWatcherPath(watcherPath));
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
                            string syncPath = watcherPath.Replace(watcherPath, WatcherConfigHelper.GetSyncPathByWatcherPath(watcherPath));
                            string targetPath = newFullPath.Replace(watcherPath, WatcherConfigHelper.GetSyncPathByWatcherPath(watcherPath));
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
                    string targetDeletePath = fullPath.Replace(fullPath, WatcherConfigHelper.GetSyncPathByWatcherPath(fullPath));
                    if (!File.Exists(fullPath) && File.Exists(targetDeletePath))
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
                            string syncPath = path.Replace(watcherPath, WatcherConfigHelper.GetSyncPathByWatcherPath(watcherPath));
                            FileInfo changeFile = new FileInfo(watcherPath);
                            changeFile.CopyTo(syncPath, true);

                        }
                        else if (Directory.Exists(path))
                        {
                            string targetPath = path.Replace(path, WatcherConfigHelper.GetSyncPathByWatcherPath(path));
                            if (!Directory.Exists(targetPath))
                            {
                                CopyUpdateFile(path, targetPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(1000);
                        goto DEFAULT;
                    }

                    break;
            }

        }


        private static bool CheckPath(FileSystemEventArgs e)
        {
            if (e.Name.Contains("Web.config"))
            {
                return false;
            }
            if (e.FullPath.EndsWith("~") || e.FullPath.EndsWith(".designer.cs") || e.FullPath.EndsWith(".TMP"))
            {
                return false;
            }
            return true;
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



        private void InitialTray()
        {
            //实例化一个NotifyIcon对象  
            notifyIcon = new NotifyIcon();
            //托盘图标气泡显示的内容  
            notifyIcon.BalloonTipText = @"项目同步工具-EBS前台专用，正在后台运行";
            //托盘图标显示的内容  
            notifyIcon.Text = @"项目同步工具-EBS前台专用";
            //注意：下面的路径可以是绝对路径、相对路径。但是需要注意的是：文件必须是一个.ico格式  
            notifyIcon.Icon = Resources.J;
            //true表示在托盘区可见，false表示在托盘区不可见  
            notifyIcon.Visible = true;
            //气泡显示的时间（单位是毫秒）  
            notifyIcon.ShowBalloonTip(2000);
            notifyIcon.MouseClick += notifyIcon_MouseClick;

            //设置二级菜单  
            MenuItem setting1 = new MenuItem("二级菜单1");
            MenuItem setting2 = new MenuItem("二级菜单2");
            MenuItem setting = new MenuItem("一级菜单", new[] { setting1, setting2 });

            //帮助选项，这里只是“有名无实”在菜单上只是显示，单击没有效果，可以参照下面的“退出菜单”实现单击事件  
            MenuItem help = new MenuItem("帮助");

            //关于选项  
            MenuItem about = new MenuItem("关于");

            //退出菜单项  
            MenuItem exit = new MenuItem("退出");
            exit.Click += exit_Click;

            ////关联托盘控件  
            //注释的这一行与下一行的区别就是参数不同，setting这个参数是为了实现二级菜单  
            MenuItem[] childen = { setting, help, about, exit };
            notifyIcon.ContextMenu = new ContextMenu(childen);

            //窗体关闭时触发  
            FormClosing += FileWatcherSystem_FormClosing;
        }

        /// <summary>  
        /// 鼠标单击  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            //鼠标左键单击  
            if (e.Button == MouseButtons.Left)
            {
                //如果窗体是可见的，那么鼠标左击托盘区图标后，窗体为不可见  
                if (Visible)
                {
                    Visible = false;
                }
                else
                {
                    Visible = true;
                    Activate();
                }
            }
        }

        /// <summary>  
        /// 退出选项  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        private void exit_Click(object sender, EventArgs e)
        {
            //退出程序  
            Environment.Exit(0);
        }

        private void FileWatcherSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            //通过这里可以看出，这里的关闭其实不是真正意义上的“关闭”，而是将窗体隐藏，实现一个“伪关闭”  
            Hide();
        }
    }
}
