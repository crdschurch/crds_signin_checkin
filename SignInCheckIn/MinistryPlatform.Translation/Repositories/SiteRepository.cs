﻿using System;
using System.Collections.Generic;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class SiteRepository : ISiteRepository
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public SiteRepository(IApiUserRepository apiUserRepository,
            IMinistryPlatformRestRepository ministryPlatformRestRepository)
        {
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;   
        }

        public List<MpCongregationDto> GetAll()
        {
            var apiUserToken = _apiUserRepository.GetToken();

            var contactColumnList = new List<string>
            {
                "Congregation_ID",
                "Congregation_Name"
            };

            var congregations = _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Search<MpCongregationDto>($"Available_Online = 1 AND (End_Date IS NULL OR End_Date > '{DateTime.Now:yyyy-MM-dd H:mm:ss}')", contactColumnList);

            return congregations;
        }
    }
}
