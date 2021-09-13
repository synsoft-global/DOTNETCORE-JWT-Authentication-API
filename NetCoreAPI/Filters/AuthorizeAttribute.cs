using System;
using System.Linq;
using System.Net;
using NetCoreDAL;
using NetCoreDAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NetCoreAPI.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// This method is used for validate user (It is authorized or token is expired)
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = (User)context.HttpContext.Items["User"];
            var flag = Convert.ToBoolean(context.HttpContext.Items["IsExpired"]);
            if (user == null || user.userID <= 0)
            {
                if (flag == true)
                {                    
                    context.Result = new JsonResult(new Response
                    {
                        Status = (int)HttpStatusCode.Unauthorized,
                        Message = "The access token is expired.",
                        Data = Enumerable.Empty<dynamic>()
                    })
                    { StatusCode = StatusCodes.Status401Unauthorized };
                }
                else
                {                    
                    context.Result = new JsonResult(new Response
                    {
                        Status = (int)HttpStatusCode.Unauthorized,
                        Message = "The access token is missing or invalid.",
                        Data = Enumerable.Empty<dynamic>()
                    })
                    { StatusCode = StatusCodes.Status401Unauthorized };
                }
            }
        }
    }
}
