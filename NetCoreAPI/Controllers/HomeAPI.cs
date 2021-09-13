using System;
using System.Net;
using NetCoreDAL;
using NetCoreAPI.Helper;
using NetCoreAPI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NetCoreAPI.Filters;
using NetCoreBLL.Interface;

namespace NetCoreAPI.Controllers
{
    [Route("api/home")]
    [ApiController, Authorize]
    public class HomeAPI : ControllerBase
    {
        private readonly IStringLocalizer<MessageResource> _localizer;
        private readonly IHome _home;
        private readonly ILogger<HomeAPI> _logger;

        public HomeAPI(ILogger<HomeAPI> logger, IHome home, IStringLocalizer<MessageResource> localizer)
        {
            _home = home;
            _localizer = localizer;
            _logger = logger;
        }

        /// <summary>
        /// This method is used for get users details
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Route("users")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var response = await _home.GetUsers();
                if (response.Status == Convert.ToInt32(HttpStatusCode.OK))
                {
                    response.Message = _localizer["getUserSucc"].Value;
                    return StatusCode(response.Status, response);
                }
                else
                {
                    response.Message = _localizer["getUserFailed"].Value;
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
