import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  // multiple requests at a time, we will increment and decrement as they come in
  busyRequestCount = 0;

  constructor(private spinnerService: NgxSpinnerService) { }

  busy(): void {
    this.busyRequestCount++;
    this.spinnerService.show(undefined, {
      type: 'pacman',
      bdColor: 'rgba(255, 255, 255, 0)',
      color: 'var(--bs-warning)'
    });
  }

  idle(): void {
    this.busyRequestCount--;
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide();
    }  
  }
}
