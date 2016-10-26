﻿using System;

namespace SignInCheckIn.Models.DTO
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public DateTime EventStartDate { get; set; }
        public string EventType { get; set; }
        public string EventSite { get; set; }
    }
}