﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinistryPlatform.Translation.Models.DTO;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public ContactRepository(IApiUserRepository apiUserRepository, IMinistryPlatformRestRepository ministryPlatformRestRepository)
        {
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
        }

        public List<MpContactDto> GetHeadsOfHouseholdByHouseholdId(int householdId)
        {
            var apiUserToken = _apiUserRepository.GetToken();

            var contactColumnList = new List<string>
            {
                "Contact_ID",
                "Contacts.Household_ID",
                "Contacts.Household_Position_ID",
                "Household_ID_Table.Home_Phone",
                "Mobile_Phone",
                "Nickname",
                "Last_Name"
            };

            var contacts = _ministryPlatformRestRepository.UsingAuthenticationToken(apiUserToken)
                .Search<MpContactDto>($"Contacts.Household_ID={householdId} AND Contacts.Household_Position_ID IN (1, 7)", contactColumnList);

            return contacts;
        }

        public MpHouseholdDto CreateHousehold(string token, MpHouseholdDto mpHouseholdDto)
        {
            List<string> householdColumns = new List<string>
            {
                "Households.Household_ID",
                "Households.Home_Phone",
                "Households.Household_Name"
            };

           return _ministryPlatformRestRepository.UsingAuthenticationToken(token).Create(mpHouseholdDto, householdColumns);
        }
    }
}
