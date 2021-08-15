using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpMo.Common.DTO.Request.Monitor;
using UpMo.Services.Abstract;
using UpMo.WebAPI.Controllers.Base;
using UpMo.WebAPI.Extensions;

namespace UpMo.WebAPI.Controllers
{
    [Route("/organizations/{organizationID}/monitors/{monitorID}/postdata")]
    public class PostDataController : AuthorizeController
    {
        private readonly IMonitorPostDataService _postDataService;

        public PostDataController(IMonitorPostDataService postDataService) =>
            _postDataService = postDataService;

        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromRoute] Guid organizationID,
            [FromRoute] Guid monitorID,
            [FromBody] PostFormDataCreateRequest request)
        {
            request.MonitorID = monitorID;
            request.AuthenticatedUserID = User.GetID();
            return ApiResponse(await _postDataService.CreateByRequestAsync(request));
        }
    }
}