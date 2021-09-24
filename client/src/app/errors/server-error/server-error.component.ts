import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-server-error',
  templateUrl: './server-error.component.html',
  styleUrls: ['./server-error.component.css']
})
export class ServerErrorComponent implements OnInit {
  error: any;

  /* we can only access the Router state in the constructor */
  constructor(private router: Router) {
    const navigation = this.router.getCurrentNavigation();
    // be safe with navigation because we only get this once when the user navigates, it gets cleared if they refresh page
    // the ? is called optional chaining. checks if it exists before proceeding
    this.error = navigation?.extras?.state?.error;
   }

  ngOnInit(): void {
  }

}
