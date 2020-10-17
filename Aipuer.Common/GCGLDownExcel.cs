using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aipuer.Common
{
    public class GCGLDownExcel
    {
        #region 模板字段
        private static string[] event_excel = { "", "name", "leadername", "content", "address", "addtime", "starttime", "endtime", "wname", "pname" };
        private static string[] work_excel = { "", "name", "leadername", "content", "address", "addtime", "starttime", "endtime", "pname" };
        private static string[] project_excel = { "", "name", "leadername", "content", "address", "addtime", "starttime", "endtime" };
        private static string[] ptop = { "序号", "项目名", "负责人", "内容", "地址", "添加时间", "开始时间", "结束时间","完成度"};
        private static string[] wtop = { "序号", "工作名", "负责人", "内容", "地址", "添加时间", "开始时间", "结束时间", "所属项目" ,"完成度"};
        private static string[] etop = { "序号", "事件名", "负责人", "内容", "地址", "添加时间", "开始时间", "结束时间", "所属工作", "所属项目" ,"完成度"};
        #endregion
        #region 主函数区
        /// <summary>
        /// ExportExcel（使用NPOI的方式）
        /// </summary>
        /// <param name="DT"></param>
        public  string ExportExcel(DataTable DTP, DataTable DTW, DataTable DTE, string modelExlPath, string strFilePath,string state)
        {
            try
            {
                HSSFWorkbook hssfworkbookDown = new HSSFWorkbook(); 
                
                if (DTP != null || DTW != null || DTE != null)
                {
                    if (DTP != null && DTP.Rows.Count > 0&&state=="0")//项目
                    {
                        HSSFSheet sheetp = (HSSFSheet)hssfworkbookDown.CreateSheet("项目");//创建表
                        {///尝试创建表头
                            HSSFDataFormat format = CreateTop(hssfworkbookDown, sheetp,ptop, "0");
                        }
                        WriterExcelP(hssfworkbookDown, 0, DTP);
                    }
                    if (DTW != null && DTW.Rows.Count > 0&&state!="2")
                    {
                        HSSFSheet sheetw = (HSSFSheet)hssfworkbookDown.CreateSheet("工作");//创建表
                        {///尝试创建表头
                            HSSFDataFormat format = CreateTop(hssfworkbookDown, sheetw, wtop, "1");
                        }
                        if (state == "0")
                        {
                            WriterExcelW(hssfworkbookDown, 1, DTW);
                        }
                        else
                        {
                            WriterExcelW(hssfworkbookDown, 0, DTW);
                        }
                    }
                    if (DTE != null && DTE.Rows.Count > 0)
                    {
                        HSSFSheet sheete = (HSSFSheet)hssfworkbookDown.CreateSheet("事件");//创建表
                        {///尝试创建表头
                            HSSFDataFormat format = CreateTop(hssfworkbookDown, sheete, etop, "2");
                        }
                        if (state == "0")
                        {
                            WriterExcelE(hssfworkbookDown, 2, DTE);
                        }
                        else if(state=="1")
                        {
                            WriterExcelE(hssfworkbookDown, 1, DTE);
                        }
                        else
                        {
                            WriterExcelE(hssfworkbookDown, 0, DTE);
                        }
                    }
                    string filename = Guid.NewGuid().ToString() + ".xls";
                    if (Directory.Exists(strFilePath) == false)
                    {
                        Directory.CreateDirectory(strFilePath);
                    }
                    strFilePath = strFilePath + filename;
                    FileStream files = new FileStream(strFilePath, FileMode.Create);
                    hssfworkbookDown.Write(files);
                    files.Close();
                    if (File.Exists(strFilePath) == false)//附件生成失败
                    {
                        return "";
                    }
                    return filename;
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        #endregion

        #region 表格内容填充区
        /// <summary>
        /// WriterExcel
        /// </summary>
        /// <param name="hssfworkbookDown"></param>
        /// <param name="sheetIndex"></param>
        /// <param name="DT"></param>
        public static void WriterExcelE(HSSFWorkbook hssfworkbookDown, int sheetIndex, DataTable DT)
        {
            try
            {
                #region 设置单元格样式
                //字体
                HSSFFont fontS9 = FontS9(hssfworkbookDown);
                //表格
                ICellStyle tableS1 = TableS1(hssfworkbookDown);
                tableS1.SetFont(fontS9);
                ICellStyle tableS2 = TableS2(hssfworkbookDown);
                tableS2.SetFont(fontS9);
                ICellStyle tableS3 = TableS3(hssfworkbookDown);
                tableS3.SetFont(fontS9);
                ICellStyle tableS4 = TableS4(hssfworkbookDown);
                tableS4.SetFont(fontS9);
                #endregion

                HSSFSheet sheet = (HSSFSheet)hssfworkbookDown.GetSheetAt(sheetIndex);
                hssfworkbookDown.SetSheetHidden(sheetIndex, false);
                hssfworkbookDown.SetActiveSheet(sheetIndex);

                int n = 1;//因为模板有表头，所以从第2行开始写
                for (int j = 0; j < DT.Rows.Count; j++)
                {
                    HSSFRow dataRow = (HSSFRow)sheet.CreateRow(j + n);
                    dataRow.CreateCell(0);
                    dataRow.Cells[0].SetCellValue(j + 1);
                    for (int i = 1; i < event_excel.Length; i++)
                    {
                        dataRow.CreateCell(i);
                        dataRow.Cells[i].SetCellValue(DT.Rows[j][event_excel[i]].ToString());
                        dataRow.Cells[i].CellStyle = tableS2;
                        if (event_excel[i].ToString() == "content")
                            dataRow.Cells[i].CellStyle = tableS3;
                    }
                    dataRow.CreateCell(event_excel.Length);
                    dataRow.Cells[event_excel.Length].SetCellValue(DT.Rows[j]["flag"].ToString() == "0" ? "" : "已完成");
                    if (DT.Rows[j]["flag"].ToString() == "1")
                    {
                        for (int i = 0; i <= event_excel.Length; i++)
                        {
                            dataRow.Cells[i].CellStyle = tableS1;
                            if (i != event_excel.Length && event_excel[i].ToString() == "content")
                                dataRow.Cells[i].CellStyle = tableS4;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// WriterExcel
        /// </summary>
        /// <param name="hssfworkbookDown"></param>
        /// <param name="sheetIndex"></param>
        /// <param name="DT"></param>
        public static void WriterExcelW(HSSFWorkbook hssfworkbookDown, int sheetIndex, DataTable DT)
        {
            try
            {
                #region 设置单元格样式
                //字体
                HSSFFont fontS9 = FontS9(hssfworkbookDown);
                //表格
                ICellStyle tableS1 = TableS1(hssfworkbookDown);
                tableS1.SetFont(fontS9);
                ICellStyle tableS2 = TableS2(hssfworkbookDown);
                tableS2.SetFont(fontS9);
                ICellStyle tableS3 = TableS3(hssfworkbookDown);
                tableS3.SetFont(fontS9);
                ICellStyle tableS4 = TableS4(hssfworkbookDown);
                tableS4.SetFont(fontS9);
                #endregion

                HSSFSheet sheet = (HSSFSheet)hssfworkbookDown.GetSheetAt(sheetIndex);
                hssfworkbookDown.SetSheetHidden(sheetIndex, false);
                hssfworkbookDown.SetActiveSheet(sheetIndex);

                int n = 1;//因为模板有表头，所以从第2行开始写
                for (int j = 0; j < DT.Rows.Count; j++)
                {
                    HSSFRow dataRow = (HSSFRow)sheet.CreateRow(j + n);
                    dataRow.CreateCell(0);
                    dataRow.Cells[0].SetCellValue(j + 1);
                    for (int i = 1; i < work_excel.Length; i++)
                    {
                        dataRow.CreateCell(i);
                        dataRow.Cells[i].SetCellValue(DT.Rows[j][work_excel[i]].ToString());
                        dataRow.Cells[i].CellStyle = tableS2;
                        if (work_excel[i].ToString() == "content")
                            dataRow.Cells[i].CellStyle = tableS3;
                    }
                    dataRow.CreateCell(work_excel.Length);
                    dataRow.Cells[work_excel.Length].SetCellValue(DT.Rows[j]["progress"].ToString() == "100" ? "已完成" : "");
                    if (DT.Rows[j]["progress"].ToString() == "100")
                    {
                        for (int i = 0; i <= work_excel.Length; i++)
                        {
                            dataRow.Cells[i].CellStyle = tableS1;
                            if (i != work_excel.Length && work_excel[i].ToString() == "content")
                                dataRow.Cells[i].CellStyle = tableS4;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// WriterExcel
        /// </summary>
        /// <param name="hssfworkbookDown"></param>
        /// <param name="sheetIndex"></param>
        /// <param name="DT"></param>
        public static void WriterExcelP(HSSFWorkbook hssfworkbookDown, int sheetIndex, DataTable DT)
        {
            try
            {
                #region 设置单元格样式
                //字体
                HSSFFont fontS9 = FontS9(hssfworkbookDown);
                //表格
                ICellStyle tableS1 = TableS1(hssfworkbookDown);
                tableS1.SetFont(fontS9);

                ICellStyle tableS2 = TableS2(hssfworkbookDown);
                tableS2.SetFont(fontS9);

                ICellStyle tableS3 = TableS3(hssfworkbookDown);
                tableS3.SetFont(fontS9);
                ICellStyle tableS4 = TableS4(hssfworkbookDown);
                tableS4.SetFont(fontS9);
                #endregion

                HSSFSheet sheet = (HSSFSheet)hssfworkbookDown.GetSheetAt(sheetIndex);
                hssfworkbookDown.SetSheetHidden(sheetIndex, false);
                hssfworkbookDown.SetActiveSheet(sheetIndex);

                int n = 1;//因为模板有表头，所以从第2行开始写
                for (int j = 0; j < DT.Rows.Count; j++)
                {
                    HSSFRow dataRow = (HSSFRow)sheet.CreateRow(j + n);
                    dataRow.CreateCell(0);
                    dataRow.Cells[0].SetCellValue(j + 1);
                    for (int i = 1; i < project_excel.Length; i++)
                    {
                        dataRow.CreateCell(i);
                        dataRow.Cells[i].SetCellValue(DT.Rows[j][project_excel[i]].ToString());
                        dataRow.Cells[i].CellStyle = tableS2;
                        if (project_excel[i].ToString() == "content")
                            dataRow.Cells[i].CellStyle = tableS3;
                    }
                    dataRow.CreateCell(project_excel.Length);
                    dataRow.Cells[project_excel.Length].SetCellValue(DT.Rows[j]["progress"].ToString() == "100" ? "已完成" : "");
                    if (DT.Rows[j]["progress"].ToString() == "100")
                    {
                        for (int i = 0; i <= project_excel.Length; i++)
                        {
                            dataRow.Cells[i].CellStyle = tableS1;
                            if (i != project_excel.Length && project_excel[i].ToString() == "content")
                                dataRow.Cells[i].CellStyle = tableS4;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        #endregion

        #region 字体设置
        //设置字体
        private static HSSFFont FontS9(HSSFWorkbook hssfworkbookDown)
        {
            HSSFFont fontS9 = (HSSFFont)hssfworkbookDown.CreateFont();
            fontS9.FontName = "Arial";
            fontS9.FontHeightInPoints = 10;
            fontS9.Boldweight = (short)FontBoldWeight.Normal;
            return fontS9;
        }
        #endregion

        #region 单元格设置
        //设置背景色为轻绿色 垂直水平居中 有边线
        private static ICellStyle TableS1(HSSFWorkbook hssfworkbookDown)
        {
            ICellStyle TableS1 = hssfworkbookDown.CreateCellStyle();
            TableS1.BorderLeft = BorderStyle.Thin;
            TableS1.BorderTop = BorderStyle.Thin;
            TableS1.BorderBottom = BorderStyle.Thin;
            TableS1.BorderRight = BorderStyle.Thin;
            TableS1.WrapText = true;
            TableS1.FillForegroundColor = HSSFColor.LightGreen.Index;
            TableS1.FillPattern = FillPattern.SolidForeground;
            TableS1.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
            TableS1.Alignment = HorizontalAlignment.Center;//水平对齐
            return TableS1;
        }
        //设置垂直水平居中 有边线
        private static ICellStyle TableS2(HSSFWorkbook hssfworkbookDown)
        {
            ICellStyle TableS2 = hssfworkbookDown.CreateCellStyle();
            TableS2.BorderLeft = BorderStyle.Thin;
            TableS2.BorderTop = BorderStyle.Thin;
            TableS2.BorderBottom = BorderStyle.Thin;
            TableS2.BorderRight = BorderStyle.Thin;
            TableS2.WrapText = true;
            TableS2.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
            TableS2.Alignment = HorizontalAlignment.Center;//水平对齐
            return TableS2;
        }
        //设置靠上靠左对齐 有边线
        private static ICellStyle TableS3(HSSFWorkbook hssfworkbookDown)
        {
            ICellStyle TableS3 = hssfworkbookDown.CreateCellStyle();
            TableS3.BorderLeft = BorderStyle.Thin;
            TableS3.BorderTop = BorderStyle.Thin;
            TableS3.BorderBottom = BorderStyle.Thin;
            TableS3.BorderRight = BorderStyle.Thin;
            TableS3.WrapText = true;
            TableS3.VerticalAlignment = VerticalAlignment.Top;//垂直靠上
            TableS3.Alignment = HorizontalAlignment.Left;//水平靠左
            return TableS3;
        }

        //设置背景色为轻绿色 靠上靠左对齐 有边线
        private static ICellStyle TableS4(HSSFWorkbook hssfworkbookDown)
        {
            ICellStyle TableS4 = hssfworkbookDown.CreateCellStyle();
            TableS4.BorderLeft = BorderStyle.Thin;
            TableS4.BorderTop = BorderStyle.Thin;
            TableS4.BorderBottom = BorderStyle.Thin;
            TableS4.BorderRight = BorderStyle.Thin;
            TableS4.WrapText = true;
            TableS4.FillForegroundColor = HSSFColor.LightGreen.Index;
            TableS4.FillPattern = FillPattern.SolidForeground;
            TableS4.VerticalAlignment = VerticalAlignment.Top;//垂直靠上
            TableS4.Alignment = HorizontalAlignment.Left;//水平靠左
            return TableS4;
        }
        /// <summary>
        /// 表头的样色设置
        /// </summary>
        /// <param name="hssfworkbookDown"></param>
        /// <returns></returns>
        private static ICellStyle TopRow(HSSFWorkbook hssfworkbookDown)
        {
            ///字体
            HSSFFont fontS9 = (HSSFFont)hssfworkbookDown.CreateFont();
            fontS9.FontName = "黑体";
            fontS9.FontHeightInPoints = 14;
            fontS9.Boldweight = (short)FontBoldWeight.Normal;
            //样式
            ICellStyle table0 = hssfworkbookDown.CreateCellStyle();
            table0.BorderLeft = BorderStyle.Thin;
            table0.BorderTop = BorderStyle.Thin;
            table0.BorderBottom = BorderStyle.Thin;
            table0.BorderRight = BorderStyle.Thin;
            table0.WrapText = true;
            table0.FillForegroundColor = 47;
            table0.FillPattern = FillPattern.SolidForeground;
            table0.VerticalAlignment = VerticalAlignment.Center;//垂直对齐
            table0.Alignment = HorizontalAlignment.Center;//水平对齐
            table0.SetFont(fontS9);
            return table0;
        }
        #endregion
        #region 创建表头格式

        public static HSSFDataFormat CreateTop(HSSFWorkbook hssfworkbookDown, HSSFSheet sheet,string [] pwe,string state)
        {
            HSSFDataFormat format = (HSSFDataFormat)hssfworkbookDown.CreateDataFormat();
            ICellStyle table0 = TopRow(hssfworkbookDown);
            {///开始创建
                HSSFRow toprow = (HSSFRow)sheet.CreateRow(0);
                toprow.Height = 2 * 256;//行高
                for (int i = 0; i < pwe.Length; i++)
                {
                    toprow.CreateCell(i).SetCellValue(pwe[i]);
                    if (i == 2 || i == 5 || i == 6 || i == 7||i==10)
                    {
                        sheet.SetColumnWidth(i, 18 * 256);//第一列宽
                    }
                    else if (i==1||i==4)//宽点
                    {
                        sheet.SetColumnWidth(i, 36 * 256);//第一列宽
                    }
                    else if(i==3){
                        sheet.SetColumnWidth(i, 54 * 256);//第一列宽
                    }
                    else if(i==8||i==9)
                    {
                        if (i == 8)///项目完成度
                        {
                            if (state == "0")
                            {
                                sheet.SetColumnWidth(i, 18 * 256);
                            }
                            else
                            {
                                sheet.SetColumnWidth(i, 36 * 256);//第一列宽
                            }
                            //第一列宽
                        }
                        else if (i == 9)
                        {

                            if (state == "1")
                            {
                                sheet.SetColumnWidth(i, 18 * 256);//第一列宽
                            }
                            else if (state == "2")
                            {
                                sheet.SetColumnWidth(i, 36 * 256);//第一列宽
                            }
                        }
                    }
                    toprow.GetCell(i).CellStyle = table0;
                }
            }
            sheet.CreateFreezePane(1, 1, 1, 1);//锁定单元格
            return format;
        }
        #endregion
    }
}
