﻿using Crossroads.Utilities.Services.Interfaces;

namespace Crossroads.Utilities.Services
{
    public class ApplicationConfiguration : IApplicationConfiguration
    {
        public int AgesAttributeTypeId { get; }
        public int GradesAttributeTypeId { get; }
        public int BirthMonthsAttributeTypeId { get; }
        public int NurseryAgesAttributeTypeId { get; }
        public int NurseryAgeAttributeId { get; }

        public ApplicationConfiguration(IConfigurationWrapper configurationWrapper)
        {
            AgesAttributeTypeId = configurationWrapper.GetConfigIntValue("KidsClubAgesAttributeTypeId");
            GradesAttributeTypeId = configurationWrapper.GetConfigIntValue("KidsClubGradesAttributeTypeId");
            BirthMonthsAttributeTypeId = configurationWrapper.GetConfigIntValue("KidsClubBirthMonthsAttributeTypeId");
            NurseryAgesAttributeTypeId = configurationWrapper.GetConfigIntValue("KidsClubNurseryAgesAttributeTypeId");
            NurseryAgeAttributeId = configurationWrapper.GetConfigIntValue("KidsClubNurseryAgeAttributeId");
        }
    }
}