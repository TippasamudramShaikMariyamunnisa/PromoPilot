using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.Application.Services
{

    public class ExcelExporter : IExcelExporter
    {
        public byte[] ExportBudgetsToExcel(IEnumerable<BudgetDto> budgets)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Budgets");

            worksheet.Cell(1, 1).Value = "BudgetID";
            worksheet.Cell(1, 2).Value = "CampaignID";
            worksheet.Cell(1, 3).Value = "StoreID";
            worksheet.Cell(1, 4).Value = "AllocatedAmount";
            worksheet.Cell(1, 5).Value = "SpentAmount";

            var row = 2;
            foreach (var budget in budgets)
            {
                worksheet.Cell(row, 1).Value = budget.BudgetID;
                worksheet.Cell(row, 2).Value = budget.CampaignID;
                worksheet.Cell(row, 3).Value = budget.StoreID;
                worksheet.Cell(row, 4).Value = budget.AllocatedAmount;
                worksheet.Cell(row, 5).Value = budget.SpentAmount;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
