﻿using Crossroads.Web.Common.MinistryPlatform;
using log4net;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MinistryPlatform.Translation.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly List<string> _eventGroupsColumns;
        private readonly List<string> _eventColumns;
        protected readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string ResetEventStoredProcedureName = "api_crds_ResetEcheckEvent";
        private const string ImportEventStoredProcedureName = "api_crds_ImportEcheckEvent";

        private const string nonEventTemplatesQueryString = "([Template]=0 OR [Template] IS NULL)";

        public EventRepository(IApiUserRepository apiUserRepository,
            IMinistryPlatformRestRepository ministryPlatformRestRepository)
        {
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;

            _eventGroupsColumns = new List<string>
            {
                "Event_Groups.[Event_Group_ID]",
                "Event_ID_Table.[Event_ID]",
                "Group_ID_Table.[Group_ID]",
                "Event_Room_ID_Table.[Event_Room_ID]",
                "Event_Room_ID_Table_Room_ID_Table.[Room_ID]",
                "Event_Room_ID_Table.[Capacity]",
                "Event_Room_ID_Table.[Label]",
                "Event_Room_ID_Table.[Allow_Checkin]",
                "Event_Room_ID_Table.[Volunteers]",
                "[dbo].crds_getEventParticipantStatusCount(Event_ID_Table.[Event_ID], Event_Room_ID_Table_Room_ID_Table.[Room_ID], 3) AS Signed_In",
                "[dbo].crds_getEventParticipantStatusCount(Event_ID_Table.[Event_ID], Event_Room_ID_Table_Room_ID_Table.[Room_ID], 4) AS Checked_In"
            };

            _eventColumns = new List<string>
            {
                "Event_ID",
                "Parent_Event_ID",
                "Template",
                "Event_Title",
                "Program_ID",
                "Primary_Contact",
                "Event_Start_Date",
                "Event_End_Date",
                "[Early_Check-in_Period]",
                "[Late_Check-in_Period]",
                "Event_Type_ID_Table.Event_Type",
                "Events.Event_Type_ID",
                "Congregation_ID_Table.Congregation_Name",
                "Events.Congregation_ID",
                "Congregation_ID_Table.Location_ID",
                "[Allow_Check-in]"
            };

        }

        /// <summary>
        /// The end date parameter is automatically cast to the end of the day for that date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="site"></param>
        /// <param name="includeSubevents"></param>
        /// <returns></returns>
        public List<MpEventDto> GetEvents(DateTime startDate, DateTime endDate, int site, bool? includeSubevents = false, List<int> eventTypeIds = null, bool excludeIds = true)
        {
            var apiUserToken = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");

            var startTimeString = startDate.ToString();
            // make sure end time is end of day
            var endTimeString = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59).ToString();

            var queryString =
                $"[Allow_Check-in]=1 AND [Cancelled]=0 AND [Event_Start_Date] >= '{startTimeString}' AND [Event_Start_Date] <= '{endTimeString}' AND Events.[Congregation_ID] = {site}";
            // only pull non-template events
            queryString = $"{queryString} AND {nonEventTemplatesQueryString}";

            if (excludeIds == true && eventTypeIds != null)
            {
                queryString += $" AND Events.[Event_Type_ID] NOT IN ({string.Join(",", eventTypeIds)})";
            }
            else if (eventTypeIds != null)
            {
                queryString += $" AND Events.[Event_Type_ID] IN ({string.Join(",", eventTypeIds)})";
            }

            if (includeSubevents != true)
            {
                // do not include subevents
                queryString = $"{queryString} AND [Parent_Event_ID] IS NULL";
            }
            var events = _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Search<MpEventDto>(queryString, _eventColumns);
            if (!events.Any())
            {
                // log query when no events are returned for debugging purposes
                Logger.Info($"No events found at {DateTime.Now} for query: ${queryString}");
            }
            return events;
        }

        public List<MpEventDto> GetEventTemplates(int site)
        {
            var apiUserToken = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            var queryString =
                $"[Allow_Check-in]=1 AND [Cancelled]=0 AND [Template]=1 AND Events.[Congregation_ID] = {site}";
            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Search<MpEventDto>(queryString, _eventColumns);
        }

        public MpEventDto GetEventById(int eventId)
        {
            var apiUserToken = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");

            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Get<MpEventDto>(eventId, _eventColumns);
        }

        public MpEventDto CreateSubEvent(MpEventDto mpEventDto)
        {
            var token = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            return _ministryPlatformRestRepository.UsingAuthenticationToken(token).Create(mpEventDto, _eventColumns);
        }

        public MpEventDto UpdateEvent(MpEventDto mpEventDto)
        {
            var token = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            return _ministryPlatformRestRepository.UsingAuthenticationToken(token).Update(mpEventDto, _eventColumns);
        }

        public List<MpEventGroupDto> GetEventGroupsForEvent(int eventId)
        {
            var eventIds = new List<int> { eventId };
            return GetEventGroupsForEvent(eventIds);
        }

        public List<MpEventGroupDto> GetEventGroupsForEventByGroupTypeId(int eventId, int groupTypeId)
        {
            return _ministryPlatformRestRepository.UsingAuthenticationToken(_apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn"))
                .Search<MpEventGroupDto>($"Event_Groups.Event_ID = {eventId} AND Group_ID_Table.[Group_Type_ID] = {groupTypeId}", _eventGroupsColumns);
        }

        public List<MpEventGroupDto> GetEventGroupsForEvent(List<int> eventIds)
        {
            return _ministryPlatformRestRepository.UsingAuthenticationToken(_apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn"))
                .Search<MpEventGroupDto>($"Event_Groups.Event_ID IN ({string.Join(",", eventIds)})", _eventGroupsColumns);
        }

        public List<MpEventGroupDto> GetEventGroupsForEventRoom(int eventId, int roomId)
        {
            return
                _ministryPlatformRestRepository.UsingAuthenticationToken(_apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn"))
                    .Search<MpEventGroupDto>($"Event_Groups.Event_ID = {eventId} AND Event_Room_ID_Table_Room_ID_Table.Room_ID = {roomId}", _eventGroupsColumns);
        }

        public void DeleteEventGroups(string authenticationToken, IEnumerable<int> eventGroupIds)
        {
            var token = authenticationToken ?? _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            _ministryPlatformRestRepository.UsingAuthenticationToken(token).Delete<MpEventGroupDto>(eventGroupIds);
        }

        public List<MpEventGroupDto> CreateEventGroups(string authenticationToken, List<MpEventGroupDto> eventGroups)
        {
            var token = authenticationToken ?? _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            return _ministryPlatformRestRepository.UsingAuthenticationToken(token).Create(eventGroups, _eventGroupsColumns);
        }

        public void ResetEventSetup(int eventId)
        {
            var authenticationToken = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            _ministryPlatformRestRepository.UsingAuthenticationToken(authenticationToken)
                .PostStoredProc(ResetEventStoredProcedureName, new Dictionary<string, object> { { "@EventId", eventId } });
        }

        public void ImportEventSetup(int destinationEventId, int sourceEventId)
        {
            var authenticationToken = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            _ministryPlatformRestRepository.UsingAuthenticationToken(authenticationToken)
                .PostStoredProc(ImportEventStoredProcedureName, new Dictionary<string, object> { { "@DestinationEventId", destinationEventId }, { "@SourceEventId", sourceEventId } });
        }

        public List<MpEventDto> GetEventAndCheckinSubevents(int eventId, bool includeTemplates = false)
        {
            var token = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            var query = $"(Events.Event_ID = {eventId} OR Events.Parent_Event_ID = {eventId}) AND Events.[Allow_Check-in] = 1";
            if (includeTemplates == false)
            {
                query = $"{query} AND {nonEventTemplatesQueryString}";
            }


            return _ministryPlatformRestRepository.UsingAuthenticationToken(token)
                .Search<MpEventDto>(query, _eventColumns);
        }


        public MpEventDto GetSubeventByParentEventId(int serviceEventId, int eventTypeId)
        {
            var token = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");
            var events = _ministryPlatformRestRepository.UsingAuthenticationToken(token)
                 .Search<MpEventDto>($"Events.Parent_Event_ID = {serviceEventId} AND Events.[Event_Type_ID] = {eventTypeId}", _eventColumns);
            return events.FirstOrDefault();
        }

        public List<MpEventDto> GetSubeventsForEvents(List<int> eventIds, int? eventTypeId)
        {
            var apiUserToken = _apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn");

            var queryString = eventIds.Aggregate("(", (current, id) => current + (id + ","));

            queryString = queryString.TrimEnd(',');
            queryString += ")";

            // search on the event type if it's not a null param
            var typeQueryString = (eventTypeId != null) ? " AND Events.[Event_Type_ID] = " + eventTypeId : "";

            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Search<MpEventDto>($"Events.[Parent_Event_ID] IN {queryString} AND Events.[Allow_Check-in] = 1 {typeQueryString}", _eventColumns);
        }

        public List<MpEventGroupDto> GetEventGroupsByGroupIdAndEventIds(int groupId, List<int> eventIds)
        {
            return _ministryPlatformRestRepository.UsingAuthenticationToken(_apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn"))
                .Search<MpEventGroupDto>($"Event_Groups.Group_ID ={groupId} AND Event_Groups.Event_ID IN ({string.Join(",", eventIds)})", _eventGroupsColumns);
        }

        public List<MpCapacityDto> GetCapacitiesForEvent(int eventId)
        {
            var parms = new Dictionary<string, object>
            {
                {"EventID", eventId},
            };

            var result = _ministryPlatformRestRepository.UsingAuthenticationToken(_apiUserRepository.GetApiClientToken("CRDS.Service.SignCheckIn")).GetFromStoredProc<MpCapacityDto>("api_crds_Capacity_App_Data", parms);
            return result[0];
        }
    }
}
