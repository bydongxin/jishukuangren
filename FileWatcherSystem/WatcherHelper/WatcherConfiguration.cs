using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatcherHelper
{
    public static class WatcherConfiguration
    {

        /// <summary>
        /// 监听路径
        /// </summary>
        public static readonly string watcherPath = ConfigurationManager.AppSettings["watcherPath"] ?? "";
        /// <summary>
        /// 同步路径
        /// </summary>
        public static readonly string syncPath = ConfigurationManager.AppSettings["syncPath"] ?? "";
        /// <summary>
        /// 监听过滤
        /// </summary>
        public static readonly string watcherFilter = ConfigurationManager.AppSettings["watcherFilter"] ?? "";
    }
}
