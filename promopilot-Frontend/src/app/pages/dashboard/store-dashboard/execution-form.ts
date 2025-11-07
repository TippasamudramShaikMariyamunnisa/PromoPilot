import { Component, Output, EventEmitter, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExecutionStatusDto } from './execution-status.model';

@Component({
  selector: 'app-execution-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './execution-form.html',
  styleUrls: ['./execution-form.css']
})
export class ExecutionFormComponent implements OnChanges {
  @Input() editStatus: ExecutionStatusDto | null = null;
  @Output() statusAdded = new EventEmitter<ExecutionStatusDto>();
  @Output() statusUpdated = new EventEmitter<ExecutionStatusDto>();

  status: ExecutionStatusDto = {
    statusID: 0,
    campaignID: 0,
    storeID: 0,
    status: 'Pending',
    feedback: ''
  };

  ngOnChanges(changes: SimpleChanges) {
    if (changes['editStatus'] && this.editStatus) {
      this.status = { ...this.editStatus };
    }
  }

  get isValid(): boolean {
    return this.status.campaignID > 0 &&
           this.status.storeID > 0 &&
           ['Pending', 'InProgress', 'Completed'].includes(this.status.status) &&
           (!this.status.feedback || this.status.feedback.length >= 10);
  }

  submit() {
    if (this.isValid) {
      if (this.status.statusID > 0) {
        this.statusUpdated.emit(this.status);
      } else {
        this.statusAdded.emit(this.status);
      }
    }
  }

  reset() {
    this.status = {
      statusID: 0,
      campaignID: 0,
      storeID: 0,
      status: 'Pending',
      feedback: ''
    };
  }
}
