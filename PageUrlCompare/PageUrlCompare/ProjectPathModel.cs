using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageUrlCompare
{
    public class ProjectPathModel
    {
        public ProjectPathModel(string project, string filePath, bool isExists)
        {
            Project = project;
            FilePath = filePath;
            IsExists = isExists;
        }

        /// <summary>
        /// 所属项目
        /// </summary>
        public string Project { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool IsExists { get; set; }

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
