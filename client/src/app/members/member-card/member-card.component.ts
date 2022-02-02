import { Component, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  // add an input property because we are receiving this data from parent object (member-list)
  @Input() member: Member;

  constructor() { }

  ngOnInit(): void {
  }

}
