﻿using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using SignInCheckIn.ApiVersioning;
using SignInCheckIn.ApiVersioning.Filters;
using SignInCheckIn.Filters;

namespace SignInCheckIn
{
    public static class WebApiConfig
    {
        public static ApiRouteProvider ApiRouteProvider = new ApiRouteProvider();

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Configure CORS
            var cors = new EnableCorsAttribute(ConfigurationManager.AppSettings["CORS"], "*", "*");
            cors.SupportsCredentials = true;
            config.EnableCors(cors);

            // Web API routes - use a custom route provider to prepend "api/" on all routes
            config.MapHttpAttributeRoutes(ApiRouteProvider);

            // Add a filter to change 200 to 299 response code for deprecated APIs
            config.Filters.Add(new DeprecatedVersionFilter());

            // Add a filter to verify domain-locked API keys
            var domainLockedApiKeyFilter = (DomainLockedApiKeyFilter)config.DependencyResolver.GetService(typeof(DomainLockedApiKeyFilter));
            config.Filters.Add(domainLockedApiKeyFilter);
        }
    }
}