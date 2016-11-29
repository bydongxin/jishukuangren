using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WatcherHelper
{
    /// <summary>
    /// 监听配置帮助类
    /// </summary>
    public class WatcherConfigHelper
    {
        public static readonly List<WatcherPathConfig> WatcherModelList;

        static WatcherConfigHelper()
        {
            string xmlStr = File.ReadAllText(@"WatcherPathConfig.xml");
            //加载xml到集合中
            WatcherModelList = XmlUtil.Deserialize(typeof(List<WatcherPathConfig>), xmlStr) as List<WatcherPathConfig>;
        }

        /// <summary>
        /// 根据站点路径获取同步路径
        /// </summary>
        /// <param name="watcherPath"></param>
        /// <returns></returns>

        public static string GetSyncPathByWatcherPath(string watcherPath)
        {
            WatcherPathConfig mdl = WatcherModelList.FirstOrDefault(i => watcherPath.StartsWith(i.WatcherPath));
            if (mdl != null)
            {
                //+1是因为Path.Combine 拼接的路径不能包含\\
                return Path.Combine(mdl.SyncPath, watcherPath.Substring(mdl.WatcherPath.Length + 1));
            }
            return "";
        }

        /// <summary>
        /// 根据站点路径获取过滤规则
        /// </summary>
        /// <param name="watcherPath"></param>
        /// <returns></returns>

        public static string GetWatcherFilter(string watcherPath)
        {
            WatcherPathConfig mdl = WatcherModelList.FirstOrDefault(i => i.WatcherPath.Contains(watcherPath));
            if (mdl != null)
            {
                return mdl.WatcherFilter;
            }
            return "";
        }

        /// <summary>
        /// 根据站点路径获取过滤规则
        /// </summary>
        /// <param name="watcherPath"></param>
        /// <returns></returns>

        public static string GetWebConfigPath(string watcherPath)
        {
            WatcherPathConfig mdl = WatcherModelList.FirstOrDefault(i => i.WatcherPath.Contains(watcherPath));
            if (mdl != null)
            {
                return mdl.WebConfigPath;
            }
            return "";
        }

        public static Hashtable AddHashtable(FileSystemEventArgs e, Hashtable hashtable)
        {
            if (!hashtable.ContainsKey(e.FullPath))
            {
                hashtable.Add(e.FullPath, e);
            }
            return hashtable;
        }
    }
}
