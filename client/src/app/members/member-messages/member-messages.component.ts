import { Component, Input, OnInit } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() username?: string;
  // messages is a property that we will pass down, in memmmber-detail.componenet.html  we use [messages]
  @Input() messages: Message[] = [];

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
  }

}
