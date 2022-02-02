import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from 'src/app/_models/member';

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

  // because its a service that is goint to make http requests to our API, bring in the http client
  constructor(private http: HttpClient) { }

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'users');
  }

  getMember(username: string) {
    return this.http.get<Member>(this.baseUrl + 'users/' + username)
  }
}
