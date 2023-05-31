import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, identity } from 'rxjs';
import { BusyService } from '../_services/busy.service';
import { delay, finalize } from 'rxjs/operators';
import { environment } from 'src/environments/environment';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.busyService.busy();  // this should turn on the busy spinner
    return next.handle(request).pipe(   // add a pipe so that we can do some clean up after the request is completed
      
      // don't do the delay in production, identity is used to return an object that does nothing
      (environment.production ? identity : delay(1000)), // temp to delay are system because it is unrealistically fast
      finalize(() => {
        this.busyService.idle(); // make the spinner go away
      })
    )
  }
}
