import { Child, Contact, Event } from '.';

export class EventParticipants {
  Contacts: Array<Contact>;
  CurrentEvent: Event;
  Participants: Array<Child> = [];
  HouseholdId: number;
  HouseholdPhoneNumber: string;
  ServicesAttended: number;
  KioskTypeId: number;

  static fromJson(json: any): EventParticipants {
    if (!json) {
      return new EventParticipants();
    }

    let eventParticipants = new EventParticipants();
    eventParticipants.CurrentEvent = Event.fromJson(json.CurrentEvent);
    eventParticipants.ServicesAttended = json.ServicesAttended;
    eventParticipants.HouseholdId = json.HouseholdId;
    eventParticipants.HouseholdPhoneNumber = json.HouseholdPhoneNumber;
    eventParticipants.KioskTypeId = json.KioskTypeId;
    eventParticipants.Participants = [];
    for (let p of json.Participants) {
      eventParticipants.Participants.push(Child.fromJson(p));
    }
    eventParticipants.Contacts = Array.isArray(json.Contacts) ? (<Array<any>>(json.Contacts)).map(c => Contact.fromJson(c)) : [];
    return eventParticipants;
  }

  public hasSelectedParticipants() {
    return this.hasParticipants() && this.Participants.find(p => p.selected()) !== undefined;
  }

  public removeUnselectedParticipants() {
    this.Participants = this.Participants.filter(p => p.selected());
  }

  public hasParticipants(): boolean {
    return this.Participants && this.Participants.length > 0;
  }
}
