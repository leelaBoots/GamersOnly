import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { ToastrService } from 'ngx-toastr';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  // we dont have access to this memberTabs view child until AFTER the component is constructed because it is dynamic.
  // add static: true to resolve this, along with using routeResolver
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent;
  member: Member =  {} as Member; // workaround, set member to an empty object if does not exist
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  activeTab?: TabDirective;
  messages: Message[] = [];
  user?: User;

  constructor(private accountService: AccountService, private route: ActivatedRoute, private messageService: MessageService,
    public presenceService: PresenceService, private memberService: MembersService, private toastr: ToastrService) { 
      this.accountService.currentUser$.pipe(take(1)).subscribe({
        next: user => {
          if (user) this.user = user;
        }
      });
  }

  ngOnInit(): void {
    // this.loadMember(); // we no longer get member from the memberService, we will get it from our Route
    this.route.data.subscribe({
      next: data => this.member = data['member']
    })

    // this returns an observable, so we must subscribe to it
    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ]

    this.galleryImages = this.getImages();
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      })
    }
    return imageUrls;
  }

  addLike(member: Member) {
    this.memberService.addLike(member.username).subscribe({
      next: () => this.toastr.success('You have liked ' + member.knownAs)
    })
  }

  // we dont use this method anymore, because we are getting the member from the route, before whatever element is constructed
  /*loadMember() {
    this.memberService.getMember(this.route.snapshot.paramMap.get('username')).subscribe(member => {
      this.member = member;

      // get images now that we actaully have the member
      this.galleryImages = this.getImages();
    })
  }*/

  selectTab(heading: string) {
    if(this.memberTabs) {
      this.memberTabs.tabs.find(x => x.heading === heading)!.active = true;
    }
  }

  loadMessages() {
    if (this.member) {
      // the tutorial named this userName, but i called it username
      this.messageService.getMessageThread(this.member.username).subscribe({
        next: messages => this.messages = messages
      })
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.user) {
      this.messageService.createHubConnection(this.user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

}
