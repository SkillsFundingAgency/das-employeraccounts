using System.ComponentModel.DataAnnotations;
using MediatR;
using SFA.DAS.Authorization;

namespace SFA.DAS.EmployerAccounts.Commands.SendTransferConnectionInvitation
{
    public class SendTransferConnectionInvitationCommand : MembershipMessage, IAsyncRequest<long>
    {
        [Required]
        [RegularExpression(Constants.AccountHashedIdRegex)]
        public string ReceiverAccountPublicHashedId { get; set; }
    }
}