using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatcherHelper
{
    public class WatcherProcess
    {
        private readonly object sender;
        private readonly object eParam;

        public event RenamedEventHandler OnRenamed;
        public event FileSystemEventHandler OnChanged;
        public event FileSystemEventHandler OnCreated;
        public event FileSystemEventHandler OnDeleted;
        public event Completed OnCompleted;

        public WatcherProcess(object sender, object eParam)
        {
            this.sender = sender;
            this.eParam = eParam;
        }

        public void Process()
        {
            if (eParam.GetType() == typeof(RenamedEventArgs))
            {
                OnRenamed?.Invoke(sender, (RenamedEventArgs)eParam);
                OnCompleted?.Invoke(((RenamedEventArgs)eParam).FullPath);
            }
            else
            {
                FileSystemEventArgs e = (FileSystemEventArgs)eParam;
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        OnCreated?.Invoke(sender, e);
                        OnCompleted?.Invoke(e.FullPath);
                        break;
                    case WatcherChangeTypes.Changed:
                        OnChanged?.Invoke(sender, e);
                        OnCompleted?.Invoke(e.FullPath);
                        break;
                    case WatcherChangeTypes.Deleted:
                        OnDeleted?.Invoke(sender, e);
                        OnCompleted?.Invoke(e.FullPath);
                        break;
                    default:
                        OnCompleted?.Invoke(e.FullPath);
                        break;
                }
            }
        }
    }
}
