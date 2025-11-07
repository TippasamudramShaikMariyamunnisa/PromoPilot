import { Component, EventEmitter, Output } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors,
  ReactiveFormsModule
} from '@angular/forms';
import { MarketingDashboardService } from './marketing-dashboard.service';
import { CampaignDto } from '../../dashboard/marketing-dashboard/campaign.model';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-campaign-form',
  standalone: true,
  templateUrl: './campaign-form.html',
  styleUrls: ['./campaign-form.css'],
  imports: [CommonModule, ReactiveFormsModule]
})
export class CampaignFormComponent {
  form: FormGroup;
  submitted = false;
  loading = false;
  @Output() campaignCreated = new EventEmitter<CampaignDto>();

  constructor(
    private fb: FormBuilder,
    private service: MarketingDashboardService,
    private toastr: ToastrService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      startDate: ['', [Validators.required, this.startDateValidator]],
      endDate: ['', [Validators.required]],
      targetProducts: ['', [Validators.required, Validators.minLength(3)]],
      storeList: ['', [Validators.required, Validators.minLength(3)]]
    }, { validators: this.endDateAfterStartValidator });
  }

  startDateValidator(control: AbstractControl): ValidationErrors | null {
    const inputDate = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return inputDate >= today ? null : { startDatePast: true };
  }

  endDateAfterStartValidator(group: AbstractControl): ValidationErrors | null {
    const start = new Date(group.get('startDate')?.value);
    const end = new Date(group.get('endDate')?.value);
    return end > start ? null : { endBeforeStart: true };
  }

  onSubmit() {
    this.submitted = true;
    if (this.form.invalid) return;

    this.loading = true;
    const dto: CampaignDto = this.form.value;

    this.service.planCampaign(dto).subscribe({
      next: (res) => {
        this.toastr.success('✅ Campaign planned successfully!', 'Success');
        this.campaignCreated.emit(res);
        this.form.reset();
        this.submitted = false;
        this.loading = false;
      },
      error: (err) => {
        console.error('Campaign planning error:', err);
        const message =
          err.error?.detail ||
          err.error?.title ||
          err.message ||
          'Unexpected error.';
        this.toastr.error('❌ Failed to plan campaign: ' + message, 'Error');
        this.loading = false;
      }
    });
  }
}
