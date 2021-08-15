using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UpMo.Common.DTO.Request.Organization;
using UpMo.Services.Abstract;
using UpMo.WebAPI.Controllers.Base;
using UpMo.WebAPI.Extensions;

namespace UpMo.WebAPI.Controllers
{
    [Route("organizations")]
    public class OrganizationsController : AuthorizeController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService) =>
            _organizationService = organizationService;

        [HttpGet]
        public async Task<IActionResult> GetOrganizationsForAuthenticatedUserAsync() =>
            ApiResponse(await _organizationService.GetOrganizationsByAuthenticatedUserIDAsync(User.GetID()));

        [HttpPost]
        public async Task<IActionResult> CreateAsync(OrganizationRequest request)
        {
            request.AuthenticatedUserID = User.GetID();
            return ApiResponse(await _organizationService.CreateByRequestAsync(request));
        }

        [HttpPut("{organizationID}")]
        public async Task<IActionResult> UpdateAsync(
            Guid organizationID,
            OrganizationRequest request)
        {
            request.ID = organizationID;
            request.AuthenticatedUserID = User.GetID();
            return ApiResponse(await _organizationService.UpdateByRequestAsync(request));
        }

        [HttpDelete("{organizationID}")]
        public async Task<IActionResult> DeleteAsync(Guid organizationID) =>
            ApiResponse(await _organizationService.SoftDeleteByIDAsync(organizationID, User.GetID()));
    }
}