import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm: NgForm; // this gives us access to the editForm element from the html template
  member: Member;
  user: User;
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    // HostListener used to show message before navigating away in browser
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(private accountService: AccountService, private memberService: MembersService, private toastr: ToastrService) {
    // we need to get the user out of the observable. we will end up with the current user form accountService
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user);
  }
   

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    this.memberService.getMember(this.user.username).subscribe(member => {
      this.member = member;
    })
  }

  updateMember() {
    this.memberService.updateMember(this.member).subscribe(() => {
      this.toastr.success('Profile Updated Successfully');

      // we get editForm from the html template so that after updating the profile, we can reset the state of the form
      // (so its not "dirty"), but we need to pass it this.member which is the updated member, so that the values will
      // be the updated ones. this will disable submit button and clear message
      this.editForm.reset(this.member);
    })
    
  }

}
