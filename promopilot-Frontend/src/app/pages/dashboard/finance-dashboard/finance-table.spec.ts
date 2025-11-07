import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { FinanceTableComponent } from './finance-table';
import { FinanceDashboardService } from './finance-dashboard.service';
import { ToastrService } from 'ngx-toastr';
import { of, throwError } from 'rxjs';
import { By } from '@angular/platform-browser';

describe('FinanceTableComponent', () => {
  let component: FinanceTableComponent;
  let fixture: ComponentFixture<FinanceTableComponent>;
  let mockService: jasmine.SpyObj<FinanceDashboardService>;
  let mockToastr: jasmine.SpyObj<ToastrService>;

  beforeEach(() => {
    mockService = jasmine.createSpyObj('FinanceDashboardService', ['getPagedBudgets', 'deleteBudget', 'exportBudgets']);
    mockToastr = jasmine.createSpyObj('ToastrService', ['success', 'error']);

    TestBed.configureTestingModule({
      imports: [FinanceTableComponent],
      providers: [
        { provide: FinanceDashboardService, useValue: mockService },
        { provide: ToastrService, useValue: mockToastr }
      ]
    });

    fixture = TestBed.createComponent(FinanceTableComponent);
    component = fixture.componentInstance;
  });

  it('should fetch budgets on init', () => {
    const mockResponse = { items: [{ budgetID: 1 }], totalCount: 1 };
    mockService.getPagedBudgets.and.returnValue(of(mockResponse));

    fixture.detectChanges();

    expect(component.budgets.length).toBe(1);
    expect(component.totalCount).toBe(1);
    expect(mockService.getPagedBudgets).toHaveBeenCalled();
  });

  it('should handle fetch error', () => {
    mockService.getPagedBudgets.and.returnValue(throwError(() => ({
      error: { detail: 'Server error' }
    })));

    fixture.detectChanges();

    expect(mockToastr.error).toHaveBeenCalledWith('Failed to fetch budgets: Server error', 'Error');
    expect(component.loading).toBeFalse();
  });

  it('should emit budget on edit', () => {
    spyOn(component.budgetSelected, 'emit');
    const budget = { budgetID: 1 };
    component.editBudget(budget);
    expect(component.budgetSelected.emit).toHaveBeenCalledWith(budget);
  });


  it('should handle delete error', fakeAsync(() => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockService.deleteBudget.and.returnValue(throwError(() => ({
      error: { detail: 'Delete failed' }
    })));

    component.deleteBudget(1);
    tick();

    expect(mockToastr.error).toHaveBeenCalledWith('Failed to delete budget: Delete failed', 'Delete Error');
  }));

  it('should export budgets as CSV', fakeAsync(() => {
    const blob = new Blob(['test'], { type: 'text/csv' });
    mockService.exportBudgets.and.returnValue(of(blob));

    spyOn(document, 'createElement').and.callFake(() => {
      return {
        href: '',
        download: '',
        click: jasmine.createSpy('click')
      } as any;
    });

    component.export('csv');
    tick();

    expect(mockToastr.success).toHaveBeenCalledWith('Budgets exported as CSV', 'Export Successful');
  }));

  it('should handle export error', fakeAsync(() => {
    mockService.exportBudgets.and.returnValue(throwError(() => ({
      error: { detail: 'Export failed' }
    })));

    component.export('excel');
    tick();

    expect(mockToastr.error).toHaveBeenCalledWith('Export failed: Export failed', 'Export Error');
  }));

  it('should navigate to next page', () => {
    component.totalCount = 10;
    component.pageSize = 4;
    component.page = 1;
    spyOn(component, 'fetchBudgets');

    component.nextPage();
    expect(component.page).toBe(2);
    expect(component.fetchBudgets).toHaveBeenCalled();
  });

  it('should not navigate to next page if at end', () => {
    component.totalCount = 8;
    component.pageSize = 4;
    component.page = 2;
    spyOn(component, 'fetchBudgets');

    component.nextPage();
    expect(component.page).toBe(2);
    expect(component.fetchBudgets).not.toHaveBeenCalled();
  });

  it('should navigate to previous page', () => {
    component.page = 2;
    spyOn(component, 'fetchBudgets');

    component.prevPage();
    expect(component.page).toBe(1);
    expect(component.fetchBudgets).toHaveBeenCalled();
  });

  it('should not navigate to previous page if on first', () => {
    component.page = 1;
    spyOn(component, 'fetchBudgets');

    component.prevPage();
    expect(component.page).toBe(1);
    expect(component.fetchBudgets).not.toHaveBeenCalled();
  });
});
