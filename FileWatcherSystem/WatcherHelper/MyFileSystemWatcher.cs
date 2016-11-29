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
        public string WatcherPath { get; set; }
        public string SyncPath { get; set; }
        public event FileSystemEventHandler OnChanged;
        public event FileSystemEventHandler OnCreated;
        public event FileSystemEventHandler OnDeleted;

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="watcherPath">监听路径</param>
        /// <param name="syncPath">同步路径</param>
        /// <param name="filter">监听过滤</param>
        public MyFileSystemWatcher(string watcherPath, string syncPath, string filter)
        {
            if (!Directory.Exists(watcherPath))
            {
                throw new Exception("哟，小伙，这路径找不到呀：" + watcherPath);
            }

            hstbWather = new Hashtable();
            WatcherPath = watcherPath;
            SyncPath = syncPath;

            fsWather = new FileSystemWatcher(watcherPath)
            {
                IncludeSubdirectories = true,// 是否监控子目录
                Filter = filter
            };
            fsWather.Renamed += fsWather_Renamed;
            fsWather.Changed += fsWather_Changed;
            fsWather.Created += fsWather_Created;
            fsWather.Deleted += fsWather_Deleted;

        }

        public void Start()
        { 
            //开始监控
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
                WatcherConfigHelper.AddHashtable(e,hstbWather);
            }

            WatcherProcess watcherProcess = new WatcherProcess(sender, e);
            watcherProcess.OnCompleted += WatcherProcess_OnCompleted;
            watcherProcess.OnRenamed += WatcherProcess_OnRenamed;
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
                WatcherConfigHelper.AddHashtable(e, hstbWather);
            }
            WatcherProcess watcherProcess = new WatcherProcess(sender, e);
            watcherProcess.OnCompleted += WatcherProcess_OnCompleted;
            watcherProcess.OnCreated += WatcherProcess_OnCreated;
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
                WatcherConfigHelper.AddHashtable(e, hstbWather);
            }
            WatcherProcess watcherProcess = new WatcherProcess(sender, e);
            watcherProcess.OnCompleted += WatcherProcess_OnCompleted;
            watcherProcess.OnDeleted += WatcherProcess_OnDeleted;
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
                WatcherConfigHelper.AddHashtable(e, hstbWather);
            }
            WatcherProcess watcherProcess = new WatcherProcess(sender, e);
            watcherProcess.OnCompleted += WatcherProcess_OnCompleted;
            watcherProcess.OnChanged += WatcherProcess_OnChanged;
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
