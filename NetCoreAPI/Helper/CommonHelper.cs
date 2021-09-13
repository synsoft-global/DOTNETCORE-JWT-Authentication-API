using System;
using Microsoft.AspNetCore.Http;

namespace NetCoreAPI.Helper
{
    public static class CommonHelper
    {
        public static string GetAbsoluteUri(HttpContext httpContext)
        {
            UriBuilder _uriBuilder = new UriBuilder();
            var request = httpContext.Request;
            _uriBuilder.Scheme = request.Scheme;
            _uriBuilder.Host = request.Host.Host;
            _uriBuilder.Path = request.Path.ToString();
            _uriBuilder.Query = request.QueryString.ToString();
            return _uriBuilder.Uri.AbsoluteUri;
        }
    }
}
