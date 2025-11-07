import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExecutionStatusDto } from './execution-status.model';
import { StoreManagerService } from './store-manager.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-execution-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './execution-table.html',
  styleUrls: ['./execution-table.css']
})
export class ExecutionTableComponent implements OnInit {
  @Output() statusSelected = new EventEmitter<ExecutionStatusDto>();

  statuses: ExecutionStatusDto[] = [];
  page = 1;
  pageSize = 4;
  sortBy = 'StatusId';
  sortDesc = false;
  totalCount = 0;
  loading = false;

  constructor(
    private service: StoreManagerService,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.fetchStatuses();
  }

  fetchStatuses() {
    this.loading = true;
    this.service.getPagedExecutionStatuses(this.page, this.pageSize, this.sortBy, this.sortDesc).subscribe({
      next: (res) => {
        this.statuses = res.items || [];
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error('Failed to fetch statuses: ' + (err.error?.detail || err.message), 'Fetch Error');
        this.loading = false;
      }
    });
  }

  toggleSort(field: string) {
    if (this.sortBy === field) {
      this.sortDesc = !this.sortDesc;
    } else {
      this.sortBy = field;
      this.sortDesc = false;
    }
    this.fetchStatuses();
  }

  view(id: number) {
    this.service.getExecutionStatusById(id).subscribe({
      next: (status) => {
        this.toastr.info(
          `Status ID: ${status.statusID}<br>
           CampaignID: ${status.campaignID}<br>
           StoreID: ${status.storeID}<br>
           Status: ${status.status}<br>
           Feedback: ${status.feedback || '—'}`,
          'Execution Status Details',
          { timeOut: 6000, enableHtml: true }
        );
      },
      error: (err) => {
        this.toastr.error('Failed to fetch status: ' + (err.error?.detail || err.message), 'View Error');
      }
    });
  }

  edit(status: ExecutionStatusDto) {
    this.statusSelected.emit(status);
  }

  delete(id: number) {
    if (!confirm('Are you sure you want to delete this status?')) return;

    this.service.deleteExecutionStatus(id).subscribe({
      next: () => {
        this.toastr.warning('Execution status deleted.', 'Deleted');
        this.fetchStatuses();
      },
      error: (err) => {
        this.toastr.error('Failed to delete status: ' + (err.error?.detail || err.message), 'Delete Error');
      }
    });
  }

  refresh() {
  this.fetchStatuses();
}


  nextPage() {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.fetchStatuses();
    }
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.fetchStatuses();
    }
  }
}
