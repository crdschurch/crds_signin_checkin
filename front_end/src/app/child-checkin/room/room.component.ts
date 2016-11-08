import { Component, OnInit } from '@angular/core';
import { Child } from '../../shared/models/child';
import { ChildCheckinService } from '../child-checkin.service';
import { RootService } from '../../shared/services';

@Component({
  selector: 'room',
  templateUrl: 'room.component.html',
  styleUrls: ['room.component.scss' ]
})

export class RoomComponent implements OnInit {

  private _children: Array<Child> = [];

  constructor(private childCheckinService: ChildCheckinService, private rootService: RootService) {}

  ngOnInit() {
    this.childCheckinService.getChildrenForRoom(1820, 4525342).subscribe((children) => {
      this.children = children;
    }, (error) => {
      console.error(error);
      this.rootService.announceEvent('generalError');
    });
  }

  checkedIn(): Array<Child> {
    return this.children.filter( (obj: Child) => { return obj.checkedIn(); } );
  }

  signedIn(): Array<Child> {
    return this.children.filter( (obj: Child) => { return !obj.checkedIn(); } );
  }

  toggleCheckIn(child: Child) {
    this.childCheckinService.checkInChildren(child).subscribe(() => {
    }, (error) => {
      console.error(error);
      this.rootService.announceEvent('generalError');
    });
  }

  get children(): Array<Child> {
    return this._children;
  }

  set children(child: Array<Child>) {
    this._children = child;
  }
}

