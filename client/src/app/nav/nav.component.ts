import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  public environment = environment;
  model: any = {}

  // accountService is public so that we can access it in the html template, even though nav.component.html is associated with this object
  // inject Router so we can route to a url via code
  constructor(public accountService: AccountService, private router: Router,
              private toastr: ToastrService) {}

  ngOnInit(): void {
  
  }

  // tslint:disable-next-line: typedef
  login() {
    this.accountService.login(this.model).subscribe({
      next: _ => {
        this.router.navigateByUrl('/members');
        this.model = {}; // this sets model to empty object, so it clears out the username/password in the nav bar
      }
    })
  }

  logout() {
    this.accountService.logout(); // call our accountService's logout method
    this.router.navigateByUrl('/'); // send back to homepage after logging out
  }

  onHomePage() {
    return (this.router.url === '/');
  }

}
