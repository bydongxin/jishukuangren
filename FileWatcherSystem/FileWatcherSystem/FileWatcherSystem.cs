using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WatcherHelper;

namespace FileWatcherSystem
{
    public partial class FileWatcherSystem : Form
    {
        public FileWatcherSystem()
        {
            InitializeComponent();
        }

        private void btnStartWatcher_Click(object sender, EventArgs e)
        {
            MyFileSystemWather myWather = new MyFileSystemWather(@"D:\test", "*.txt");
            myWather.OnChanged += OnChanged;
            myWather.OnCreated += OnCreated;
            myWather.OnRenamed += OnRenamed;
            myWather.OnDeleted += OnDeleted;
            myWather.Start();

        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {

            Console.WriteLine("文件新建事件处理逻辑");

        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            FileInfo changeFile = new FileInfo(e.FullPath);
            changeFile.CopyTo(@"D:\test1\" + e.Name, true);
            Console.WriteLine("文件改变事件处理逻辑");
        }

        private static void OnDeleted(object source, FileSystemEventArgs e)
        {

            Console.WriteLine("文件删除事件处理逻辑");
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {

            Console.WriteLine("文件重命名事件处理逻辑");
        }


    }
}
