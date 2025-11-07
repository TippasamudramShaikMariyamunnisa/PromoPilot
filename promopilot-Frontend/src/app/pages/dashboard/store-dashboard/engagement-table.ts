import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EngagementDto } from './engagement.model';
import { StoreManagerService } from './store-manager.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-engagement-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './engagement-table.html',
  styleUrls: ['./engagement-table.css']
})
export class EngagementTableComponent implements OnInit {
  @Output() engagementSelected = new EventEmitter<EngagementDto>();

  engagements: EngagementDto[] = [];
  page = 1;
  pageSize = 4;
  sortBy = 'CampaignId';
  sortDesc = false;
  totalCount = 0;
  loading = false;

  constructor(
    private service: StoreManagerService,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.fetchEngagements();
  }

  fetchEngagements() {
    this.loading = true;
    this.service.getPagedEngagements(this.page, this.pageSize, this.sortBy, this.sortDesc).subscribe({
      next: (res) => {
        this.engagements = res.items || [];
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error('Failed to fetch engagements: ' + (err.error?.detail || err.message), 'Fetch Error');
        this.loading = false;
      }
    });
  }

  toggleSort(field: string) {
  this.sortBy = field;
  this.sortDesc = this.sortBy === field ? !this.sortDesc : false;
  this.fetchEngagements();
}


  view(id: number) {
    this.service.getEngagementById(id).subscribe({
      next: (engagement) => {
        this.toastr.info(
           `CampaignID: ${engagement.campaignID}<br>
           CustomerID: ${engagement.customerID}<br>
           RedemptionCount: ${engagement.redemptionCount}<br>
           PurchaseValue: ₹${engagement.purchaseValue}`,
          'Engagement Details',
          { timeOut: 6000, enableHtml: true }
        );
      },
      error: (err) => {
        this.toastr.error('Failed to fetch engagement: ' + (err.error?.detail || err.message), 'View Error');
      }
    });
  }

edit(engagement: EngagementDto) {
  console.log('Edit clicked:', engagement);
  this.engagementSelected.emit(engagement);
}



  delete(id: number) {
    if (!confirm('Are you sure you want to delete this engagement?')) return;

    this.service.deleteEngagement(id).subscribe({
      next: () => {
        this.toastr.warning('Engagement deleted.', 'Deleted');
        this.fetchEngagements();
      },
      error: (err) => {
        this.toastr.error('Failed to delete engagement: ' + (err.error?.detail || err.message), 'Delete Error');
      }
    });
  }

  refresh() {
  this.fetchEngagements();
}

  nextPage() {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.fetchEngagements();
    }
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.fetchEngagements();
    }
  }
}
