import { Component, Output, EventEmitter, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EngagementDto } from './engagement.model';

@Component({
  selector: 'app-engagement-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './engagement-form.html',
  styleUrls: ['./engagement-form.css']
})
export class EngagementFormComponent implements OnChanges {
  @Input() editEngagement: EngagementDto | null = null;
  @Output() engagementTracked = new EventEmitter<EngagementDto>();
  @Output() engagementUpdated = new EventEmitter<EngagementDto>();

  engagement: EngagementDto = {
    engagementID: 0,
    campaignID: 0,
    customerID: 0,
    redemptionCount: 0,
    purchaseValue: 0
  };

  ngOnChanges(changes: SimpleChanges) {
    if (changes['editEngagement'] && this.editEngagement) {
      this.engagement = { ...this.editEngagement };
    }
  }

  get isValid(): boolean {
    return this.engagement.campaignID > 0 &&
           this.engagement.customerID > 0 &&
           this.engagement.redemptionCount >= 0 &&
           this.engagement.purchaseValue >= 0.01;
  }

  track() {
    if (this.isValid) {
      if (this.engagement.engagementID > 0) {
        this.engagementUpdated.emit(this.engagement);
      } else {
        this.engagementTracked.emit(this.engagement);
      }
    }
  }

  reset() {
    this.engagement = {
      engagementID: 0,
      campaignID: 0,
      customerID: 0,
      redemptionCount: 0,
      purchaseValue: 0
    };
  }
}
