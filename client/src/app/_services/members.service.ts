import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from 'src/app/_models/member';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

/* This is no longer needed, because we now attach authorization headers via jwt.interceptor.ts
const httpOptions = {
  headers: new HttpHeaders({
    // ? is the optional chaining operator becuase we can't guarantee that we have the token in storage
    Authorization: 'Bearer ' + JSON.parse(localStorage.getItem('user'))?.token
  })
} */

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  // because services are singletons, they persist until the application is done
  // this makes them a good candidate for state management, we could use Redux, or Mobex but that is overkill
  // for this application
  members: Member[] = []; // this is used to reduce calls to the API
  memberCache = new Map(); // using a Map allows us to use get and set to store query results in cache
  user: User | undefined;
  userParams: UserParams | undefined;

  // because its a service that is going to make http requests to our API, bring in the http client
  // because we need access to the user, inject the AccountService.
  // Its ok to inject services into other services so long as you do not create a circular reference
  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) {
          this.userParams = new UserParams(user);
          this.user = user;
        }
      }
    })
  }

  /* we can user these get/set helper methods inside our components */

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    if (this.user) {
      // this clears our the userParams by setting it equal to a new object
      this.userParams = new UserParams(this.user);
      return this.userParams;
    }
    return;
  }

  // this is old method that checks cache, before we had pagination
  /*getMembers(): Observable<Member[]> {
    // check if we already have the members loaded before call to API
    if (this.members.length > 0) return of(this.members); // "of" will return something as an observable

    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.members = members; // save the members the first time we try to get them
        return members; // map already returns things as observables
      })
    )
  }*/

  getMembers(userParams: UserParams) {
    // use a string of the userParams separated by - as the key store query results in cache
    const response = this.memberCache.get(Object.values(userParams).join('-'));

    if (response) return of(response);

    let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

    params = params.append('minAge', userParams.minAge.toString());
    params = params.append('maxAge', userParams.maxAge.toString());
    params = params.append('gender', userParams.gender.toString());
    params = params.append('orderBy', userParams.orderBy.toString());
    
    // use a pipe() save the query results in cache using params string as the key, after the query is made to the API
    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(
      map(response => { 
        this.memberCache.set(Object.values(userParams).join('-'), response);
        return response;
      })
    )
  }

  

  getMember(username: string) {

    // reduce the various arrays of members into a single array, then search for the username in the cache
    const member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.result), [])
      .find((member: Member) => member.username === username);

    // if we find the member in the cache, then use that
    if (member) return of(member);

    // otherwise we need to make a call to the api to get the user
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;  // update the member in our member array
      }
      )
    );
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }

  addLike(username: string) {
    //for post, just pass empty object {}
    return this.http.post(this.baseUrl + 'likes/' + username, {});
  }

  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = getPaginationHeaders(pageNumber, pageSize);

    params = params.append('predicate', predicate);

    return getPaginatedResult<Member[]>(this.baseUrl + 'likes', params, this.http);
  }

}
