﻿using System.Collections.Generic;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ChildCheckinRepository : IChildCheckinRepository
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public ChildCheckinRepository(IApiUserRepository apiUserRepository, IMinistryPlatformRestRepository ministryPlatformRestRepository)
        {
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
        }

        public List<MpParticipantDto> GetChildrenByEventAndRoom(int eventId, int roomId)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            var columnList = new List<string>
            {
                "Event_ID_Table.Event_ID",
                "Room_ID_Table.Room_ID",
                "Event_Participants.Event_Participant_ID",
                "Participant_ID_Table.Participant_ID",
                "Participant_ID_Table_Contact_ID_Table.Contact_ID",
                "Participant_ID_Table_Contact_ID_Table.First_Name",
                "Participant_ID_Table_Contact_ID_Table.Last_Name",
                "Participation_Status_ID_Table.Participation_Status_ID"
            };

            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).
                        SearchTable<MpParticipantDto>("Event_Participants", $"Event_ID_Table.[Event_ID] = {eventId} AND Room_ID_Table.[Room_ID] = {roomId}", columnList);
        }

        public void CheckinChildrenForCurrentEventAndRoom(int checkinStatusId, int eventParticipantId)
        {
            var apiUserToken = _apiUserRepository.GetToken();
            
            var updateObject = new Dictionary<string, object>
            {
                { "Event_Participant_ID", eventParticipantId },
                { "Participation_Status_ID_Table.Participation_Status_ID", checkinStatusId }
            };

            _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).UpdateRecord("Event_Participants", eventParticipantId, updateObject);
        }
    }
}