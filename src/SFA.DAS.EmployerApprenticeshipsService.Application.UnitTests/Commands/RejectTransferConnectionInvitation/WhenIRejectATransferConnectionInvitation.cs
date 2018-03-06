﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Application.Commands.RejectTransferConnectionInvitation;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Models;
using SFA.DAS.EAS.Domain.Models.TransferConnections;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.HashingService;

namespace SFA.DAS.EAS.Application.UnitTests.Commands.RejectTransferConnectionInvitation
{
    [TestFixture]
    public class WhenIRejectATransferConnectionInvitation
    {
        private RejectTransferConnectionInvitationCommandHandler _handler;
        private RejectTransferConnectionInvitationCommand _command;
        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        private Mock<IHashingService> _hashingService;
        private Mock<ITransferConnectionInvitationRepository> _transferConnectionInvitationRepository;
        private Mock<IUserRepository> _userRepository;
        private TransferConnectionInvitation _transferConnectionInvitation;
        private IEntity _entity;
        private Domain.Data.Entities.Account.Account _senderAccount;
        private Domain.Data.Entities.Account.Account _receiverAccount;
        private User _receiverUser;

        [SetUp]
        public void Arrange()
        {
            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _hashingService = new Mock<IHashingService>();
            _transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();
            _userRepository = new Mock<IUserRepository>();

            _receiverUser = new User
            {
                ExternalId = Guid.NewGuid(),
                Id = 123456,
                FirstName = "John",
                LastName = "Doe"
            };

            _senderAccount = new Domain.Data.Entities.Account.Account
            {
                Id = 333333,
                Name = "Sender"
            };

            _receiverAccount = new Domain.Data.Entities.Account.Account
            {
                Id = 222222,
                Name = "Receiver"
            };

            _entity = _transferConnectionInvitation = new TransferConnectionInvitation
            {
                Id = 111111,
                SenderAccount = _senderAccount,
                ReceiverAccount = _receiverAccount,
                Status = TransferConnectionInvitationStatus.Pending
            };

            _hashingService.Setup(h => h.DecodeValue(_receiverAccount.HashedId)).Returns(_receiverAccount.Id);
            _hashingService.Setup(h => h.DecodeValue(_senderAccount.HashedId)).Returns(_senderAccount.Id);
            _userRepository.Setup(r => r.GetUserById(_receiverUser.Id)).ReturnsAsync(_receiverUser);
            _employerAccountRepository.Setup(r => r.GetAccountById(_senderAccount.Id)).ReturnsAsync(_senderAccount);
            _employerAccountRepository.Setup(r => r.GetAccountById(_receiverAccount.Id)).ReturnsAsync(_receiverAccount);
            _transferConnectionInvitationRepository.Setup(r => r.GetTransferConnectionInvitationToApproveOrReject(_transferConnectionInvitation.Id, _receiverAccount.Id)).ReturnsAsync(_transferConnectionInvitation);

            _handler = new RejectTransferConnectionInvitationCommandHandler(
                _employerAccountRepository.Object,
                _transferConnectionInvitationRepository.Object,
                _userRepository.Object
            );

            _command = new RejectTransferConnectionInvitationCommand
            {
                AccountId = _receiverAccount.Id,
                UserId = _receiverUser.Id,
                TransferConnectionInvitationId = _transferConnectionInvitation.Id
            };
        }

        [Test]
        public async Task ThenShouldGetReceiversAccount()
        {
            await _handler.Handle(_command);

            _employerAccountRepository.Verify(r => r.GetAccountById(_receiverAccount.Id), Times.Once);
        }

        [Test]
        public async Task ThenShouldGetUser()
        {
            await _handler.Handle(_command);

            _userRepository.Verify(r => r.GetUserById(_receiverUser.Id), Times.Once);
        }

        [Test]
        public async Task ThenShouldGetTransferConnectionInvitation()
        {
            await _handler.Handle(_command);

            _transferConnectionInvitationRepository.Verify(r => r.GetTransferConnectionInvitationToApproveOrReject(_transferConnectionInvitation.Id, _receiverAccount.Id), Times.Once);
        }

        [Test]
        public async Task ThenShouldRejectTransferConnectionInvitation()
        {
            var now = DateTime.UtcNow;

            await _handler.Handle(_command);

            Assert.That(_transferConnectionInvitation.RejectorUser.Id, Is.EqualTo(_receiverUser.Id));
            Assert.That(_transferConnectionInvitation.Status, Is.EqualTo(TransferConnectionInvitationStatus.Rejected));
            Assert.That(_transferConnectionInvitation.ModifiedDate, Is.GreaterThanOrEqualTo(now));
        }

        [Test]
        public async Task ThenShouldPublishTransferConnectionInvitationRejectedMessage()
        {
            await _handler.Handle(_command);

            var messages = _entity.GetEvents().ToList();
            var message = messages.OfType<TransferConnectionInvitationRejectedMessage>().FirstOrDefault();

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(message, Is.Not.Null);
            Assert.That(message.TransferConnectionInvitationId, Is.EqualTo(_transferConnectionInvitation.Id));
            Assert.That(message.SenderAccountId, Is.EqualTo(_senderAccount.Id));
            Assert.That(message.SenderAccountName, Is.EqualTo(_senderAccount.Name));
            Assert.That(message.ReceiverAccountId, Is.EqualTo(_receiverAccount.Id));
            Assert.That(message.ReceiverAccountName, Is.EqualTo(_receiverAccount.Name));
            Assert.That(message.RejectorUserName, Is.EqualTo(_receiverUser.FullName));
            Assert.That(message.RejectorUserExternalId, Is.EqualTo(_receiverUser.ExternalId));
            Assert.That(message.CreatedAt, Is.EqualTo(_transferConnectionInvitation.ModifiedDate));
        }
    }
}