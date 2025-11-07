import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EngagementFormComponent } from './engagement-form';
import { EngagementTableComponent } from './engagement-table';
import { ExecutionFormComponent } from './execution-form';
import { ExecutionTableComponent } from './execution-table';
import { StoreManagerService } from './store-manager.service';
import { ToastrService } from 'ngx-toastr';
import { EngagementDto } from './engagement.model';
import { ExecutionStatusDto } from './execution-status.model';
import { AuthService } from '../../../services/AuthService';
import { Router } from '@angular/router';
import { ElementRef } from '@angular/core';

@Component({
  selector: 'app-store-manager-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    EngagementFormComponent,
    EngagementTableComponent,
    ExecutionFormComponent,
    ExecutionTableComponent
  ],
  templateUrl: './store-dashboard.html',
  styleUrls: ['./store-dashboard.css']
})
export class StoreDashboardComponent {
  activeView: 'engagementForm' | 'engagementTable' | 'executionForm' | 'executionTable' = 'engagementForm';

  @ViewChild(EngagementFormComponent) engagementForm?: EngagementFormComponent;
  @ViewChild(EngagementTableComponent) engagementTable?: EngagementTableComponent;
  @ViewChild(ExecutionFormComponent) executionForm?: ExecutionFormComponent;
  @ViewChild(ExecutionTableComponent) executionTable?: ExecutionTableComponent;
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  constructor(
    private service: StoreManagerService,
    private toastr: ToastrService,
    private auth: AuthService,
    private router: Router
  ) {}

editEngagement: EngagementDto | null = null;
editStatus: ExecutionStatusDto | null = null;
isUploading = false;
selectedStore: any = null;
lastDownloadedBlob: Blob | null = null;
lastDownloadedFilename: string = '';

startEditingEngagement(dto: EngagementDto) {
  this.editEngagement = dto;
  this.activeView = 'engagementForm';
}

startEditingStatus(dto: ExecutionStatusDto) {
  this.editStatus = dto;
  this.activeView = 'executionForm';
}

handleEngagementTracked(dto: EngagementDto) {
  this.service.trackEngagement(dto).subscribe({
    next: () => {
      this.toastr.success('Engagement recorded successfully!', 'Success');
      this.engagementForm?.reset();
      this.engagementTable?.refresh();
      this.activeView = 'engagementTable';
    },
    error: (err) => {
      this.toastr.error('Failed to track engagement: ' + (err.error?.detail || err.message), 'Error');
    }
  });
}
handleEngagementUpdated(dto: EngagementDto) {
  this.service.updateEngagement(dto.engagementID, dto).subscribe({
    next: () => {
      this.toastr.success('Engagement updated successfully!', 'Updated');
      this.editEngagement = null;
      this.engagementForm?.reset();
      this.engagementTable?.refresh();
      this.activeView = 'engagementTable';
    },
    error: (err) => {
      this.toastr.error('Failed to update engagement: ' + (err.error?.detail || err.message), 'Error');
    }
  });
}


handleStatusAdded(dto: ExecutionStatusDto) {
  this.service.addExecutionStatus(dto).subscribe({
    next: () => {
      this.toastr.success('Execution status added successfully!', 'Success');
      this.executionForm?.reset();
      this.executionTable?.refresh();
      this.activeView = 'executionTable';
    },
    error: (err) => {
      this.toastr.error('Failed to add execution status: ' + (err.error?.detail || err.message), 'Error');
    }
  });
}
handleStatusUpdated(dto: ExecutionStatusDto) {
  this.service.updateExecutionStatus(dto.statusID, dto).subscribe({
    next: () => {
      this.toastr.success('Execution status updated successfully!', 'Updated');
      this.editStatus = null;
      this.executionForm?.reset();
      this.executionTable?.refresh();
      this.activeView = 'executionTable';
    },
    error: (err) => {
      this.toastr.error('Failed to update status: ' + (err.error?.detail || err.message), 'Error');
    }
  });
}

exportEngagements(format: 'csv' | 'excel') {
  this.service.exportEngagements(format).subscribe({
    next: (blob) => {
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = `engagements.${format === 'csv' ? 'csv' : 'xlsx'}`;
      link.click();
      this.toastr.success(`Engagements exported as ${format.toUpperCase()}`, 'Export Success');
    },
    error: (err) => {
      this.toastr.error('Export failed: ' + (err.error?.detail || err.message), 'Export Error');
    }
  });
}

downloadExcel() {
  this.service.exportEngagements('excel').subscribe({
    next: (blob: Blob) => {
      const filename = 'PromoPilot_StoreReport.xlsx';
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

  this.service.uploadStoreReport(formData.get('file') as File).subscribe({
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

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
    this.toastr.info('Logged out successfully.', 'Session Ended');
  }

}