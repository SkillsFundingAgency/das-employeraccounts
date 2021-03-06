using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.Models.TransferConnections;

namespace SFA.DAS.EmployerAccounts.Queries.GetLatestPendingReceivedTransferConnectionInvitation
{
    public class GetLatestPendingReceivedTransferConnectionInvitationQueryHandler : IAsyncRequestHandler<GetLatestPendingReceivedTransferConnectionInvitationQuery, GetLatestPendingReceivedTransferConnectionInvitationResponse>
    {
        private readonly Lazy<EmployerAccountsDbContext> _db;
        private readonly IConfigurationProvider _configurationProvider;

        public GetLatestPendingReceivedTransferConnectionInvitationQueryHandler(Lazy<EmployerAccountsDbContext> db, IConfigurationProvider configurationProvider)
        {
            _db = db;
            _configurationProvider = configurationProvider;
        }

        public async Task<GetLatestPendingReceivedTransferConnectionInvitationResponse> Handle(GetLatestPendingReceivedTransferConnectionInvitationQuery message)
        {
            var transferConnectionInvitation = await _db.Value.TransferConnectionInvitations
                .Where(i => i.ReceiverAccountId == message.AccountId && i.Status == TransferConnectionInvitationStatus.Pending)
                .OrderByDescending(i => i.CreatedDate)
                .ProjectTo<TransferConnectionInvitationDto>(_configurationProvider)
                .FirstOrDefaultAsync();

            return new GetLatestPendingReceivedTransferConnectionInvitationResponse
            {
                TransferConnectionInvitation = transferConnectionInvitation
            };
        }
    }
}