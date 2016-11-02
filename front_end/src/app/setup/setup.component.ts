import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { SetupService } from './setup.service';
import { RootService } from '../shared/services';

@Component({
  selector: 'setup',
  templateUrl: 'setup.component.html',
  styleUrls: ['setup.component.scss'],
  providers: [ SetupService ]
})
export class SetupComponent implements OnInit {
  machineId: string;
  isError: boolean;

  constructor(private setupService: SetupService,
              private route: ActivatedRoute,
              private rootService: RootService) {}

  reset() {
    document.cookie.split(';').forEach(function(c) {
      document.cookie = c.replace(/^ +/, '').replace(/=.*/, '=;expires=' + new Date().toUTCString() + ';path=/');
    });
    this.machineId = undefined;
  }

  ngOnInit() {
    this.isError = this.route.snapshot.params['error'];
    let machineGuid;
    this.route.params.forEach((params: Params) => {
       machineGuid = params['machine'];
     });
    if (machineGuid || this.setupService.getMachineIdConfigCookie()) {
      this.machineId = machineGuid || this.setupService.getMachineIdConfigCookie();
      this.setupService.setMachineIdConfigCookie(this.machineId);
    }
  }
}
