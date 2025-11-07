import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MarketingDashboardService } from './marketing-dashboard.service';
import { CampaignDto } from './campaign.model';
import { CampaignEditModalComponent } from './campaign-edit-modal';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-campaign-table',
  standalone: true,
  imports: [CommonModule, CampaignEditModalComponent],
  templateUrl: './campaign-table.html',
  styleUrls: ['./campaign-table.css']
})
export class CampaignTableComponent implements OnInit {
  campaigns: CampaignDto[] = [];
  selectedCampaign: CampaignDto | null = null;
  page = 1;
  pageSize = 3;
  sortBy: string = 'StartDate';
  sortDesc: boolean = false;
  loading = false;
  hasMorePages: boolean = true;

  constructor(
    private service: MarketingDashboardService,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.fetchCampaigns();
  }

  fetchCampaigns() {
    this.loading = true;
    this.service.getCampaigns(this.page, this.pageSize, this.sortBy, this.sortDesc).subscribe({
      next: (res) => {
        this.campaigns = res.items || [];
        this.hasMorePages = res.totalCount > this.page * this.pageSize;
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error('Error fetching campaigns: ' + (err.status || err.message), 'Fetch Failed');
        this.loading = false;
      }
    });
  }

  toggleSort() {
    if (this.sortBy === 'StartDate') {
      this.sortDesc = !this.sortDesc;
    } else {
      this.sortBy = 'StartDate';
      this.sortDesc = false;
    }
    this.fetchCampaigns();
  }

  openEditModal(campaign: CampaignDto) {
    this.selectedCampaign = { ...campaign };
  }

  handleUpdate(updated: CampaignDto) {
    this.service.updateCampaign(updated.campaignId, updated).subscribe({
      next: () => {
        this.toastr.success('Campaign updated!', 'Success');
        this.selectedCampaign = null;
        this.fetchCampaigns();
      },
      error: (err) => {
        this.toastr.error('Failed to update: ' + (err.error?.detail || err.message), 'Update Failed');
      }
    });
  }

  closeModal() {
    this.selectedCampaign = null;
  }

  viewCampaign(id: number) {
    this.service.getCampaignById(id).subscribe({
      next: (campaign) => {
        this.toastr.info(
          `CampaignsID: ${campaign.campaignId}\nCampaignName: ${campaign.name}\nStartDate: ${campaign.startDate}\nEndDate: ${campaign.endDate}\nProducts: ${campaign.targetProducts}\nStores: ${campaign.storeList}`,
          'Campaign Details',
          { timeOut: 6000, enableHtml: true }
        );
      },
      error: (err) => {
        this.toastr.error('Failed to fetch campaign: ' + (err.error?.detail || err.message), 'Fetch Failed');
      }
    });
  }

  cancel(id: number) {
    if (!confirm('Are you sure you want to cancel this campaign?')) return;

    this.service.cancelCampaign(id).subscribe({
      next: () => {
        this.toastr.warning('Campaign cancelled.', 'Cancelled');
        this.fetchCampaigns();
      },
      error: (err) => {
        this.toastr.error('Failed to cancel: ' + (err.error?.detail || err.message), 'Cancel Failed');
      }
    });
  }

  nextPage() {
    if (this.hasMorePages) {
      this.page++;
      this.fetchCampaigns();
    }
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.fetchCampaigns();
    }
  }
}
