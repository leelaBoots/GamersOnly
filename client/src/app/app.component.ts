import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
/* this is our base component */
export class AppComponent implements OnInit {
  title = 'The Dating app';
  users: any;

  constructor(private accountService: AccountService){}

  ngOnInit() {
    this.setCurrentUser();
  }

  setCurrentUser() {
    // because we stringified the object in local source, we must parse it here
    const user: User = JSON.parse(localStorage.getItem('user'));
    // here we make the effort to get the user token from local storage then setting it in our account service, to make the user login persistent on page refresh. 
    this.accountService.setCurrentUser(user);
  }

}
