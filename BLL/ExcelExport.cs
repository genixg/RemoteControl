using OfficeOpenXml;
using OfficeOpenXml.Style;
using RemoteControl.Models.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteControl.BLL
{
    public class ExcelExport
	{
		private ExcelWorksheet worksheet;
		private int currentRow = 0;
		private int rowHeaderStart = 1;
		private int rowHeaderEnd = 1;
		private int totalColumns = 0;
		string pathToImages;

		public string GetMonitoringReportXLSX(DateTime datestart, DateTime dateto, string filepath, List<EmployeeDTO> employees, List<DepartmentDTO> depts, string documentTitle = "Мониторинг работы сотрудников")
		{
			pathToImages = AppDomain.CurrentDomain.BaseDirectory + "images\\";

			string path = filepath;
			var newFile = new FileInfo(path);
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			using (var package = new ExcelPackage(newFile))
			{
				ExcelRange cell;
				worksheet = package.Workbook.Worksheets.Add("Лист 1");
				worksheet.View.ZoomScale = 85;
				// Общие стили
				worksheet.DefaultColWidth = 12;
				worksheet.Cells.Style.Font.Name = "Timer New Roman";
				worksheet.Cells.Style.Font.Size = 10;
				worksheet.Cells.Style.WrapText = false;
				worksheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				worksheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
				//worksheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
				//worksheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

				#region шапка документа

				currentRow = 1;
				// Заголовок в первой строке документа (потому что нужно число колонок)
				cell = worksheet.Cells[currentRow, 1, currentRow, 4];
				cell.Merge = true;
				cell.Value = documentTitle;
				cell.Style.Font.Bold = true;
				cell.Style.Font.Size = 11;
				cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				worksheet.Cells[currentRow, 1].Style.WrapText = false;
				worksheet.Row(currentRow).Height = 16;
				currentRow = 3;

				worksheet.Row(currentRow).Style.Font.Bold = true;
				worksheet.Cells[currentRow, 1, currentRow, 3].Merge = true;
				worksheet.Cells[currentRow, 1, currentRow, 3].Value = string.Format("Отчетный период с {0} по {1}", datestart.ToString("dd.MM.yyyy"), dateto.ToString("dd.MM.yyyy"));
				currentRow++;

				currentRow++;
				currentRow++;
				#endregion

				#region Шапка таблицы
				rowHeaderStart = currentRow;
				rowHeaderEnd = currentRow;
				worksheet.Row(currentRow).Height = 38;
				worksheet.Row(currentRow).Style.Font.Bold = true;
				worksheet.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				worksheet.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				worksheet.Row(currentRow).Style.WrapText = true;

				int col = 1;
				var area = worksheet.Cells[currentRow, col, currentRow, col];
				area.Merge = true;
				area.Value = "Подразделение";
				worksheet.Column(col).Width = 40;
				col += 1;

				area = worksheet.Cells[currentRow, col, currentRow, col];
				area.Merge = true;
				area.Value = "Сотрудник";
				worksheet.Column(col).Width = 42;
				col += 1;

				area = worksheet.Cells[currentRow, col, currentRow, col];
				area.Style.WrapText = true;
				area.Value = "Макс.опоздание (мин)";
				worksheet.Column(col).Width = 17;
				col += 1;

				area = worksheet.Cells[currentRow, col, currentRow, col];
				area.Style.WrapText = true;
				area.Value = "Дней с опозданиями за период";
				worksheet.Column(col).Width = 17;
				col += 1;

				area = worksheet.Cells[currentRow, col, currentRow, col];
				area.Style.WrapText = true;
				area.Value = "Проверки";
				worksheet.Column(col).Width = 40;
				col += 1;

				area = worksheet.Cells[currentRow, col, currentRow, col];
				area.Style.WrapText = true;
				area.Value = "Объяснительные";
				worksheet.Column(col).Width = 40;
				col += 1;

				totalColumns = col - 1;
				currentRow++;

				//// Номера столбцов
				//for (var c = 1; c <= totalColumns; c++)
				//{
				//    worksheet.Cells[currentRow, c].Value = c;
				//}
				//currentRow++;
				#endregion

				foreach (var d in depts)
				{
					var dEmps = employees.Where(e => e.DepartmentId == d.ID);
					foreach (var e in dEmps)
					{
						worksheet.Cells[currentRow, 1].Value = e.DepartmentName;
						worksheet.Cells[currentRow, 2].Value = e.Name;
						worksheet.Cells[currentRow, 3].Value = e.MaxTimePeriod;
						worksheet.Cells[currentRow, 4].Value = e.LateDays;
						worksheet.Cells[currentRow, 5].Value = e.ChecksText;
						worksheet.Cells[currentRow, 5].Style.WrapText = true;
						worksheet.Cells[currentRow, 6].Value = e.ChecksInfoText;
						worksheet.Cells[currentRow, 6].Style.WrapText = true;

						worksheet.Row(currentRow).OutlineLevel = d.Level;
						col += 1;
						currentRow++;
					}
				}

				var borderStyleTop = worksheet.Cells[rowHeaderStart, 1, rowHeaderEnd, totalColumns].Style.Border;
				borderStyleTop.Bottom.Style = ExcelBorderStyle.Thin;
				borderStyleTop.Top.Style = ExcelBorderStyle.Thin;
				borderStyleTop.Left.Style = ExcelBorderStyle.Thin;
				borderStyleTop.Right.Style = ExcelBorderStyle.Thin;
				if (currentRow > rowHeaderEnd+1)
				{
					var borderStyleData = worksheet.Cells[rowHeaderEnd + 1, 1, currentRow - 1, totalColumns].Style.Border;
					borderStyleData.Left.Style = ExcelBorderStyle.Thin;
					borderStyleData.Top.Style = ExcelBorderStyle.Thin;
					borderStyleData.Right.Style = ExcelBorderStyle.Thin;

					worksheet.Cells[currentRow - 1, 1, currentRow - 1, totalColumns].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

					//worksheet.Cells[rowHeaderEnd + 1, 1, currentRow - 1, 1].Style.Font.Size = 6;
					//worksheet.Cells[rowHeaderEnd + 1, 1, currentRow - 1, 1].Style.Font.Color.SetColor(Color.Gray);
					worksheet.Cells[rowHeaderEnd + 1, 1, currentRow - 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


					worksheet.Cells[rowHeaderEnd + 1, 3, currentRow - 1, 3].Style.WrapText = true;
				}

				#region настройки печати, колонтитулы и другие настройки документа
				// Повторять при печати строки заголовка на каждой странице - сквозные строки
				worksheet.PrinterSettings.RepeatRows = new ExcelAddress(String.Format("'{2}'!${0}:${1}", rowHeaderStart, rowHeaderEnd, worksheet.Name));
				// Область печати
				worksheet.PrinterSettings.PrintArea = worksheet.Cells[1, 1, currentRow - 1, totalColumns];
				// Ландшафтная ориентация
				worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
				// Узкие поля
				worksheet.PrinterSettings.RightMargin = 0.2M;
				worksheet.PrinterSettings.LeftMargin = 0.2M;
				worksheet.PrinterSettings.TopMargin = 0.5M;

				worksheet.PrinterSettings.HorizontalCentered = true;
				worksheet.PrinterSettings.BottomMargin = 0.5M;
				// Печать всех столбцов на одной странице
				worksheet.PrinterSettings.FitToPage = true;
				worksheet.PrinterSettings.FitToWidth = 1;
				worksheet.PrinterSettings.FitToHeight = 0;

				// колонтитул - "КРОСС" в левом верхнем углу, логотип в колонтитул, внизу номер страницы из ...
				worksheet.HeaderFooter.differentOddEven = false;
				//worksheet.HeaderFooter.OddHeader.LeftAlignedText = "КРОСС";
				worksheet.HeaderFooter.OddFooter.RightAlignedText = string.Format("Страница {0} из {1}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);
				worksheet.HeaderFooter.AlignWithMargins = true;
				//worksheet.HeaderFooter.OddHeader.InsertPicture(new FileInfo(pathToImages + "kross-logo-for-excel-h30.png"), PictureAlignment.Left);

				// зафиксировать столбцы до индикатора
				//worksheet.View.FreezePanes(rowHeaderEnd + 1, columnIndicator + 1);
				#endregion

				// Форматный вывод цифр	
				if (currentRow > 10)
				{
					//worksheet.Cells[10, 1, currentRow, totalColumns].Style.Numberformat.Format = Params.PriceFormat;
				}

				package.Save();
			}
			return path;
		}
	}
}
