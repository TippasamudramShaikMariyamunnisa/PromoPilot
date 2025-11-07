import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CampaignTableComponent } from './campaign-table';
import { MarketingDashboardService } from './marketing-dashboard.service';
import { ToastrService } from 'ngx-toastr';
import { of, throwError } from 'rxjs';
import { CampaignEditModalComponent } from './campaign-edit-modal';

describe('CampaignTableComponent', () => {
  let component: CampaignTableComponent;
  let fixture: ComponentFixture<CampaignTableComponent>;
  let mockService: jasmine.SpyObj<MarketingDashboardService>;
  let mockToastr: jasmine.SpyObj<ToastrService>;

  beforeEach(() => {
    mockService = jasmine.createSpyObj('MarketingDashboardService', [
      'getCampaigns',
      'getCampaignById',
      'updateCampaign',
      'cancelCampaign'
    ]);
    mockToastr = jasmine.createSpyObj('ToastrService', ['success', 'error', 'info', 'warning']);

    TestBed.configureTestingModule({
      imports: [CampaignTableComponent, CampaignEditModalComponent],
      providers: [
        { provide: MarketingDashboardService, useValue: mockService },
        { provide: ToastrService, useValue: mockToastr }
      ]
    });

    fixture = TestBed.createComponent(CampaignTableComponent);
    component = fixture.componentInstance;
  });

  it('should handle fetch error', () => {
    mockService.getCampaigns.and.returnValue(throwError(() => ({ status: 500 })));
    fixture.detectChanges();
    expect(mockToastr.error).toHaveBeenCalledWith('Error fetching campaigns: 500', 'Fetch Failed');
  });

  it('should toggle sort direction', () => {
    spyOn(component, 'fetchCampaigns');
    component.sortBy = 'StartDate';
    component.sortDesc = false;
    component.toggleSort();
    expect(component.sortDesc).toBeTrue();
    expect(component.fetchCampaigns).toHaveBeenCalled();
  });

  it('should open edit modal', () => {
    const campaign = { campaignId: 1, name: 'Test' } as any;
    component.openEditModal(campaign);
    expect(component.selectedCampaign).toEqual(campaign);
  });

  it('should close modal', () => {
    component.selectedCampaign = { campaignId: 1 } as any;
    component.closeModal();
    expect(component.selectedCampaign).toBeNull();
  });


  it('should handle update error', () => {
    const updated = { campaignId: 1 } as any;
    mockService.updateCampaign.and.returnValue(throwError(() => ({ error: { detail: 'fail' } })));
    component.handleUpdate(updated);
    expect(mockToastr.error).toHaveBeenCalledWith('Failed to update: fail', 'Update Failed');
  });

  it('should handle view error', () => {
    mockService.getCampaignById.and.returnValue(throwError(() => ({ error: { detail: 'fail' } })));
    component.viewCampaign(1);
    expect(mockToastr.error).toHaveBeenCalledWith('Failed to fetch campaign: fail', 'Fetch Failed');
  });

  it('should not cancel if user declines', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.cancel(1);
    expect(mockService.cancelCampaign).not.toHaveBeenCalled();
  });

  it('should handle cancel error', fakeAsync(() => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockService.cancelCampaign.and.returnValue(throwError(() => ({ error: { detail: 'fail' } })));
    component.cancel(1);
    tick();
    expect(mockToastr.error).toHaveBeenCalledWith('Failed to cancel: fail', 'Cancel Failed');
  }));

  it('should go to next page if hasMorePages is true', () => {
    spyOn(component, 'fetchCampaigns');
    component.hasMorePages = true;
    component.page = 1;
    component.nextPage();
    expect(component.page).toBe(2);
    expect(component.fetchCampaigns).toHaveBeenCalled();
  });

  it('should not go to next page if hasMorePages is false', () => {
    spyOn(component, 'fetchCampaigns');
    component.hasMorePages = false;
    component.page = 1;
    component.nextPage();
    expect(component.page).toBe(1);
    expect(component.fetchCampaigns).not.toHaveBeenCalled();
  });

  it('should go to previous page if page > 1', () => {
    spyOn(component, 'fetchCampaigns');
    component.page = 2;
    component.prevPage();
    expect(component.page).toBe(1);
    expect(component.fetchCampaigns).toHaveBeenCalled();
  });

  it('should not go to previous page if page is 1', () => {
    spyOn(component, 'fetchCampaigns');
    component.page = 1;
    component.prevPage();
    expect(component.page).toBe(1);
    expect(component.fetchCampaigns).not.toHaveBeenCalled();
  });
});
