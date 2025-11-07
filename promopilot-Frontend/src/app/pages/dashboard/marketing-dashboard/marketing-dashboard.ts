import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CampaignFormComponent } from '../../dashboard/marketing-dashboard/campaign-form';
import { CampaignTableComponent } from '../../dashboard/marketing-dashboard/campaign-table';
import { MarketingDashboardService } from '../../dashboard/marketing-dashboard/marketing-dashboard.service';
import { HttpClientModule } from '@angular/common/http';
import { AuthService } from '../../../services/AuthService';
import { Router } from '@angular/router';
import { EngagementTrackerComponent } from './engagement-tracker';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-marketing-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    CampaignFormComponent,
    CampaignTableComponent,
    EngagementTrackerComponent
  ],
  templateUrl: './marketing-dashboard.html',
  styleUrls: ['./marketing-dashboard.css']
})
export class MarketingDashboardComponent {
  activeView: 'form' | 'table' = 'form';
  showForm = false;
  showTable = true;
  isUploading = false;

  lastDownloadedBlob: Blob | null = null;
  lastDownloadedFilename: string = '';

  constructor(
    private service: MarketingDashboardService,
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  toggleView(view: string) {
    this.showForm = view === 'form';
    this.showTable = view === 'table';
  }

  onCampaignCreated() {
    this.toggleView('table');
  }

  downloadPdf() {
    this.service.exportCampaignsAsPdf().subscribe({
      next: (blob: Blob) => {
        const filename = 'PromoPilot_Campaigns.pdf';
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        a.click();
        window.URL.revokeObjectURL(url);

        this.lastDownloadedBlob = blob;
        this.lastDownloadedFilename = filename;

        this.toastr.success('Campaigns PDF downloaded!', 'Download Complete');
      },
      error: () => {
        this.toastr.error('Failed to download campaigns PDF.', 'Download Error');
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

    this.service.uploadMarketingReport(formData.get('file') as File).subscribe({
      next: () => {
        this.toastr.success('📤 Report uploaded successfully. It will be sent to other teams via email shortly.');
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
