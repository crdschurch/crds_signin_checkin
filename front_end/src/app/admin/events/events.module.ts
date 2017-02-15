import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { MomentModule } from 'angular2-moment';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { HeaderModule } from '../header';
import { NewFamilyRegistrationModule } from './new-family-registration';
import { ManageChildrenComponent } from './manage-children/manage-children.component';
import { EventImportComponent } from './import/event-import.component';
import { EventListComponent } from './event-list.component';
import { EventResetComponent } from './reset/event-reset.component';
import { RoomListComponent } from './rooms/room-list.component';
import { RoomGroupListComponent } from './rooms/room-group-list.component';
import { RoomGroupComponent } from './rooms/room-group.component';
import { RoomComponent } from './rooms/room.component';
import { RoomBumpComponent } from './rooms/alternate-rooms/room-bump.component';
import { AlternateRoomsComponent } from './rooms/alternate-rooms/alternate-rooms.component';
import { eventsRouting } from './events.routes';
import { CanDeactivateGuard } from '../../shared/guards';

@NgModule({
  declarations: [
    EventImportComponent,
    EventListComponent,
    EventResetComponent,
    RoomListComponent,
    RoomGroupListComponent,
    RoomGroupComponent,
    RoomComponent,
    ManageChildrenComponent,
    AlternateRoomsComponent,
    RoomBumpComponent
  ],
  imports: [
    eventsRouting,
    SharedModule,
    MomentModule,
    HeaderModule,
    NewFamilyRegistrationModule,
    ReactiveFormsModule
  ],
  exports: [ ],
  providers: [ CanDeactivateGuard ]
})

export class EventsModule { }
