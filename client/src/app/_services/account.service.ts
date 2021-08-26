import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { longStackSupport } from 'q';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../_models/user';

/* This decorator means our service can be injected into components or other services in our application */
@Injectable({
  providedIn: 'root'
})
/* this will be used to make requests to our API */
export class AccountService {
  baseUrl = 'https://localhost:5001/api/';
  private currentUserSource = new ReplaySubject<User>(1); // 1 means we get the last 1 user, so this will be the size of the buffer
  currentUser$ = this.currentUserSource.asObservable(); // by convention, we add $ to end of observable name

  /* inject the httpClient into our account service.
    Services are singlestons, so the data persists until the application is closed.
    Components are destroyed as soon as they are not in use, or we move to another component */
  constructor(private http: HttpClient) { }

  /* login() is going to receive the login credentials from our navbar login form */
  // tslint:disable-next-line: typedef
  login(model: User) {
    /* because this is a post request, we are required to include a body, so we send our model as the body */
    /* anything in the pipe() is rxjs operator */
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      // since we are subscribing in our nav component, this map will run when we login, and populate user in local storage
      map((response: any) => {
        const user = response;
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user); // this is how you set the next value in the ReplaySubject
        }
      })
    )
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user); // this is how you set the next value in the ReplaySubject
        }
      })
    )
  }


  /* helper method */
  setCurrentUser(user: User) {
    this.currentUserSource.next(user); // this is how you set the next value in the ReplaySubject
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
