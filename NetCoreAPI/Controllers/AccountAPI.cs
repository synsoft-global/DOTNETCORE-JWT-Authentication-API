using System;
using System.Net;
using System.Linq;
using NetCoreDAL;
using NetCoreDAL.Models;
using NetCoreAPI.Resources;
using NetCoreBLL.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NetCoreAPI.Helper;

namespace NetCoreAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AccountAPI : ControllerBase
    {
        private readonly IStringLocalizer<MessageResource> _localizer;
        private readonly IAccount _account;
        private readonly ILogger<AccountAPI> _logger;

        public AccountAPI(IAccount account, ILogger<AccountAPI> logger,
            IStringLocalizer<MessageResource> localizer)
        {
            _account = account;
            _localizer = localizer;
            _logger = logger;
        }

        /// <summary>
        /// This method is used for user sign-in & generat jwt token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("token")]
        [HttpPost]
        public async Task<IActionResult> SignIn(SignIn signIn)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _account.SignIn(signIn);
                    if (response.Status == Convert.ToInt32(HttpStatusCode.Created))
                    {
                        response.Message = _localizer["loginSucc"].Value;
                        return StatusCode(response.Status, response);
                    }
                    else
                    {
                        response.Message = _localizer["invalidCredentials"].Value;
                        return StatusCode(response.Status, response);
                    }
                }
                else
                {
                    var Errors = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage).ToList();
                    var response = new Response
                    {
                        Status = Convert.ToInt32(HttpStatusCode.UnprocessableEntity),
                        Message = Errors.ToString(),
                        Data = Enumerable.Empty<dynamic>()
                    };
                    return StatusCode(response.Status, response);
                }
            }
            catch (Exception ex)
            {
                var absoluteUri = CommonHelper.GetAbsoluteUri(HttpContext);
                _logger.LogError(ex, _localizer["dbError"].Value + absoluteUri, 0);
                var response = new Response
                {
                    Status = Convert.ToInt32(HttpStatusCode.InternalServerError),
                    Message = _localizer["exception"].Value,
                    Data = Enumerable.Empty<dynamic>(),
                };
                return StatusCode(response.Status, response);
            }
        }

        /// <summary>
        /// This method is used for user sign-up & generat jwt token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("signup")]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUp signUp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = await _account.SignUp(signUp);
                    if (string.IsNullOrEmpty(response.Message))
                    {
                        response.Message = _localizer["addUserFailed"].Value;
                    }
                    return StatusCode(response.Status, response);
                }
                else
                {
                    var Errors = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage).ToList();
                    var response = new Response
                    {
                        Status = Convert.ToInt32(HttpStatusCode.UnprocessableEntity),
                        Message = Errors.ToString(),
                        Data = null
                    };
                    return StatusCode(response.Status, response);
                }
            }
            catch (Exception ex)
            {
                var absoluteUri = CommonHelper.GetAbsoluteUri(HttpContext);
                _logger.LogError(ex, _localizer["dbError"].Value + absoluteUri, 0);
                var response = new Response
                {
                    Status = Convert.ToInt32(HttpStatusCode.InternalServerError),
                    Message = _localizer["exception"].Value,
                    Data = null,
                };
                return StatusCode(response.Status, response);
            }
        }
    }
}
