﻿using System.Web.Http;
using SignInCheckIn.ApiVersioning;
using SignInCheckIn.ApiVersioning.Filters;

namespace SignInCheckIn
{
    public static class WebApiConfig
    {
        public static ApiRouteProvider ApiRouteProvider = new ApiRouteProvider();

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes - use a custom route provider to prepend "api/" on all routes
            config.MapHttpAttributeRoutes(ApiRouteProvider);

            // Add a filter to change 200 to 299 response code for deprecated APIs
            config.Filters.Add(new DeprecatedVersionFilter());
        }
    }
}
