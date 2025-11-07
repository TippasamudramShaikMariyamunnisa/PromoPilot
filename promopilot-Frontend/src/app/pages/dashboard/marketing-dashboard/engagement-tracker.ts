import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { EngagementService } from '../../../services/engagement.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-engagement-tracker',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  providers: [EngagementService],
  templateUrl: './engagement-tracker.html',
  styleUrls: ['./engagement-tracker.css']
})
export class EngagementTrackerComponent {
  engagements: any[] = [];
  isLoading = false;
  errorMessage = '';

  pageNumber = 1;
  pageSize = 10;
  sortBy = 'campaignID';
  sortDesc = false;
  totalCount = 0;

  constructor(
    private engagementService: EngagementService,
    private toastr: ToastrService
  ) {}

  export(format: 'csv' | 'excel') {
    this.engagementService.exportEngagements(format).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `engagements.${format === 'csv' ? 'csv' : 'xlsx'}`;
        a.click();
        window.URL.revokeObjectURL(url);
        this.toastr.success(`Engagements exported as ${format.toUpperCase()}`, 'Export Successful');
      },
      error: (err) => {
        this.toastr.error('Export failed: ' + (err.error?.detail || err.message), 'Export Error');
      }
    });
  }
}
