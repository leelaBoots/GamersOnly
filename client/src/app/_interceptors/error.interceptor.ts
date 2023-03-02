import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';

// this class will allow us to do some error handling on the front-end and display toastr messages etc.
// this will handle all types of http response errors in one place

// Injectable status
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  // we also need to provide this interceptor in our app module

  constructor(private router: Router, private toastr: ToastrService) {}

  // we can intercept the request, or the response (next)
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(error => {
        if (error) {
          switch (error.status) {
            case 400:
              // this looks weird, but it matches the structure of the object retured in the response 
              if (error.error.errors) {
                // validation errors are known as modalStateErrors in ASP.net
                const modalStateErrors = [];

                // we do this to flatten the array of errors[] we received in the response and push them into an array variable
                for (const key in error.error.errors) {
                  if(error.error.errors[key]) {
                    modalStateErrors.push(error.error.errors[key])
                  }
                }
                throw modalStateErrors.flat();
              } else {
                // this handles a normal 400 response
                this.toastr.error(error.statusText, error.status);
              }
              break;
            case 401:
              this.toastr.error(error.statusText, error.status);
              break;
            case 404:
              this.router.navigateByUrl('/not-found');
              break;
            case 500:
              // use a feature of the router to pass it some states
              const navigationExtras: NavigationExtras = {state: {error: error.error}}
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              this.toastr.error('Something unexpected occurred.');
              console.log(error);
              break;
          }
        }
        // if we dont catch it, we return the error to who ever called it. we should never hit this
        return throwError(error);
      })
    )
  }
}
