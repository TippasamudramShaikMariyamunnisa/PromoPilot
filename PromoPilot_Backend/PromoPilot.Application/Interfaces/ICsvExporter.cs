using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface ICsvExporter
    {
        string ExportBudgetsToCsv(IEnumerable<BudgetDto> budgets);
    }
}
