import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
/* The AuthGaurd will automatically susbcribe to all observables. We will use it to observe the AuthService observable */
export class AuthGuard implements CanActivate {
  // we still do need to inject the AccountService
  constructor(private accountService: AccountService, private toastr: ToastrService) {}
  canActivate(): Observable<boolean> {
    // currentUser$ would return a user, so we need to use a map
    return this.accountService.currentUser$.pipe(
      map(user => {
        if (user) return true;
        this.toastr.error('You shall not pass!');
      })
    ) 
  }
  
}
