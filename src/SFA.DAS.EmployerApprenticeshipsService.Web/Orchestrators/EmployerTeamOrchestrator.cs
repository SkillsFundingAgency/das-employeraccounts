﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using MediatR;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetAccountTeamMembers;
using SFA.DAS.EmployerApprenticeshipsService.Web.Models;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Orchestrators
{
    public class EmployerTeamOrchestrator
    {
        private readonly IMediator _mediator;

        public EmployerTeamOrchestrator(IMediator mediator)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));
            _mediator = mediator;
        }


        public async Task<EmployerTeamMembersViewModel> GetTeamMembers(int accountId, string userId)
        {
            var accountTeamMemberReponse = await _mediator.SendAsync(new GetAccountTeamMembersQuery { Id = accountId, UserId = userId });
            return new EmployerTeamMembersViewModel { TeamMembers = accountTeamMemberReponse.TeamMembers, AccountId = accountId };
        }
    }
}