using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WatcherHelper
{
    /// <summary>
    /// 监听类
    /// </summary>
    public  class WatcherPathConfig
    {
        List<WatcherItem> watcherItems = new List<WatcherItem>();

        [XmlElement(ElementName = "WatcherItem")]
        public List<WatcherItem> Watcher
        {
            get { return watcherItems; }
            set { watcherItems = value; }
        }
    }

    public class WatcherItem
    {
        /// <summary>
        /// 监听路径
        /// </summary>
        public string WatcherPath { get; set; }
        /// <summary>
        /// 同步路径
        /// </summary>
        public string SyncPath { get; set; }
        /// <summary>
        /// 监听过滤
        /// </summary>
        public string WatcherFilter { get; set; }
        /// <summary>
        /// 真实WebConfig
        /// </summary>
        public string WebConfigPath { get; set; }
    }
}
