import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {}

  // accountService is public so that we can access it in the html template, even though nav.component.html is associated with this object
  constructor(public accountService: AccountService) {}

  ngOnInit(): void {
  }

  // tslint:disable-next-line: typedef
  login() {
    this.accountService.login(this.model).subscribe(response => {
      console.log(response);
    }, error => {
      console.log(error);
    })
  }

  logout() {
    this.accountService.logout(); // call our accountService's logout method
  }

}
