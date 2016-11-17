﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Crossroads.Utilities.Services.Interfaces;
using Microsoft.Win32.SafeHandles;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Printing.Utilities.Models;
using Printing.Utilities.Services.Interfaces;
using SignInCheckIn.Models.DTO;
using SignInCheckIn.Services.Interfaces;

namespace SignInCheckIn.Services
{
    public class ChildSigninService : IChildSigninService
    {
        private readonly IChildSigninRepository _childSigninRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventService _eventService;
        private readonly IGroupRepository _groupRepository;
        private readonly IKioskRepository _kioskRepository;
        private readonly IPrintingService _printingService;
        private readonly IPdfEditor _pdfEditor;

        public ChildSigninService(IChildSigninRepository childSigninRepository, IEventRepository eventRepository, 
            IGroupRepository groupRepository, IEventService eventService, IPdfEditor pdfEditor, IPrintingService printingService,
            IContactRepository contactRepository, IKioskRepository kioskRepository)
        {
            _childSigninRepository = childSigninRepository;
            _contactRepository = contactRepository;
            _eventRepository = eventRepository;
            _groupRepository = groupRepository;
            _eventService = eventService;
            _kioskRepository = kioskRepository;
            _printingService = printingService;
            _pdfEditor = pdfEditor;
        }

        public ParticipantEventMapDto GetChildrenAndEventByPhoneNumber(string phoneNumber, int siteId)
        {
            var eventDto = _eventService.GetCurrentEventForSite(siteId);
            var householdId = _childSigninRepository.GetHouseholdIdByPhoneNumber(phoneNumber);

            var mpChildren = _childSigninRepository.GetChildrenByHouseholdId(householdId, Mapper.Map<MpEventDto>(eventDto));
            var childrenDtos = Mapper.Map<List<MpParticipantDto>, List<ParticipantDto>>(mpChildren);

            //var mpHouseholdContactDtos = _contactRepository.GetHeadsOfHouseholdByHouseholdId(householdId);

            ParticipantEventMapDto participantEventMapDto = new ParticipantEventMapDto
            {
                Participants = childrenDtos,
                CurrentEvent = eventDto
            };

            return participantEventMapDto;
        }

        public ParticipantEventMapDto SigninParticipants(ParticipantEventMapDto participantEventMapDto)
        {
            var eventDto = _eventService.GetEvent(participantEventMapDto.CurrentEvent.EventId);

            if (_eventService.CheckEventTimeValidity(eventDto) == false)
            {
                throw new Exception("Sign-In Not Available For Event " + eventDto.EventId);
            }

            // Get groups that are configured for the event
            var eventGroupsForEvent = _eventRepository.GetEventGroupsForEvent(participantEventMapDto.CurrentEvent.EventId);

            var mpEventParticipantDtoList = (from participant in participantEventMapDto.Participants.Where(r => r.Selected)
                // Get groups for the participant
                let groupIds = _groupRepository.GetGroupsForParticipantId(participant.ParticipantId)

                // TODO: Gracefully handle exception for mix of valid and invalid signins
                let eventGroup = eventGroupsForEvent.Find(r => groupIds.Exists(g => r.GroupId == g.Id))

                select
                    new MpEventParticipantDto
                    {
                        EventId = participantEventMapDto.CurrentEvent.EventId,
                        ParticipantId = participant.ParticipantId,
                        ParticipantStatusId = 3, // Status ID of 3 = "Attended"
                        TimeIn = DateTime.Now,
                        OpportunityId = null,
                        RoomId = eventGroup.RoomReservation.RoomId
                    }).ToList();


            var response = new ParticipantEventMapDto
            {
                CurrentEvent = participantEventMapDto.CurrentEvent,
                Participants = _childSigninRepository.CreateEventParticipants(mpEventParticipantDtoList).Select(Mapper.Map<ParticipantDto>).ToList()
            };

            //response.Participants.ForEach(p => p.Selected = true);

            foreach (var item in response.Participants)
            {
                item.Selected = true;
                var label = _pdfEditor.PopulatePdfMergeFields("somepath", new Dictionary<string, string>());
                var printId = _printingService.SendPrintRequest(new PrintRequestDto());
            }

            return response;
        }

        public ParticipantEventMapDto PrintParticipants(ParticipantEventMapDto participantEventMapDto, string kioskIdentifier)
        {
            // TODO: Finish this
            //var kiofkConfig = _kioskRepository.GetMpKioskConfigByIdentifier(Guid.Parse(kioskIdentifier));

            //if (kiofkConfig.PrinterMapId != null)
            //{
            //    var kioskPrinterMap = _kioskRepository.GetPrinterMapById(kiofkConfig.PrinterMapId.GetValueOrDefault());
            //}
            //else
            //{
            //    throw new Exception("Printer Map Id Not Set For Kisok " + kiofkConfig.KioskConfigId);
            //}

            //foreach (var participant in participantEventMapDto.Participants.Where(r => r.Selected == true))
            //{
            //    var participantRoom = _eventRepository.

            //    Dictionary<string, string> printValues = new Dictionary<string, string>
            //    {
            //        { "ChildName", participant.FirstName },
            //        { "ChildRoomName1", participant. }
            //    }
            //}

            return participantEventMapDto;
        }
    }
}
