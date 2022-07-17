import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from 'src/app/_models/member';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

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

  // because its a service that is going to make http requests to our API, bring in the http client
  constructor(private http: HttpClient) { }

  getMembers(): Observable<Member[]> {
    // check if we already have the members loaded before call to API
    if (this.members.length > 0) return of(this.members); // "of" will return something as an observable

    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.members = members; // save the members the first time we try to get them
        return members; // map already returns things as observables
      })
    )
  }

  getMember(username: string) {
    // attempt to find our user by username, use 3 === for typescript equality
    const member = this.members.find(x => x.username === username);
    if (member != undefined) return of(member); // find returns undefined if not found

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
}
