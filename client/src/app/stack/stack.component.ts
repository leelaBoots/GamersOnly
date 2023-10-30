import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-stack',
  templateUrl: './stack.component.html',
  styleUrls: ['./stack.component.css']
})
export class StackComponent {
  constructor(private router: Router){}

  ngOnInit() {
    
  }

  getHome() {
    this.router.navigateByUrl('/');
  }
}
