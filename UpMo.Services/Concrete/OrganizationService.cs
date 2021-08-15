using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UpMo.Common.DTO.Request.Organization;
using UpMo.Common.DTO.Response.Organization;
using UpMo.Common.Response;
using UpMo.Data;
using UpMo.Data.Extensions;
using UpMo.Entities;
using UpMo.Services.Abstract;

namespace UpMo.Services.Concrete
{
    public class OrganizationService : BaseService, IOrganizationService
    {
        public OrganizationService(IMapper mapper,
                                   UpMoContext context) : base(mapper, context)
        {
        }

        public async Task<ApiResponse> CreateByRequestAsync(OrganizationRequest request)
        {
            var organization = _mapper.Map<Organization>(request);

            var manager = new Manager
            {
                UserID = organization.CreatorUserID,
                Admin = true,
                Viewer = true
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                await _context.AddAsync(organization);
                manager.OrganizationID = organization.ID;
                await _context.AddAsync(manager);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }

            return new ApiResponse(ResponseStatus.Created, new { organization = _mapper.Map<OrganizationResponse>(organization) });
        }

        public async Task<ApiResponse> UpdateByRequestAsync(OrganizationRequest request)
        {
            var toBeUpdatedOrganization = await _context.Organizations.Include(x => x.Managers)
                                                                      .SingleOrDefaultAsync(x => x.ID == request.ID);
            if (toBeUpdatedOrganization is null)
            {
                return new ApiResponse(ResponseStatus.NotFound, ResponseMessage.NotFoundOrganization);
            }

            if (toBeUpdatedOrganization.CheckCreatorOrAdmin(request.AuthenticatedUserID))
            {
                toBeUpdatedOrganization.Name = request.Name;
                await _context.SaveChangesAsync();

                return new ApiResponse(ResponseStatus.OK, new { organization = _mapper.Map<OrganizationResponse>(toBeUpdatedOrganization) });
            }

            return new ApiResponse(ResponseStatus.Forbid, ResponseMessage.Forbid);
        }

        public async Task<ApiResponse> SoftDeleteByIDAsync(Guid organizationID, int authenticatedUserID)
        {
            var toBeSofDeletedOrganization = await _context.Organizations.SingleOrDefaultAsync(x => x.ID == organizationID);
            if (toBeSofDeletedOrganization is null)
            {
                return new ApiResponse(ResponseStatus.NotFound, ResponseMessage.NotFoundOrganization);
            }

            if (toBeSofDeletedOrganization.CheckCreator(authenticatedUserID))
            {
                toBeSofDeletedOrganization.DeletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return new ApiResponse(ResponseStatus.NoContent);
            }

            return new ApiResponse(ResponseStatus.Forbid, ResponseMessage.Forbid);
        }

        public async Task<ApiResponse> GetOrganizationsByAuthenticatedUserIDAsync(int authenticatedUserID)
        {
            var organizationsForAuthenticatedUser = await _context.Organizations
                                                    .Include(x => x.Managers)
                                                    .Include(x => x.Monitors).ThenInclude(x => x.PostForms)
                                                    .AsSplitQuery()
                                                    .Where(x => x.CreatorUserID == authenticatedUserID
                                                                || x.Managers.Any(x => x.Viewer && x.UserID == authenticatedUserID))
                                                    .ToListAsync();

            object returnObject = new { organizations = _mapper.Map<List<OrganizationResponse>>(organizationsForAuthenticatedUser) };
            return new ApiResponse(ResponseStatus.OK, returnObject);
        }
    }
}