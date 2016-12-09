import { Component, OnInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';

import { NewFamily, NewParent, NewChild, Group } from '../../../shared/models';
import { AdminService } from '../../admin.service';
import { ApiService, RootService } from '../../../shared/services';
import { HeaderService } from '../../header/header.service';

import * as moment from 'moment';

@Component({
  styleUrls: ['new-family-registration.component.scss'],
  selector: 'new-family-registration',
  templateUrl: 'new-family-registration.component.html'
})
export class NewFamilyRegistrationComponent implements OnInit {
  private mask: any = [/[1-9]/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, '-', /\d/, /\d/, /\d/, /\d/];
  private eventId: string;
  private family: NewFamily;
  private gradeGroups: Array<Group> = [];
  private processing: boolean;
  private submitted: boolean;
  private maxDate: Date = moment().toDate();

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    private headerService: HeaderService,
    private adminService: AdminService,
    private rootService: RootService,
    private router: Router) {}

  ngOnInit() {
    this.setUp();
  }

  setUp() {
   this.processing = false;
   this.submitted = false;
   this.eventId = this.route.snapshot.params['eventId'];
   this.family = new NewFamily();
   this.family.parent = new NewParent();
   this.family.children = [this.newChild()];
   this.family.numberOfKids = 1;

   this.apiService.getEvent(this.eventId).subscribe((event) => {
        this.family.event = event;
        this.headerService.announceEvent(event);
        this.adminService.getGradeGroups().subscribe((groups) => {
            this.gradeGroups = groups;
          },
          error => console.error(error)
        );
      },
      error => console.error(error)
    );
  }

  updateNumberOfKids(): void {
    let tmpChildren: Array<NewChild> = [];

    for (let i = 0; i < this.family.numberOfKids; i++) {
      if (this.family.children[i] === undefined) {
        tmpChildren.push(this.newChild());
      } else {
        tmpChildren.push(this.family.children[i]);
      }
    }

    this.family.children = tmpChildren;
  }

  needGradeLevel(child: NewChild): boolean {
    return moment(child.DateOfBirth).isBefore(moment().startOf('day').subtract(5, 'y'));
  }

  updateChildYearGradeGroup(child: NewChild, groupId: number) {
    child.YearGrade = groupId;
  }

  onSubmit(form: NgForm) {
    this.submitted = true;
    if (!form.pristine && form.valid) {
      this.processing = true;
      this.adminService.createNewFamily(this.family).subscribe((res) => {
        this.rootService.announceEvent('echeckNewFamilyCreated');
        form.resetForm();
        setTimeout(() => {
          this.setUp();
        });
      }, (error) => {
        switch (error.status) {
          case 412:
            this.rootService.announceEvent('echeckNewFamilyAdminSetupWrong');
            break;
          default:
            this.rootService.announceEvent('generalError');
            break;
          }
        this.processing = false;
      });
    }
  }

  private newChild(): NewChild {
    let child = new NewChild();
    child.DateOfBirth = moment().startOf('day').toDate();

    return child;
  }
}
