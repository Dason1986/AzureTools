using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AzureTools.Tools.IO
{
    public static class ExcelExporter
    {
        public static void Export<T>(Stream stream, string sheetName, string[] headers, IEnumerable<T> data, Func<T, string, string> valueSelector)
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet(sheetName);

            IRow headerRow = sheet.CreateRow(0);
            ICellStyle headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.Grey25Percent.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;

            for (int i = 0; i < headers.Length; i++)
            {
                ICell cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            List<T> dataList = data.ToList();
            for (int rowIndex = 0; rowIndex < dataList.Count; rowIndex++)
            {
                IRow row = sheet.CreateRow(rowIndex + 1);
                T item = dataList[rowIndex];
                for (int colIndex = 0; colIndex < headers.Length; colIndex++)
                {
                    row.CreateCell(colIndex).SetCellValue(valueSelector(item, headers[colIndex]) ?? string.Empty);
                }
            }

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            workbook.Write(stream);
        }
    }
}
