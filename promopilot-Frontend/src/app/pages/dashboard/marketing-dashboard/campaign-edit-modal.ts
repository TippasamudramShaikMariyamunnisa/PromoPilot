import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CampaignDto } from './campaign.model';

@Component({
  selector: 'app-campaign-edit-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './campaign-edit-modal.html',
  styleUrls: ['./campaign-edit-modal.css']
})
export class CampaignEditModalComponent {
  @Input() campaign!: CampaignDto;
  @Output() updated = new EventEmitter<CampaignDto>();
  @Output() closed = new EventEmitter<void>();

  todayIso: string = new Date().toISOString().split('T')[0];

  get isNameInvalid(): boolean {
    return !(this.campaign?.name?.trim()?.length);
  }

  get isStartDateInvalid(): boolean {
    return !(this.campaign?.startDate) || this.campaign.startDate < this.todayIso;
  }

  get isEndDateInvalid(): boolean {
    return !(this.campaign?.endDate) || this.campaign.endDate <= this.campaign.startDate;
  }

  get isProductsInvalid(): boolean {
    return !(this.campaign?.targetProducts?.trim()?.length);
  }

  get isStoresInvalid(): boolean {
    return !(this.campaign?.storeList?.trim()?.length);
  }

  get isValid(): boolean {
    return !this.isNameInvalid &&
           !this.isStartDateInvalid &&
           !this.isEndDateInvalid &&
           !this.isProductsInvalid &&
           !this.isStoresInvalid;
  }

  save() {
    if (this.isValid) {
      this.updated.emit(this.campaign);
    }
  }

  cancel() {
    this.closed.emit();
  }
}
