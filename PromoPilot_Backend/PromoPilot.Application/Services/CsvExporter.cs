using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.Application.Services
{
    public class CsvExporter : ICsvExporter
    {
        public string ExportBudgetsToCsv(IEnumerable<BudgetDto> budgets)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BudgetID,CampaignID,StoreID,AllocatedAmount,SpentAmount");

            foreach (var budget in budgets)
            {
                sb.AppendLine($"{budget.BudgetID},{budget.CampaignID},{budget.StoreID},{budget.AllocatedAmount},{budget.SpentAmount}");
            }

            return sb.ToString();
        }
    }
}
