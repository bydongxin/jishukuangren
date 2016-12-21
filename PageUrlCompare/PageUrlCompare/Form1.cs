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

namespace PageUrlCompare
{
    public partial class Form1 : Form
    {

        const string BizSite = "Sinochem.EBS.Web.BizSite";
        const string InfoSite = "Sinochem.EBS.Web.InfoSite";
        const string OperSite = "Sinochem.EBS.Web.OperSite";
        public Form1()
        {
            InitializeComponent();
            InitControl();


        }

        private void InitControl()
        {
            string toolTip = "路径必须为TFS项目路径。" + Environment.NewLine + "例如： X:\\xxx\\EBS\\Code\\Sinochem.EBS.Web";

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 100;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(this.btnOpenProject, toolTip);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = @"请选择Excel文件",
                Filter = @"Excel文件(*.xlsx)|*.xlsx"
            };
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtExcelPath.Text = fileDialog.FileName;
            }
        }

        private void bntCompare_Click(object sender, EventArgs e)
        {
            string excelPath = txtExcelPath.Text.Trim();
            string projectPath = txtProjectPath.Text.Trim();
            if (string.IsNullOrEmpty(excelPath))
            {
                MessageBox.Show(@"Excel文件路径不能为空", @"请选择Excel文件", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(projectPath))
            {
               
                MessageBox.Show(@"项目路径不能为空", @"请选择项目路径", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Directory.Exists(projectPath))
            {
                string tip = "错误的项目路径。" + Environment.NewLine + "路径必须为TFS项目路径。" + Environment.NewLine + "例如： X:\\xxx\\EBS\\Code\\Sinochem.EBS.Web";
                MessageBox.Show(tip, @"警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ComparePath(excelPath, projectPath);


        }

        /// <summary>
        /// 对比路径
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="projectPath"></param>
        private void ComparePath(string excelPath, string projectPath)
        {
            List<ExcelPathModel> excelModels = new List<ExcelPathModel>();
            List<ProjectPathModel> projectPathModels = new List<ProjectPathModel>();
            try
            {
                //获取Excel数据封装到List
                DataSet dataSet = NpoiHelper.ReadExcel(excelPath, 3, 1);
                if (dataSet != null && dataSet.Tables[0] != null)
                {
                    DataTable dt = dataSet.Tables[0];
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        string url = dataRow["URL"] + "";
                        string title = dataRow["页面标题"] + "";
                        string master = dataRow["母版页"] + "";
                        string creater = dataRow["负责人"] + "";
                        if (Path.GetExtension(url) == ".aspx")
                        {
                            excelModels.Add(new ExcelPathModel(url, title, master, creater));
                        }
                    }
                }
                string bizPath = Path.Combine(projectPath, BizSite);
                string infoPath = Path.Combine(projectPath, InfoSite);
                string operPath = Path.Combine(projectPath, OperSite);

                //获取项目路径下所有.aspx结尾文件
                ListFiles(new DirectoryInfo(bizPath), projectPathModels, BizSite);
                ListFiles(new DirectoryInfo(infoPath), projectPathModels, InfoSite);
                ListFiles(new DirectoryInfo(operPath), projectPathModels, OperSite);

                //对比
                foreach (ProjectPathModel projectPathModel in projectPathModels)
                {
                    foreach (var excelModel in excelModels)
                    {
                        string tmpProPath = projectPathModel.FilePath;
                        string tmpExcelPath = excelModel.Url;
                        if (tmpProPath == tmpExcelPath)
                        {
                            projectPathModel.IsExists = true;
                            projectPathModel.PageTitle = excelModel.PageTitle;
                            projectPathModel.PageMaster = excelModel.PageMaster;
                            projectPathModel.Creater = excelModel.Creater;
                        }
                    }
                }
                IList<ProjectPathModel> lstPathModels = projectPathModels.Where(i => i.IsExists == false).ToList();
                dgvData.DataSource = lstPathModels.Select(i => new
                {
                    Project = i.Project,
                    FilePath = i.FilePath,
                }).ToList();
                string resultStr = "项目文件中获取URL：" + projectPathModels.Count + "条" + Environment.NewLine +
                                   "Excel中获取URL：" + excelModels.Count + "条" + Environment.NewLine +
                                   "差集：" + lstPathModels.Count() + "条" + Environment.NewLine +
                                   "交易站点：" + lstPathModels.Count(i => i.Project == BizSite) + "条" + Environment.NewLine +
                                   "运营站点：" + lstPathModels.Count(i => i.Project == OperSite) + "条" + Environment.NewLine +
                                   "匿名站点：" + lstPathModels.Count(i => i.Project == InfoSite) + "条";

                MessageBox.Show(resultStr, @"对比结果", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show(@"对比路径出错：" + ex.StackTrace, @"警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// 递归获取文件夹下所有文件
        /// </summary>
        /// <param name="info">路径</param>
        /// <param name="projectPathModels"></param>
        /// <param name="projectName">项目名称</param>
        private static void ListFiles(FileSystemInfo info, List<ProjectPathModel> projectPathModels, string projectName)
        {
            if (!info.Exists) return;
            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录   
            if (dir == null) return;
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件   
                if (file != null)
                {
                    string path = file.FullName;
                    string pathLower = path.ToLower();
                    if (Path.GetExtension(path) == ".aspx" && !pathLower.Contains(@"obj\debug") && !pathLower.Contains(@"obj\release"))
                    {
                        var comparePath = path.Substring(path.IndexOf(projectName, StringComparison.Ordinal) + projectName.Length);
                        projectPathModels.Add(new ProjectPathModel(projectName, comparePath.Replace("\\", "/"), false));
                    }
                }
                //对于子目录，进行递归调用   
                else
                    ListFiles(files[i], projectPathModels, projectName);
            }
        }

        private void btnOpenProject_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = @"请选择项目文件夹路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtProjectPath.Text = dialog.SelectedPath;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtProjectPath.Text = "";
            txtExcelPath.Text = "";
            dgvData.DataSource = null;
        }
    }
}
