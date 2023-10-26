import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { PresenceService } from './presence.service';

/* This decorator means our service can be injected into components or other services in our application */
@Injectable({
  providedIn: 'root'
})
/* this will be used to make requests to our API */
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1); // 1 means we get the last 1 user, so this will be the size of the buffer
  currentUser$ = this.currentUserSource.asObservable(); // by convention, we add $ to end of observable name

  /* inject the httpClient into our account service.
    Services are singlestons, so the data persists until the application is closed.
    Components are destroyed as soon as they are not in use, or we move to another component */
  
    // this is also a good place to establish the presenceService because we want to maintain the connection from the moment
    // we sign on.
  constructor(private http: HttpClient, private presenceService: PresenceService) { }

  /* login() is going to receive the login credentials from our navbar login form */
  // tslint:disable-next-line: typedef
  login(model: User) {

    // lets convert username to lowercase here before we send the request, so the username will not need to be case sensitive
    model.username = model.username.toLowerCase();
    
    /* because this is a post request, we are required to include a body, so we send our model as the body */
    /* anything in the pipe() is rxjs operator */
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      // since we are subscribing in our nav component, this map will run when we login, and populate user in local storage
      map((response: any) => {
        const user = response;
        if (user) {
          this.setCurrentUser(user);
        }
      })
    )
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    )
  }


  /* helper method */
  setCurrentUser(user: User) {
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role;
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user); // this is how you set the next value in the ReplaySubject

    // this is for our SignalR connection
    this.presenceService.createHubConnection(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presenceService.stopHubConnection();
  }

  getDecodedToken(token: string) {
    // splitting on '.' and getting 2nd element removes the algorithm info at begining of token, and the signature info at the end of the token.
    return JSON.parse(atob(token.split('.')[1]));
  }
}
