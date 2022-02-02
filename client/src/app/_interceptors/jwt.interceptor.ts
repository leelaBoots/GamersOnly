import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountService } from '../_services/account.service';
import { User } from '../_models/user';
import { take } from 'rxjs/operators';

/* purpose of this class is handle the token without having to pass it via our httpOptions with every request made */

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    let currentUser: User;

    // using pipe(take(1)) allows us to subscribe, get 1 user, and it will automatically unscubscribe when done, since we are unsure if we need to unsubscribe
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => currentUser = user);

    // clone the above request, and add our authentication header onto it
    if (currentUser) {
      request = request.clone({
        // dont forget the space between Bearer and token
        setHeaders: {
          // the tick marks is a js feature called template literals, it makes it easier to read when strings are made of multiple parts
          Authorization: `Bearer ${currentUser.token}`
        }
      })
    }

    return next.handle(request);
  }
}
