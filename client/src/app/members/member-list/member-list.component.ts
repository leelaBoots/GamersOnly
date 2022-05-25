import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members$: Observable<Member[]>; // the $ is a convention to denote Observable variable

  constructor(private memberService: MembersService) { }

  ngOnInit(): void {
    // this.loadMembers();
    // we will now do it this way, part of reducing API calls:
    this.members$ = this.memberService.getMembers();
  }

  // this method has no error handling because we let the error.interceptor handle it
  // Also we will no longer use this method, because we will save members in memory to avoid uneccessary API calls
  /*loadMembers() {
    this.memberService.getMembers().subscribe(members => {
      this.members = members;
    })
  }*/

}
