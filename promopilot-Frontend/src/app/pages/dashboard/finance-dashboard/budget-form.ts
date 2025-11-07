import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FinanceDashboardService } from './finance-dashboard.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-budget-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './budget-form.html',
  styleUrls: ['./budget-form.css']
})
export class BudgetFormComponent implements OnChanges {
  @Input() initialData: any;
  @Output() budgetCreated = new EventEmitter<any>();

  form: FormGroup;
  submitted = false;
  loading = false;

  constructor(private fb: FormBuilder, private service: FinanceDashboardService, private toastr: ToastrService) {
    this.form = this.fb.group({
      CampaignID: ['', Validators.required],
      StoreID: ['', Validators.required],
      AllocatedAmount: ['', [Validators.required, Validators.min(0.01)]],
      SpentAmount: ['', [Validators.required, Validators.min(0)]]
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['initialData'] && this.initialData) {
      this.form.patchValue({
        CampaignID: this.initialData.campaignID,
        StoreID: this.initialData.storeID,
        AllocatedAmount: this.initialData.allocatedAmount,
        SpentAmount: this.initialData.spentAmount
      });
    }
  }

  onSubmit() {
    this.submitted = true;
    if (this.form.invalid) return;

    this.loading = true;
    const payload = this.form.value;
    const isUpdate = !!this.initialData?.budgetID;

    const request = isUpdate
      ? this.service.updateBudget(this.initialData.budgetID, payload)
      : this.service.allocateBudget(payload);

    request.subscribe({
      next: (res: any) => {
        const msg = isUpdate ? 'Budget updated successfully!' : 'Budget allocated successfully!';
        this.toastr.success(msg, 'Success');
        this.budgetCreated.emit(res);
        this.form.reset();
        this.submitted = false;
        this.loading = false;
      },
      error: (err: { error: { detail: any; title: any }; message: any }) => {
        const msg = err.error?.detail || err.error?.title || err.message;
        this.toastr.error('Failed to save budget: ' + msg, 'Error');
        this.loading = false;
      }
    });
  }
}
