﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
//using Crossroads.ClientApiKeys;

namespace SignInCheckIn
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            UnityConfig.RegisterComponents();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AutoMapperConfig.RegisterMappings();

            log4net.Config.XmlConfigurator.Configure();  // must be done before ClientApiKeys
            //DomainLockedClientApiKeyConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}