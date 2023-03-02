import { Component, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { UserParams } from 'src/app/_models/userParams';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  //members$: Observable<Member[]>; // the $ is a convention to denote Observable variable. not using this after adding pagination
  members: Member[];
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]

  // we're injecting our accountService beacuse we need access to the user
  constructor(private memberService: MembersService) { 
    this.userParams = this.memberService.getUserParams();
  }

  ngOnInit(): void {
    this.loadMembers();
    // we will now do it this way, part of reducing API calls. now this is the old way.
    // this.members$ = this.memberService.getMembers();
  }

  // this method has no error handling because we let the error.interceptor handle it
  // Also we will no longer use this method, because we will save members in memory to avoid uneccessary API calls
  /*loadMembers() {
    this.memberService.getMembers().subscribe(members => {
      this.members = members;
    })
  }*/

  loadMembers() {
    // we dont need the pipe in subscribe to take 1, http requests typically return
    if (this.userParams) {
      this.memberService.setUserParams(this.userParams);
      this.memberService.getMembers(this.userParams).subscribe({
        next: response => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      })
    }
    
  }

  // this will allow user to reset their gender prefs
  resetFilters() {
    this.userParams = this.memberService.resetUserParams();
    this.loadMembers();
  }

  pageChanged(event: any) {
    if (this.userParams && this.userParams?.pageNumber !==event.page) {
      this.userParams.pageNumber = event.page;
      // take this opportunity to update the memberService as well, so queries will be stored
      this.memberService.setUserParams(this.userParams);
      this.loadMembers();
    }
  }

}
