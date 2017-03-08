﻿using System.Collections.Generic;
using Crossroads.Utilities.Services.Interfaces;
using Moq;
using NUnit.Framework;
using SignInCheckIn.Models.DTO;
using SignInCheckIn.Services;
using SignInCheckIn.Services.Interfaces;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework.Internal;
using SignInCheckIn.Hubs;

namespace SignInCheckIn.Tests.Services
{
    public class WebsocketServiceTest
    {
        private Mock<IEventService> _eventService;
        private Mock<IApplicationConfiguration> _applicationConfiguration;
        private Mock<IHubContext> _hubContext;
        
        private WebsocketService _fixture;

        private int eventId = 123;
        private int roomId = 456;


        [SetUp]
        public void SetUp()
        {
            AutoMapperConfig.RegisterMappings();

            _eventService = new Mock<IEventService>(MockBehavior.Strict);
            _applicationConfiguration = new Mock<IApplicationConfiguration>();
            
            _applicationConfiguration.Setup(ac => ac.CheckinParticipantsChannel).Returns("CheckinParticipantsChannel");
            _applicationConfiguration.Setup(ac => ac.CheckinCapacityChannel).Returns("CheckinCapacityChannel");
            _applicationConfiguration.Setup(ac => ac.AdventureClubEventTypeId).Returns(20);

            var eventDto = new EventDto()
            {
                ParentEventId = 678,
                EventId = 123,
                EventTypeId = 7 // not Adventure Club
            };
            _eventService.Setup(m => m.GetEvent(It.IsAny<int>())).Returns(eventDto);
            _hubContext = new Mock<IHubContext>();

            _fixture = new WebsocketService(_eventService.Object, _applicationConfiguration.Object, _hubContext.Object);
        }

        [Test]
        public void ShouldPublishCheckinCapacity()
        {
            const string expectedChannelName = "CheckinParticipantsChannel123456";
            var data = new List<ParticipantDto>();
            var mock = new Mock<IDependance>();
            mock.Setup(m => m.OnEvent(expectedChannelName, It.IsAny<ChannelEvent>()));
            var mockIHubConnectionContext = new Mock<IHubConnectionContext<dynamic>>();
            mockIHubConnectionContext.Setup(hc => hc.Group(expectedChannelName)).Returns(mock.Object);
            _hubContext.SetupGet(hc => hc.Clients).Returns(mockIHubConnectionContext.Object);

            _fixture.PublishCheckinParticipantsAdd(eventId, roomId, data);

            mock.Verify();
            mockIHubConnectionContext.Verify();
        }

        [Test]
        public void ShouldPublishCheckinParticipantsCheckedIn()
        {
            const string expectedChannelName = "CheckinParticipantsChannel123456";
            var data = new ParticipantDto();
            var mock = new Mock<IDependance>();
            mock.Setup(m => m.OnEvent(expectedChannelName, It.IsAny<ChannelEvent>()));
            var mockIHubConnectionContext = new Mock<IHubConnectionContext<dynamic>>();
            mockIHubConnectionContext.Setup(hc => hc.Group(expectedChannelName)).Returns(mock.Object);
            _hubContext.SetupGet(hc => hc.Clients).Returns(mockIHubConnectionContext.Object);

            _fixture.PublishCheckinParticipantsCheckedIn(eventId, roomId, data);

            mock.Verify();
            mockIHubConnectionContext.Verify();
        }

        [Test]
        public void ShouldPublishCheckinParticipantsAdd()
        {
            const string expectedChannelName = "CheckinParticipantsChannel123456";
            var data = new List<ParticipantDto>();
            var mock = new Mock<IDependance>();
            mock.Setup(m => m.OnEvent(expectedChannelName, It.IsAny<ChannelEvent>()));
            var mockIHubConnectionContext = new Mock<IHubConnectionContext<dynamic>>();
            mockIHubConnectionContext.Setup(hc => hc.Group(expectedChannelName)).Returns(mock.Object);
            _hubContext.SetupGet(hc => hc.Clients).Returns(mockIHubConnectionContext.Object);

            _fixture.PublishCheckinParticipantsAdd(eventId, roomId, data);

            mock.Verify();
            mockIHubConnectionContext.Verify();
        }

        [Test]
        public void ShouldPublishCheckinParticipantsRemove()
        {
            const string expectedChannelName = "CheckinParticipantsChannel123456";
            var data = new ParticipantDto();
            var mock = new Mock<IDependance>();
            mock.Setup(m => m.OnEvent(expectedChannelName, It.IsAny<ChannelEvent>()));
            var mockIHubConnectionContext = new Mock<IHubConnectionContext<dynamic>>();
            mockIHubConnectionContext.Setup(hc => hc.Group(expectedChannelName)).Returns(mock.Object);
            _hubContext.SetupGet(hc => hc.Clients).Returns(mockIHubConnectionContext.Object);

            _fixture.PublishCheckinParticipantsRemove(eventId, roomId, data);

            mock.Verify();
            mockIHubConnectionContext.Verify();
        }

        [Test]
        public void ShouldPublishCheckinParticipantsOverrideCheckin()
        {
            const string expectedChannelName = "CheckinParticipantsChannel123456";
            var data = new ParticipantDto();
            var mock = new Mock<IDependance>();
            mock.Setup(m => m.OnEvent(expectedChannelName, It.IsAny<ChannelEvent>()));
            var mockIHubConnectionContext = new Mock<IHubConnectionContext<dynamic>>();
            mockIHubConnectionContext.Setup(hc => hc.Group(expectedChannelName)).Returns(mock.Object);
            _hubContext.SetupGet(hc => hc.Clients).Returns(mockIHubConnectionContext.Object);

            _fixture.PublishCheckinParticipantsOverrideCheckin(eventId, roomId, data);

            mock.Verify();
            mockIHubConnectionContext.Verify();
        }

        [Test]
        public void ShouldPublishByParentEventId()
        {
            Assert.Fail();
        }

    }

    public interface IDependance
    {
        string OnEvent(string channelName, ChannelEvent channelEvent);
    }
}
