import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BudgetFormComponent } from '../../dashboard/finance-dashboard/budget-form';
import { FinanceTableComponent } from '../../dashboard/finance-dashboard/finance-table';
import { FinanceDashboardService } from '../../dashboard/finance-dashboard/finance-dashboard.service';
import { HttpClientModule } from '@angular/common/http';
import { AuthService } from '../../../services/AuthService';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-finance-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    BudgetFormComponent,
    FinanceTableComponent
  ],
  templateUrl: './finance-dashboard.html',
  styleUrls: ['./finance-dashboard.css']
})
export class FinanceDashboardComponent {
  activeView: 'form' | 'table' = 'form';
  showForm = false;
  showTable = true;
  selectedBudget: any = null;
  isUploading = false;
  lastDownloadedBlob: Blob | null = null;
  lastDownloadedFilename: string = '';


  @ViewChild('table') table!: FinanceTableComponent;

  constructor(
    private service: FinanceDashboardService,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  toggleView(view: string) {
    this.showForm = false;
    this.showTable = false;
    this.selectedBudget = null;

    switch (view) {
      case 'form':
        this.showForm = true;
        break;
      case 'table':
        this.showTable = true;
        break;
    }
  }

  onBudgetCreated() {
  this.toggleView('table');
  setTimeout(() => {
    this.table.page = 1; // optional: reset to first page
    this.table.fetchBudgets(); // refresh the table
  }, 0);
}


  onBudgetSelected(budget: any) {
    this.selectedBudget = budget;
    this.showForm = true;
    this.showTable = false;
  }


downloadExcel() {
  this.service.exportBudgets('excel').subscribe({
    next: (blob: Blob) => {
      const filename = 'PromoPilot_FinanceReport.xlsx';
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      a.click();
      window.URL.revokeObjectURL(url);

      this.lastDownloadedBlob = blob;
      this.lastDownloadedFilename = filename;

      this.toastr.success('Finance report downloaded!', 'Download Complete');
    },
    error: () => {
      this.toastr.error('Failed to download finance report.', 'Download Error');
    }
  });
}

notifyTeams() {
  if (!this.lastDownloadedBlob || !this.lastDownloadedFilename) {
    this.toastr.warning('Please download the report first.', 'No File Found');
    return;
  }

  this.isUploading = true;

  const formData = new FormData();
  formData.append('file', this.lastDownloadedBlob, this.lastDownloadedFilename);

  this.service.uploadFinanceReport(formData.get('file') as File).subscribe({
    next: () => {
      this.toastr.success(
        '📤 Report uploaded successfully. It will be sent to other teams via email shortly.'
      );
      this.isUploading = false;
    },
    error: () => {
      this.toastr.error('Upload failed. Please try again.', 'Upload Error');
      this.isUploading = false;
    }
  });
}

  onLogout() {
    this.auth.logout();
    this.router.navigate(['/login']);
    this.toastr.info('Logged out successfully.', 'Session Ended');
  }
}
