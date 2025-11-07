import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FinanceDashboardService } from './finance-dashboard.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-finance-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './finance-table.html',
  styleUrls: ['./finance-table.css']
})
export class FinanceTableComponent implements OnInit {
  budgets: any[] = [];
  loading = false;
  page = 1;
  pageSize = 4;
  sortBy = 'BudgetId';
  sortDesc = false;
  totalCount = 0;

  @Output() budgetSelected = new EventEmitter<any>();

  constructor(private service: FinanceDashboardService, private toastr: ToastrService) {}

  ngOnInit() {
    this.fetchBudgets();
  }

  fetchBudgets() {
    this.loading = true;
    this.service.getPagedBudgets(this.page, this.pageSize, this.sortBy, this.sortDesc).subscribe({
      next: (res: { items: any[]; totalCount: number }) => {
        this.budgets = res.items;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err: { error: { detail: any }; message: any }) => {
        this.toastr.error('Failed to fetch budgets: ' + (err.error?.detail || err.message), 'Error');
        this.loading = false;
      }
    });
  }
  

editBudget(budget: any) {
    this.budgetSelected.emit(budget);
  }

  deleteBudget(id: number) {
    if (confirm('Are you sure you want to delete this budget?')) {
      this.service.deleteBudget(id).subscribe({
        next: () => {
          this.toastr.success('Budget deleted successfully!', 'Deleted');
          this.fetchBudgets();
        },
        error: (err: { error: { detail: any }; message: any }) => {
          this.toastr.error('Failed to delete budget: ' + (err.error?.detail || err.message), 'Delete Error');
        }
      });
    }
  }


  export(format: 'csv' | 'excel') {
    this.service.exportBudgets(format).subscribe({
      next: (blob: Blob | MediaSource) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `budgets.${format === 'csv' ? 'csv' : 'xlsx'}`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.toastr.success(`Budgets exported as ${format.toUpperCase()}`, 'Export Successful');
      },
      error: (err: { error: { detail: any }; message: any }) => {
        this.toastr.error('Export failed: ' + (err.error?.detail || err.message), 'Export Error');
      }
    });
  }

  
    nextPage() {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.fetchBudgets();
    }
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.fetchBudgets();
    }
  }

}
