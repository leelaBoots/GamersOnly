import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, filter, map } from 'rxjs/operators';

// this class will allow us to do some error handling on the front-end and display toastr messages etc.
// this will handle all types of http response errors in one place

// Injectable status
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  err: string;
  // we also need to provide this interceptor in our app module

  constructor(private router: Router, private toastr: ToastrService) {}

  // we can intercept the request, or the response (next)
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // pipe is needed to transform or modify an observable 
    return next.handle(request).pipe(
      
      catchError((error: HttpErrorResponse) => {
        if (error.error instanceof Error) {
          this.toastr.error(error.error.message);
        } else if (error) {
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
                if (Array.isArray(error.error)) {
                  // this handles a normal 400 response
                  if(error.error[0].description) {
                    // if there is a description in the json, just print that
                    this.toastr.error(error.error[0].description);
                  } else {
                    // otherwise just stringify the whole JSON object, since we don't know whats in there
                    this.toastr.error(JSON.stringify(error.error[0]));
                  }
                } else {
                  this.toastr.error(error.error);
                }
              }
              break;
            case 401:
              if (error.error) {
                this.toastr.error(error.error);
              } else {
                this.toastr.error(error.statusText, error.status.toString());
              }
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
