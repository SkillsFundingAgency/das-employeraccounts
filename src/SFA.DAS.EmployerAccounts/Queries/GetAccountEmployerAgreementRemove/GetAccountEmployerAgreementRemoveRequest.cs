using MediatR;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreementRemove
{
    public class GetAccountEmployerAgreementRemoveRequest : IAsyncRequest<GetAccountEmployerAgreementRemoveResponse>
    {
        public string HashedAccountId { get; set; }
        public string UserId { get; set; }
        public string HashedAgreementId { get; set; }
    }
}