import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

import { RoomComponent } from './room';
import { HttpClientService } from '../shared/services';
import { Child, Event, Room } from '../shared/models';

@Injectable()
export class ChildCheckinService {
  private _roomComp: RoomComponent;
  private _roomSetUpFunc: Function;
  private _selectedEvent: Event;
  private url = '';
  private forceChildReloadSource = new Subject<Event>();
  forceChildReload$ = this.forceChildReloadSource.asObservable();

  constructor(private http: HttpClientService) {
    this.url = `${process.env.ECHECK_API_ENDPOINT}/checkin`;
  }

  forceChildReload(newSelectedEvent) {
    this._selectedEvent = newSelectedEvent;
    this.forceChildReloadSource.next();
  }

  getChildrenForRoom(roomId: number, eventId: number = null) {
    const url = (eventId == null) ? `${this.url}/children/${roomId}` : `${this.url}/children/${roomId}?eventId=${eventId}`;

    return this.http.get(url).map((response) => {
        let childrenAvailable: Array<Child> = [];
        const participants = response.json().Participants;
        if (participants) {
          for (let kid of response.json().Participants) {
            let child = Object.create(Child.prototype);
            Object.assign(child, kid);
            // set all selected to true
            // TODO: backend should probably do this
            child.AssignedRoomId = roomId;
            child.Selected = true;
            childrenAvailable.push(child);
          }
        }

        return childrenAvailable;
      }).catch(e => {
        return Observable.throw(e);
      });
  }

  checkInChildren(child: Child, eventId: number) {
    const url = `${this.url}/event/${eventId}/participant`;
    child.toggleCheckIn();

    return this.http.put(url, child)
                    .map(res => Child.fromJson(res.json()))
                    .catch(this.handleError);
  }

  getChildByCallNumber(eventId: number, callNumber: string, roomId: number) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/checkin/events/${eventId}/child/${callNumber}/rooms/${roomId}`;
    return this.http.get(url)
                    .map(res => {
                      return Child.fromJson(res.json());
                    }).catch(e => {
                      return Observable.throw(e);
                    });
  }

  overrideChildIntoRoom(child: Child, eventId: number, roomId: number) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/checkin/events/` +
      `${eventId}/child/${child.EventParticipantId}/rooms/${child.AssignedRoomId}/override/${roomId}`;
    return this.http.put(url, {})
                    .map(res => {
                      return Observable.of();
                    }).catch(e => {
                      return Observable.throw(e.json().errors[0]);
                    });
  }

  getEventRoomDetails(eventId: number, roomId: number) {
    const url = `${process.env.ECHECK_API_ENDPOINT}/events/${eventId}/rooms/${roomId}`;
    return this.http.get(url)
                    .map(res => Room.fromJson(res.json()))
                    .catch(this.handleError);
  }

  get selectedEvent(): Event {
    return this._selectedEvent;
  }

  set selectedEvent(event) {
    this._selectedEvent = event;
    this._roomSetUpFunc(this._roomComp);
  }

  set roomSetUpFunc(func: Function) {
    this._roomSetUpFunc = func;
  }

  set roomComp(comp: RoomComponent) {
    this._roomComp = comp;
  }

  private handleError (error: any) {
    return Observable.throw(error || 'Server error');
  }
}
