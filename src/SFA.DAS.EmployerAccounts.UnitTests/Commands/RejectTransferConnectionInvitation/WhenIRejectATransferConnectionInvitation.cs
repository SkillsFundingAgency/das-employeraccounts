﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.RejectTransferConnectionInvitation;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.TransferConnections;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.UnitTests.Builders;
using SFA.DAS.UnitOfWork;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.RejectTransferConnectionInvitation
{
    [TestFixture]
    public class WhenIRejectATransferConnectionInvitation
    {
        private RejectTransferConnectionInvitationCommandHandler _handler;
        private RejectTransferConnectionInvitationCommand _command;
        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        private Mock<ITransferConnectionInvitationRepository> _transferConnectionInvitationRepository;
        private Mock<IUserAccountRepository> _userRepository;
        private TransferConnectionInvitation _transferConnectionInvitation;
        private UnitOfWorkContext _unitOfWorkContext;
        private Account _senderAccount;
        private Account _receiverAccount;
        private User _receiverUser;

        [SetUp]
        public void Arrange()
        {
            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();
            _userRepository = new Mock<IUserAccountRepository>();

            _receiverUser = new User
            {
                Id = 123456,
                Ref = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe"
            };

            _senderAccount = new Account
            {
                Id = 333333,
                Name = "Sender"
            };

            _receiverAccount = new Account
            {
                Id = 222222,
                Name = "Receiver"
            };

            _unitOfWorkContext = new UnitOfWorkContext();

            _transferConnectionInvitation = new TransferConnectionInvitationBuilder()
                .WithId(111111)
                .WithSenderAccount(_senderAccount)
                .WithReceiverAccount(_receiverAccount)
                .WithStatus(TransferConnectionInvitationStatus.Pending)
                .Build();
            
            _userRepository.Setup(r => r.GetUserByRef(_receiverUser.Ref)).ReturnsAsync(_receiverUser);
            _employerAccountRepository.Setup(r => r.GetAccountById(_senderAccount.Id)).ReturnsAsync(_senderAccount);
            _employerAccountRepository.Setup(r => r.GetAccountById(_receiverAccount.Id)).ReturnsAsync(_receiverAccount);
            _transferConnectionInvitationRepository.Setup(r => r.GetTransferConnectionInvitationById(_transferConnectionInvitation.Id)).ReturnsAsync(_transferConnectionInvitation);

            _handler = new RejectTransferConnectionInvitationCommandHandler(
                _employerAccountRepository.Object,
                _transferConnectionInvitationRepository.Object,
                _userRepository.Object
            );

            _command = new RejectTransferConnectionInvitationCommand
            {
                AccountId = _receiverAccount.Id,
                UserRef = _receiverUser.Ref,
                TransferConnectionInvitationId = _transferConnectionInvitation.Id
            };
        }

        [Test]
        public async Task ThenShouldRejectTransferConnectionInvitation()
        {
            var now = DateTime.UtcNow;

            await _handler.Handle(_command);

            Assert.That(_transferConnectionInvitation.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
            Assert.That(_transferConnectionInvitation.Changes.Count, Is.EqualTo(1));

            var change = _transferConnectionInvitation.Changes.Single();

            Assert.That(change.CreatedDate, Is.GreaterThanOrEqualTo(now));
            Assert.That(change.DeletedByReceiver, Is.Null);
            Assert.That(change.DeletedBySender, Is.Null);
            Assert.That(change.ReceiverAccount, Is.Null);
            Assert.That(change.SenderAccount, Is.Null);
            Assert.That(change.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
            Assert.That(change.User, Is.Not.Null);
            Assert.That(change.User.Id, Is.EqualTo(_receiverUser.Id));
        }

        [Test]
        public async Task ThenShouldPublishRejectedTransferConnectionInvitationEvent()
        {
            await _handler.Handle(_command);

            var messages = _unitOfWorkContext.GetEvents().ToList();
            var message = messages.OfType<RejectedTransferConnectionRequestEvent>().FirstOrDefault();

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Created, Is.EqualTo(_transferConnectionInvitation.Changes.Select(c => c.CreatedDate).Cast<DateTime?>().SingleOrDefault()));
            Assert.That(message.ReceiverAccountHashedId, Is.EqualTo(_receiverAccount.HashedId));
            Assert.That(message.ReceiverAccountId, Is.EqualTo(_receiverAccount.Id));
            Assert.That(message.ReceiverAccountName, Is.EqualTo(_receiverAccount.Name));
            Assert.That(message.RejectorUserRef, Is.EqualTo(_receiverUser.Ref));
            Assert.That(message.RejectorUserId, Is.EqualTo(_receiverUser.Id));
            Assert.That(message.RejectorUserName, Is.EqualTo(_receiverUser.FullName));
            Assert.That(message.SenderAccountHashedId, Is.EqualTo(_senderAccount.HashedId));
            Assert.That(message.SenderAccountId, Is.EqualTo(_senderAccount.Id));
            Assert.That(message.SenderAccountName, Is.EqualTo(_senderAccount.Name));
            Assert.That(message.TransferConnectionRequestId, Is.EqualTo(_transferConnectionInvitation.Id));
        }

        [Test]
        public void ThenShouldThrowExceptionIfRejectorIsNotTheReceiver()
        {
            _command.AccountId = _senderAccount.Id;

            Assert.ThrowsAsync<Exception>(() => _handler.Handle(_command), "Requires rejector account is the receiver account.");
        }

        [Test]
        public void ThenShouldThrowExceptionIfTransferConnectionInvitationIsNotPending()
        {
            _transferConnectionInvitation = new TransferConnectionInvitationBuilder()
                .WithId(111111)
                .WithSenderAccount(_senderAccount)
                .WithReceiverAccount(_receiverAccount)
                .WithStatus(TransferConnectionInvitationStatus.Rejected)
                .Build();

            _transferConnectionInvitationRepository.Setup(r => r.GetTransferConnectionInvitationById(_transferConnectionInvitation.Id)).ReturnsAsync(_transferConnectionInvitation);

            Assert.ThrowsAsync<Exception>(() => _handler.Handle(_command), "Requires transfer connection invitation is pending.");
        }
    }
}