using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace PageUrlCompare
{

    public class NpoiHelper
    {
        #region Read excel

        public static DataSet ReadExcel(string path)
        {
            IWorkbook workbook = ReadExcelToWorkbook(path);
            DataSet ds = ReadExcel(workbook);
            return ds;
        }
        /// <summary>
        /// 读取Excel
        /// </summary>
        /// <param name="path">Excel路径</param>
        /// <param name="sheetIndex">需要读取第几个Sheet，0为第一个</param>
        /// <param name="columnsIndex">表头行数</param>
        /// <returns></returns>
        public static DataSet ReadExcel(string path, int sheetIndex, int columnsIndex)
        {
            IWorkbook workbook = ReadExcelToWorkbook(path);
            DataSet ds = ReadExcel(workbook, sheetIndex, columnsIndex);
            return ds;
        }
        public static DataSet ReadExcel(IWorkbook workbook, int sheetIndex, int columnsIndex)
        {
            DataSet ds = new DataSet();

            if (workbook != null)
            {
                ISheet sheet = workbook.GetSheetAt(sheetIndex);

                if (sheet.LastRowNum > 0)
                {
                    DataTable table = new DataTable();
                    IRow headerRow = sheet.GetRow(columnsIndex);
                    int rowCount = sheet.LastRowNum + 1;
                    int columnsCount = headerRow.LastCellNum;

                    for (int num = 0; num < columnsCount; num++)
                    {
                        ICell cell = headerRow.GetCell(num);
                        if (cell != null)
                            table.Columns.Add(cell.ToString());
                    }

                    for (int j = 0; j < rowCount; j++)
                    {
                        IRow row = sheet.GetRow(j);

                        if (row != null)
                        {
                            DataRow dr = table.NewRow();

                            for (int k = 0; k < columnsCount; k++)
                            {
                                ICell cell = row.GetCell(k);

                                if (cell != null)
                                    dr[k] = cell.ToString().Trim();
                            }

                            table.Rows.Add(dr);
                        }
                    }

                    sheet = null;
                    ds.Tables.Add(table);
                }
            }

            return ds;
        }
        public static DataSet ReadExcel(IWorkbook workbook)
        {
            DataSet ds = new DataSet();

            if (workbook != null)
            {
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    ISheet sheet = workbook.GetSheetAt(i);

                    if (sheet.LastRowNum > 0)
                    {
                        DataTable table = new DataTable();
                        IRow headerRow = sheet.GetRow(0);
                        int rowCount = sheet.LastRowNum + 1;
                        int columnsCount = headerRow.LastCellNum;

                        for (int num = 0; num < columnsCount; num++)
                        {
                            table.Columns.Add(num.ToString());
                        }

                        for (int j = 0; j < rowCount; j++)
                        {
                            IRow row = sheet.GetRow(j);

                            if (row != null)
                            {
                                DataRow dr = table.NewRow();

                                for (int k = 0; k < columnsCount; k++)
                                {
                                    ICell cell = row.GetCell(k);

                                    if (cell != null)
                                        dr[k] = cell.ToString();
                                }

                                table.Rows.Add(dr);
                            }
                        }

                        sheet = null;
                        ds.Tables.Add(table);
                    }
                }
            }

            return ds;
        }

        public static IWorkbook ReadExcelToWorkbook(string path)
        {
            IWorkbook workbook = null;

            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
            }

            return workbook;
        }
        public static IWorkbook ReadExcelToWorkbook(Stream stream)
        {
            IWorkbook workbook = new XSSFWorkbook(stream);
            return workbook;
        }

        #endregion

        #region Writing excel

        public static MemoryStream WritingExcel(IWorkbook workbook, DataSet ds)
        {
            MemoryStream ms = new MemoryStream();
            if (workbook.NumberOfSheets >= ds.Tables.Count)
            {
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    if ((i + 1) <= ds.Tables.Count)
                    {
                        workbook.SetSheetName(i, ds.Tables[i].TableName);
                        ISheet sheet = workbook.GetSheetAt(i);
                        AppendWorksheet(ref sheet, ds.Tables[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }

        #endregion

        public static MemoryStream WritingExcelExt(IWorkbook workbook, DataSet ds)
        {
            MemoryStream ms = new MemoryStream();

            if (workbook.NumberOfSheets >= ds.Tables.Count)
            {
                for (int i = 0; i < workbook.NumberOfSheets; i++)
                {
                    if ((i + 1) <= ds.Tables.Count)
                    {
                        workbook.SetSheetName(i, ds.Tables[i].TableName);
                        ISheet sheet = workbook.GetSheetAt(i);
                        AppendWorksheetExt(ref sheet, ds.Tables[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            return ms;
        }
        public static void AppendWorksheetExt(ref ISheet sheet, DataTable dt)
        {
            int startRowNum = sheet.LastRowNum + 1;
            int start = 0;//记录同组开始行号 
            int end = 0;//记录同组结束行号 
            string temp = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet.CreateRow(i + startRowNum);

                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = row.CreateCell(j);
                    string value = dt.Rows[i][j] != null ? dt.Rows[i][j].ToString() : "";
                    cell.SetCellValue(value);


                    if (j == 0)
                    {
                        if (value == temp)//上下行相等，记录要合并的最后一行 
                        {
                            end = i + 2;
                        }
                        else//上下行不等，记录 
                        {
                            if (start != end)
                            {
                                CellRangeAddress region = new CellRangeAddress(start, end, 0, 0);
                                sheet.AddMergedRegion(region);

                                CellRangeAddress region1 = new CellRangeAddress(dt.Rows.Count, dt.Rows.Count + 1, 0, 0);
                                sheet.AddMergedRegion(region1);

                            }
                            start = i + 2;

                            end = i + 2;

                            temp = value;
                        }
                    }
                }
            }
        }
        #region Append worksheet

        public static void AppendWorksheet(ref ISheet sheet, DataTable dt)
        {
            int startRowNum = sheet.LastRowNum + 1;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow dataRow = sheet.CreateRow(startRowNum + i);

                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = dataRow.CreateCell(j);
                    string value = dt.Rows[i][j] != null ? dt.Rows[i][j].ToString() : "";
                    cell.SetCellValue(value);
                }
            }
        }

        #endregion


    }
}