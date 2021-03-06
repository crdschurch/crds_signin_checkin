import { NgModule } from '@angular/core';
import { HeaderModule } from './header';
import { SharedModule } from '../shared/shared.module';
import { MomentModule } from 'angular2-moment';

import { AdminComponent } from './admin.component';
import { SignInComponent } from './sign-in';
import { ReportsComponent } from './reports';
import { EventsModule } from './events/events.module';
import { adminRouting } from './admin.routes';

@NgModule({
  // the view classes that belong to this module.
  // Angular has three kinds of view classes:
  // components, directives, and pipes.
  declarations: [
    AdminComponent,
    SignInComponent,
    ReportsComponent,
  ],
  // other modules whose exported classes are needed
  // by component templates declared in this module.
  imports: [
    HeaderModule,
    adminRouting,
    SharedModule,
    EventsModule,
    MomentModule
  ],
  // the subset of declarations that should be visible and
  // usable in the component templates of other modules.
  exports: [],
  // creators of services that this module contributes to the global collection
  // of services; they become accessible in all parts of the app.
  providers: []
})

export class AdminModule { }
