import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormControl, FormGroup } from '@angular/forms';
import { AdminService } from '../../admin.service';
import { RootService, ChannelEvent, ChannelService  } from '../../../shared/services';
import { Constants } from '../../../shared/constants';
import { Room } from '../../../shared/models';
import * as _ from 'lodash';

@Component({
  selector: '.room',
  templateUrl: 'room.component.html',
  styleUrls: ['room.component.scss'],
  providers: [ ]
})
export class RoomComponent implements OnInit {
  @Input() room: Room;
  @Output() notifyDirty: EventEmitter<boolean> = new EventEmitter<boolean>();
  public pending: boolean;
  private roomForm: FormGroup;
  private origRoomData: Room;
  private eventId: any;
  public _dirty: boolean;
  private sobots: Number[] = [];

  constructor(private route: ActivatedRoute, private adminService: AdminService, private rootService: RootService, private channelService: ChannelService) {
  }

  mainEventId() {
    return this.eventId;
  }

  highlight(e) {
    e.target.select();
  }

  add(field) {
    this.room[field]++;
    this.dirty = true;
  }

  remove(field) {
    if (this.room[field] >= 1) {
      this.room[field]--
      this.dirty = true;
    }
  }
  toggle(field) {
    this.room[field] = !this.room[field];
    this.dirty = true;
  }

  saveRoom() {
    this.pending = true;
    this.adminService.updateRoom(this.room.EventId, this.room.RoomId, this.room).subscribe(room => {
      this.origRoomData = _.clone(this.room);
      this.room = room;
      this.dirty = false;
      this.pending = false;
    }, (error) => {
      this.room = this.origRoomData;
      this.dirty = false;
      this.pending = false;
      this.rootService.announceEvent('generalError');
    });
    return false;
  }

  set dirty(isChanged) {
    this._dirty = isChanged;
    this.notifyDirty.emit(isChanged);
  }

  get dirty() {
    return this._dirty;
  }

  hasCapacity() {
    return this.room.Capacity;
  }

  change() {
    this.dirty = true;
  }

  checkedInEqualsCapacity() {
    return this.room.CheckedIn >= this.room.Capacity;
  }

  signedInWillEqualCapacity() {
    // only return true if checkedInEqualsCapacity isnt true
    if (!this.checkedInEqualsCapacity()) {
      return this.room.SignedIn + this.room.CheckedIn >= this.room.Capacity;
    }
  }

  getRoomRatioString() {
    if (this.room.CheckedIn || this.room.Volunteers) {
      return `${this.room.CheckedIn}/${this.room.Volunteers}`;
    } else {
      return '0';
    }
  }

  toggleClick() {
    if (this.pending) {
      return false;
    }
  }

  isAdventureClub() {
    return Number(this.room.EventId) !== Number(this.mainEventId());
  }

  ageRangeAndGrades(): any {
    let ageGrades = this.room.getSelectionDescription(false);

    if (ageGrades.length === 0) {
      ageGrades = ['Add'];
    }

    return ageGrades;
  }

  ngOnInit() {
    this.origRoomData = _.clone(this.room);
    this.eventId = this.route.snapshot.params['eventId']
    this.setup(this);
  }

  setup(comp) {
    // Get an observable for events emitted on this channel
    // Get an observable for events emitted on this channel
    let channelName = `${Constants.CheckinParticipantsChannel}${comp.eventId}${comp.room.RoomId}`;
    comp.channelService.sub(channelName).subscribe(
      (x: ChannelEvent) => {
        if (x.Name === 'Add') {
          comp.room.SignedIn++;
          comp.sobots.push(11);
        } else if (x.Name === 'Remove' && (x.Data.OriginalRoomId != x.Data.OverRideRoomId)) {
          comp.room.SignedIn--;
          return false;
        }
      },
      (error: any) => {
        console.warn('Attempt to join channel failed!', error);
      }
    );
  }
}
