﻿using System.Collections.Generic;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System.Linq;
using Crossroads.Utilities.Services.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ChildSigninRepository : IChildSigninRepository
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IApplicationConfiguration _applicationConfiguration;

        public ChildSigninRepository(IApiUserRepository apiUserRepository,
            IMinistryPlatformRestRepository ministryPlatformRestRepository,
            IApplicationConfiguration applicationConfiguration)
        {
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _applicationConfiguration = applicationConfiguration;
        }

        public List<MpParticipantDto> GetChildrenByPhoneNumber(string phoneNumber)
        {
            var householdId = GetHouseholdIdByPhoneNumber(phoneNumber);

            if (householdId == null) return new List<MpParticipantDto>();
            var children = GetChildParticpantsByPrimaryHousehold(householdId);
            GetChildParticpantsByOtherHousehold(householdId, children);
            children = GetOnlyKidsClubChildren(children);
            return children.Distinct(new MpParticipantDtoComparer()).ToList();
        }

        private int? GetHouseholdIdByPhoneNumber(string phoneNumber)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            var columnList = new List<string>
            {
                "Contact_ID",
                "Household_ID_Table.Household_ID",
                "Household_Position_ID_Table.Household_Position_ID",
                "Household_ID_Table.Home_Phone",
                "Mobile_Phone",
            };

            var household = _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).
                Search<MpContactDto>($"Household_Position_ID_Table.[Household_Position_ID] IN ({_applicationConfiguration.HouseHoldIdsThatCanCheckIn}) AND ([Mobile_Phone] = '{phoneNumber}' OR Household_ID_Table.[Home_Phone] = '{phoneNumber}')", columnList);

            if (household == null || !household.Any())
            {
                return null;
            }

            return household.First().HouseholdId;
        }

        private List<MpParticipantDto> GetChildParticpantsByPrimaryHousehold(int? householdId)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            var columnList = new List<string>
            {
                "Participant_ID",
                "Contact_ID_Table.Contact_ID",
                "Contact_ID_Table_Household_ID_Table.Household_ID",
                "Contact_ID_Table_Household_Position_ID_Table.Household_Position_ID",
                "Contact_ID_Table.First_Name",
                "Contact_ID_Table.Last_Name",
                "Contact_ID_Table.Date_of_Birth",
            };

            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).
                        Search<MpParticipantDto>($"Contact_ID_Table_Household_ID_Table.[Household_ID] = {householdId} AND Contact_ID_Table_Household_Position_ID_Table.[Household_Position_ID] = {_applicationConfiguration.MinorChildId}", columnList);
        }

        private void GetChildParticpantsByOtherHousehold(int? householdId, List<MpParticipantDto> children)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            var columnList = new List<string>
            {
                "Contact_ID_Table_Participant_Record_Table.Participant_ID",
                "Contact_ID_Table.Contact_ID",
                "Household_ID_Table.Household_ID",
                "Household_Position_ID_Table.Household_Position_ID",
                "Contact_ID_Table.First_Name",
                "Contact_ID_Table.Last_Name",
                "Contact_ID_Table.Date_of_Birth",
            };

            var otherChildren = _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).
                                    SearchTable<MpParticipantDto>("Contact_Households", $"Household_Position_ID_Table.[Household_Position_ID] = {_applicationConfiguration.MinorChildId}  AND Household_ID_Table.[Household_ID] = {householdId}", columnList);

            foreach (var child in otherChildren)
            {
                if (!children.Exists(x => x.ContactId == child.ContactId))
                {
                    children.Add(child);
                }
            }
        }

        private List<MpParticipantDto> GetOnlyKidsClubChildren(List<MpParticipantDto> children)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            var columnList = new List<string>
            {
                "Participant_ID_Table.Participant_ID",
                "Participant_ID_Table_Contact_ID_Table.Contact_ID",
                "Participant_ID_Table_Contact_ID_Table_Household_ID_Table.Household_ID",
                "Participant_ID_Table_Contact_ID_Table_Household_Position_ID_Table.Household_Position_ID",
                "Participant_ID_Table_Contact_ID_Table.First_Name",
                "Participant_ID_Table_Contact_ID_Table.Last_Name",
                "Participant_ID_Table_Contact_ID_Table.Date_of_Birth",
                "Group_ID_Table_Congregation_ID_Table.Congregation_ID",
                "Group_ID_Table_Group_Type_ID_Table.Group_Type_ID",
                "Group_ID_Table_Ministry_ID_Table.Ministry_ID"
            };

            var participantIds = string.Join(",", children.Select(x => x.ParticipantId));

            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).
                        SearchTable<MpParticipantDto>("Group_Participants", $"Participant_ID_Table.[Participant_ID] IN ({participantIds}) AND Group_ID_Table_Congregation_ID_Table.[Congregation_ID] = {_applicationConfiguration.KidsClubCongregationId} AND Group_ID_Table_Group_Type_ID_Table.[Group_Type_ID] = {_applicationConfiguration.KidsClubGroupTypeId} AND Group_ID_Table_Ministry_ID_Table.[Ministry_ID] = {_applicationConfiguration.KidsClubMinistryId}", columnList);
        }

        private class MpParticipantDtoComparer : IEqualityComparer<MpParticipantDto>
        {
            // Consider them equal if participant id and contact id are the same
            public bool Equals(MpParticipantDto x, MpParticipantDto y)
            {
                return x.ParticipantId == y.ParticipantId && x.ContactId == y.ContactId;
            }

            // Hash code is a hash of participant id and contact id
            public int GetHashCode(MpParticipantDto obj)
            {
                return $"{obj.ParticipantId}{obj.ContactId}".GetHashCode();
            }
        }

	    public void CreateEventParticipants(List<MpEventParticipantDto> mpEventParticipantDtos)
        {
            var token = _apiUserRepository.GetToken();

            var columnList = new List<string>
            {
                "Event_Participants_ID_Table.Event_Participant_ID",
                "Event_Participants_ID_Table.Event_ID",
                "Event_Participants_ID_Table.Participant_ID",
                "Event_Participants_ID_Table.Participation_Status_ID",
                "Event_Participants_ID_Table.Time_In",
                "Event_Participants_ID_Table.Time_Confirmed",
                "Event_Participants_ID_Table.Time_Out",
                "Event_Participants_ID_Table.Notes",
                "Event_Participants_ID_Table.Domain_ID",
                "Event_Participants_ID_Table.Group_Participant_ID",
                "Event_Participants_ID_Table.Check-in_Station",
                "Event_Participants_ID_Table.Group_ID",
                "Event_Participants_ID_Table.Room_ID",
                "Event_Participants_ID_Table.Call_Parents",
                "Event_Participants_ID_Table.Group_Role_ID",
                "Event_Participants_ID_Table.Response_ID",
                "Event_Participants_ID_Table.Opportunity_ID",
                "Event_Participants_ID_Table.Registrant_Message_Sent"
            };

            var objects = _ministryPlatformRestRepository.UsingAuthenticationToken(token).Create(mpEventParticipantDtos, columnList);
        }

        //public List<T> Create<T>(List<T> objectsToCreate, List<string> selectColumns)
        //{
        //    return Create(objectsToCreate, string.Join(",", selectColumns.ToArray()));
        //}

        //public List<MpEventGroupDto> CreateEventGroups(string authenticationToken, List<MpEventGroupDto> eventGroups)
        //{
        //    var token = authenticationToken ?? _apiUserRepository.GetToken();
        //    return _ministryPlatformRestRepository.UsingAuthenticationToken(token).Create(eventGroups, _eventGroupsColumns);
        //}
    }
}
