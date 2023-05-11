import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm // this is needed so we can clear out the form after the message is sent
  @Input() username?: string;
  
  // messages is a property that we will pass down, in memmmber-detail.componenet.html  we use [messages]
  // we no longer use this, because we are using messageHub
  //@Input() messages: Message[] = [];
  
  messageContent = '';

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {
  }

  sendMessage() {
    if (!this.username) return;

    // because we are returning a promise, we can use "then" instead of "subscribe"
    // we dont need to do anything with the message we get back, our messageTHread observable handles that
    // we just need to reset the form
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      this.messageForm?.reset();
    })
  }

}
