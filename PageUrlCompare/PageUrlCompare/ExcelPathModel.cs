using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageUrlCompare
{

    public class ExcelPathModel
    {
        public ExcelPathModel(string url, string pageTitle, string pageMaster, string creater)
        {
            Url = url;
            PageTitle = pageTitle;
            PageMaster = pageMaster;
            Creater = creater;
        }

        /// <summary>
        /// 页面路径
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 页面标题
        /// </summary>
        public string PageTitle { get; set; }
        /// <summary>
        /// 页面模板页
        /// </summary>
        public string PageMaster { get; set; }
        /// <summary>
        /// 负责人
        /// </summary>
        public string Creater { get; set; }

    }
}
