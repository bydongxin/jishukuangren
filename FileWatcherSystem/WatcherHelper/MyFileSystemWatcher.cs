using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WatcherHelper
{
    public delegate void Completed(string key);

    public class MyFileSystemWatcher
    {

        private readonly FileSystemWatcher fsWather;
        private readonly Hashtable hstbWather;
        public event RenamedEventHandler OnRenamed;
        public event FileSystemEventHandler OnChanged;
        public event FileSystemEventHandler OnCreated;
        public event FileSystemEventHandler OnDeleted;

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="path">要监控的路径</param>
        /// <param name="filter"></param> 
        public MyFileSystemWatcher(string path, string filter)
        {
            if (!Directory.Exists(path))
            {
                throw new Exception("哟，小伙，这路径找不到呀：" + path);
            }

            hstbWather = new Hashtable();

            fsWather = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                Filter = filter
            };
            // 是否监控子目录
            fsWather.Renamed += fsWather_Renamed;
            fsWather.Changed += fsWather_Changed;
            fsWather.Created += fsWather_Created;
            fsWather.Deleted += fsWather_Deleted;
        }

        /// <summary> 
        /// 开始监控 
        /// </summary> 
        public void Start()
        {
            fsWather.EnableRaisingEvents = true;
        }

        /// <summary> 
        /// 停止监控 
        /// </summary> 
        public void Stop()
        {
            fsWather.EnableRaisingEvents = false;
        }

        /// <summary> 
        /// filesystemWatcher 本身的事件通知处理过程 
        /// </summary> 
        /// <param name="sender"></param> 
        /// <param name="e"></param> 
        private void fsWather_Renamed(object sender, RenamedEventArgs e)
        {
            lock (hstbWather)
            {
                hstbWather.Add(e.FullPath, e);
            }

            WatcherProcess watcherProcess = new WatcherProcess(sender, e);
            watcherProcess.OnCompleted += new Completed(WatcherProcess_OnCompleted);
            watcherProcess.OnRenamed += new RenamedEventHandler(WatcherProcess_OnRenamed);
            Thread thread = new Thread(watcherProcess.Process);
            thread.Start();
        }

        private void WatcherProcess_OnRenamed(object sender, RenamedEventArgs e)
        {
            OnRenamed?.Invoke(sender, e);
        }

        private void fsWather_Created(object sender, FileSystemEventArgs e)
        {
            lock (hstbWather)
            {
                hstbWather.Add(e.FullPath, e);
            }
            WatcherProcess watcherProcess = new WatcherProcess(sender, e);
            watcherProcess.OnCompleted += new Completed(WatcherProcess_OnCompleted);
            watcherProcess.OnCreated += new FileSystemEventHandler(WatcherProcess_OnCreated);
            Thread threadDeal = new Thread(watcherProcess.Process);
            threadDeal.Start();
        }

        private void WatcherProcess_OnCreated(object sender, FileSystemEventArgs e)
        {
            OnCreated?.Invoke(sender, e);
        }

        private void fsWather_Deleted(object sender, FileSystemEventArgs e)
        {
            lock (hstbWather)
            {
                hstbWather.Add(e.FullPath, e);
            }
            WatcherProcess watcherProcess = new WatcherProcess(sender, e);
            watcherProcess.OnCompleted += new Completed(WatcherProcess_OnCompleted);
            watcherProcess.OnDeleted += new FileSystemEventHandler(WatcherProcess_OnDeleted);
            Thread tdDeal = new Thread(watcherProcess.Process);
            tdDeal.Start();
        }

        private void WatcherProcess_OnDeleted(object sender, FileSystemEventArgs e)
        {
            OnDeleted?.Invoke(sender, e);
        }

        private void fsWather_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                if (hstbWather.ContainsKey(e.FullPath))
                {
                    WatcherChangeTypes oldType = ((FileSystemEventArgs)hstbWather[e.FullPath]).ChangeType;
                    if (oldType == WatcherChangeTypes.Created || oldType == WatcherChangeTypes.Changed)
                    {
                        return;
                    }
                }
            }

            lock (hstbWather)
            {
                hstbWather.Add(e.FullPath, e);
            }
            WatcherProcess watcherProcess = new WatcherProcess(sender, e);
            watcherProcess.OnCompleted += new Completed(WatcherProcess_OnCompleted);
            watcherProcess.OnChanged += new FileSystemEventHandler(WatcherProcess_OnChanged);
            Thread thread = new Thread(watcherProcess.Process);
            thread.Start();
        }

        private void WatcherProcess_OnChanged(object sender, FileSystemEventArgs e)
        {
            OnChanged?.Invoke(sender, e);
        }

        private void WatcherProcess_OnCompleted(string key)
        {
            lock (hstbWather)
            {
                hstbWather.Remove(key);
            }
        }
    }
}
