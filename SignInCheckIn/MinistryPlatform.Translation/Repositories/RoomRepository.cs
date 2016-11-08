﻿using System.Collections.Generic;
using System.Linq;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly List<string> _eventRoomColumns;
        private readonly List<string> _roomColumnList;
        private readonly List<string> _bumpingRuleColumns; 

        public RoomRepository(IApiUserRepository apiUserRepository,
            IMinistryPlatformRestRepository ministryPlatformRestRepository)
        {
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;

            _eventRoomColumns = new List<string>
            {
                "Event_Rooms.Event_Room_ID",
                "Event_Rooms.Event_ID",
                "Event_Rooms.Room_ID",
                "Room_ID_Table.Room_Name",
                "Room_ID_Table.Room_Number",
                "Event_Rooms.Allow_Checkin",
                "Event_Rooms.Volunteers",
                "Event_Rooms.Capacity",
                "Event_Rooms.Label"
            };

            _roomColumnList = new List<string>
            {
                "Room_ID",
                "Room_Name",
                "Room_Number"
            };

            _bumpingRuleColumns = new List<string>
            {
                "Bumping_Rules_ID",
                "From_Event_Room_ID",
                "To_Event_Room_ID",
                "Priority_Order",
                "cr_Bumping_Rules.Bumping_Rule_Type_ID",
                "Bumping_Rule_Type_ID_Table.[Description]",
                "From_Event_Room_ID_Table_Room_ID_Table.[Room_Name] AS From_Room_Name",
                "From_Event_Room_ID_Table_Room_ID_Table.[Room_Number] AS From_Room_Number",
                "To_Event_Room_ID_Table_Room_ID_Table.[Room_Name] AS To_Room_Name",
                "To_Event_Room_ID_Table_Room_ID_Table.[Room_Number] AS To_Room_Number"
            };
        }

        public List<MpEventRoomDto> GetRoomsForEvent(int eventId, int locationId)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            var rooms = _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Search<MpRoomDto>("Building_ID_Table.Location_ID=" + locationId, _roomColumnList);

            var eventRoomColumnList = new List<string>
            {
                "Event_Room_ID",
                "Event_ID",
                "Room_ID",
                "Capacity",
                "Volunteers",
                "Allow_CheckIn"
            };

            var eventRooms = _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Search<MpEventRoomDto>("Event_ID=" + eventId, eventRoomColumnList);

            foreach (var room in rooms)
            {
                // populate the room data on an existing room event, or add a new event room dto for that room in the return call
                MpEventRoomDto tempDto = eventRooms.FirstOrDefault(r => r.RoomId == room.RoomId);

                if (tempDto == null)
                {
                    // create a new dto and it to the event rooms list, with default values
                    MpEventRoomDto newEventRoomDto = new MpEventRoomDto
                    {
                        AllowSignIn = false,
                        Capacity = 0,
                        CheckedIn = 0,
                        EventId = eventId,
                        EventRoomId = null,
                        RoomId = room.RoomId,
                        RoomName = room.RoomName,
                        SignedIn = 0,
                        Volunteers = 0
                    };

                    eventRooms.Add(newEventRoomDto);
                }
                else
                {
                    // populate room info on room event dto
                    eventRooms.Where(x => x.RoomId == room.RoomId).All(x =>
                    {
                        x.RoomName = room.RoomName;
                        x.RoomNumber = room.RoomNumber;
                        return true;
                    });
                }
            }

            return eventRooms;
        }

        public MpEventRoomDto CreateOrUpdateEventRoom(string authenticationToken, MpEventRoomDto eventRoom)
        {
            MpEventRoomDto response;
            if (eventRoom.EventRoomId.HasValue)
            {
                response = _ministryPlatformRestRepository.UsingAuthenticationToken(authenticationToken).Update(eventRoom, _eventRoomColumns);
            }
            else
            {
                response = _ministryPlatformRestRepository.UsingAuthenticationToken(authenticationToken).Create(eventRoom, _eventRoomColumns);
            }

            return response;
        }

        public MpEventRoomDto GetEventRoom(int eventId, int roomId)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).Search<MpEventRoomDto>($"Event_Rooms.Event_ID = {eventId} AND Event_Rooms.Room_ID = {roomId}", _eventRoomColumns).FirstOrDefault();
        }

        public MpRoomDto GetRoom(int roomId)
        {
            var apiUserToken = _apiUserRepository.GetToken();
            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).Get<MpRoomDto>(roomId, _roomColumnList);
        }

        public List<MpBumpingRuleDto> GetBumpingRulesByRoomId(int fromRoomId)
        {
            var apiUserToken = _apiUserRepository.GetToken();
            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).Search<MpBumpingRuleDto>($"From_Event_Room_ID={fromRoomId}", _bumpingRuleColumns);
        }

        public void UpdateBumpingRules(List<MpBumpingRuleDto> mpBumpingRuleDtos)
        {
            var apiUserToken = _apiUserRepository.GetToken();
            _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken).Update(mpBumpingRuleDtos, _eventRoomColumns);
        }

        public List<MpRoomDto> GetRoomsBySite(int locationId)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Search<MpRoomDto>("Building_ID_Table.Location_ID=" + locationId, _roomColumnList);
        } 
    }
}
