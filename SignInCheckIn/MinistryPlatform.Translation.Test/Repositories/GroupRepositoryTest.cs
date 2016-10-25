﻿using System.Collections.Generic;
using Crossroads.Utilities.Services.Interfaces;
using FluentAssertions;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;
using MinistryPlatform.Translation.Models.DTO;

namespace MinistryPlatform.Translation.Test.Repositories
{
    public class GroupRepositoryTest
    {
        private GroupRepository _fixture;

        private Mock<IApiUserRepository> _apiUserRepository;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private Mock<IApplicationConfiguration> _applicationConfiguration;

        private List<string> _attributeColumns;
        private List<string> _groupColumns;

        [SetUp]
        public void SetUp()
        {
            _apiUserRepository = new Mock<IApiUserRepository>(MockBehavior.Strict);
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>(MockBehavior.Strict);
            _applicationConfiguration = new Mock<IApplicationConfiguration>(MockBehavior.Strict);
            _applicationConfiguration.SetupGet(mocked => mocked.AgesAttributeTypeId).Returns(123);
            _applicationConfiguration.SetupGet(mocked => mocked.BirthMonthsAttributeTypeId).Returns(456);
            _applicationConfiguration.SetupGet(mocked => mocked.GradesAttributeTypeId).Returns(789);
            _applicationConfiguration.SetupGet(mocked => mocked.NurseryAgeAttributeId).Returns(234);
            _applicationConfiguration.SetupGet(mocked => mocked.NurseryAgesAttributeTypeId).Returns(567);

            _attributeColumns = new List<string>
            {
                "Attribute_ID_Table.[Attribute_ID]",
                "Attribute_ID_Table.[Attribute_Name]",
                "Attribute_ID_Table.[Sort_Order]",
                "Attribute_ID_Table_Attribute_Type_ID_Table.[Attribute_Type_ID]",
                "Attribute_ID_Table_Attribute_Type_ID_Table.[Attribute_Type]"
            };

            _groupColumns = new List<string>
            {
                "Groups.Group_ID",
                "Groups.Group_Name"
            };

            _fixture = new GroupRepository(_apiUserRepository.Object, _ministryPlatformRestRepository.Object, _applicationConfiguration.Object);
        }

        [Test]
        public void TestGetGroupNoAttrsWithToken()
        {
            const string token = "token 123";
            const int groupId = 456;
            var group = new MpGroupDto();

            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Get<MpGroupDto>(groupId, _groupColumns)).Returns(group);

            var result = _fixture.GetGroup(token, groupId, false);
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            result.Should().BeSameAs(group);
        }

        [Test]
        public void TestGetGroupNoAttrsWithoutToken()
        {
            const string token = "token 123";
            const int groupId = 456;
            var group = new MpGroupDto();

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns(token);
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Get<MpGroupDto>(groupId, _groupColumns)).Returns(group);

            var result = _fixture.GetGroup(null, groupId, false);
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            result.Should().BeSameAs(group);
        }

        [Test]
        public void TestGetGroupWithAttrsWithToken()
        {
            const string token = "token 123";
            const int groupId = 456;
            var group = new MpGroupDto
            {
                Id = 789
            };

            var attrs = new List<MpAttributeDto>
            {
                new MpAttributeDto
                {
                    Id = 1000,
                    Type = new MpAttributeTypeDto
                    {
                        Id = 123
                    }
                },
                new MpAttributeDto
                {
                    Id = 2000,
                    Type = new MpAttributeTypeDto
                    {
                        Id = 456
                    }
                },
                new MpAttributeDto
                {
                    Id = 3000,
                    Type = new MpAttributeTypeDto
                    {
                        Id = 789
                    }
                },
                new MpAttributeDto
                {
                    Id = 4000,
                    Type = new MpAttributeTypeDto
                    {
                        Id = 567
                    }
                },
            };

            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Get<MpGroupDto>(groupId, _groupColumns)).Returns(group);
            _ministryPlatformRestRepository.Setup(mocked => mocked.SearchTable<MpAttributeDto>("Group_Attributes", $"Group_Attributes.Group_ID = {group.Id}", _attributeColumns))
                .Returns(attrs);

            var result = _fixture.GetGroup(token, groupId, true);
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            result.Should().BeSameAs(group);
            result.HasAgeRange().Should().BeTrue();
            result.AgeRange.Should().BeSameAs(attrs[0]);
            result.HasBirthMonth().Should().BeTrue();
            result.BirthMonth.Should().BeSameAs(attrs[1]);
            result.HasGrade().Should().BeTrue();
            result.Grade.Should().BeSameAs(attrs[2]);
            result.HasNurseryMonth().Should().BeTrue();
            result.NurseryMonth.Should().BeSameAs(attrs[3]);
        }

        [Test]
        public void TestGetGroupWithAttrsWithoutToken()
        {
            const string token = "token 123";
            const int groupId = 456;
            var group = new MpGroupDto
            {
                Id = 789
            };

            var attrs = new List<MpAttributeDto>
            {
                new MpAttributeDto
                {
                    Id = 1000,
                    Type = new MpAttributeTypeDto
                    {
                        Id = 123
                    }
                },
                new MpAttributeDto
                {
                    Id = 2000,
                    Type = new MpAttributeTypeDto
                    {
                        Id = 456
                    }
                },
                new MpAttributeDto
                {
                    Id = 3000,
                    Type = new MpAttributeTypeDto
                    {
                        Id = 789
                    }
                },
                new MpAttributeDto
                {
                    Id = 4000,
                    Type = new MpAttributeTypeDto
                    {
                        Id = 567
                    }
                },
            };

            _apiUserRepository.Setup(mocked => mocked.GetToken()).Returns(token);
            _ministryPlatformRestRepository.Setup(mocked => mocked.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(mocked => mocked.Get<MpGroupDto>(groupId, _groupColumns)).Returns(group);
            _ministryPlatformRestRepository.Setup(mocked => mocked.SearchTable<MpAttributeDto>("Group_Attributes", $"Group_Attributes.Group_ID = {group.Id}", _attributeColumns))
                .Returns(attrs);

            var result = _fixture.GetGroup(null, groupId, true);
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            result.Should().BeSameAs(group);
            result.HasAgeRange().Should().BeTrue();
            result.AgeRange.Should().BeSameAs(attrs[0]);
            result.HasBirthMonth().Should().BeTrue();
            result.BirthMonth.Should().BeSameAs(attrs[1]);
            result.HasGrade().Should().BeTrue();
            result.Grade.Should().BeSameAs(attrs[2]);
            result.HasNurseryMonth().Should().BeTrue();
            result.NurseryMonth.Should().BeSameAs(attrs[3]);
        }
    }
}