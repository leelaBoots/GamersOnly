import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../_models/user';
import { BehaviorSubject, take } from 'rxjs';
import { Group } from '../_models/group';
import { BusyService } from './busy.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  // inject httpClient
  constructor(private http: HttpClient, private busyService: BusyService) { }

  createHubConnection(user: User, otherUsername: string) {
    this.busyService.busy();

    // 'message' hub name defined in program.cs in the API
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

      this.hubConnection.start()
        .catch(error => console.log(error))
        .finally(() => this.busyService.idle());

      this.hubConnection.on('ReceiveMessageThread', messages => {   
        this.messageThreadSource.next(messages);
      })

      this.hubConnection.on('UpdatedGroup', (group: Group) => {
        // here we are going to see if there are any messages to the user that just joined the group that are marked as unread.
        // if so, we will mark them as read in our client to match how they are in the server
        if (group.connections.some(x => x.username === otherUsername)) {
          this.messageThread$.pipe(take(1)).subscribe({
            next: messages => {
              messages.forEach(message => {
                if (!message.dateRead) {
                  message.dateRead = new Date(Date.now())
                }
              })
              // use the ... spread operator to replace the array with our updated version of the array
              this.messageThreadSource.next([...messages]);
            }
          })
        }
      })

      this.hubConnection.on('NewMessage', message => {
        this.messageThread$.pipe(take(1)).subscribe({
          next: messages => {
            this.messageThreadSource.next([...messages, message])
          }
        })
      })
  }

  stopHubConnection() {
    if (this.hubConnection) {
      // sending empty array will clear out the messages when navigating away from the message component
      // this prevents wrong messages initially appearing when returning to the message component for a different user
      this.messageThreadSource.next([]);
      this.hubConnection.stop();
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http);

  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  // async forces promise to be returned from this method
  async sendMessage(username: string, content: string) {
    // we no longer use http post, we will use our message hub
    //return this.http.post<Message>(this.baseUrl + 'messages', {recipientUsername: username, content});

    // invokes a message on our server. SendMessage defined MessageHub
    return this.hubConnection?.invoke('SendMessage', {recipientUsername: username, content})
      .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
