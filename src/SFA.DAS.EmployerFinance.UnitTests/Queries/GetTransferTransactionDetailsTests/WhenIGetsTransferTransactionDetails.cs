﻿using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetTransferTransactionDetails;
using SFA.DAS.Hashing;
using SFA.DAS.Testing.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetTransferTransactionDetailsTests
{
    class WhenAReceiverGetsTransferTransactionDetails
    {
        private const long SenderAccountId = 1;
        private const string SenderAccountName = "Test Sender";
        private const string SenderPublicHashedId = "ABC123";

        private const long ReceiverAccountId = 2;
        private const string ReceiverAccountName = "Test Receiver";
        private const string ReceiverPublicHashedId = "DEF456";

        private const string PeriodEnd = "1718-R01";

        private const string FirstCourseName = "Course 1";
        private const string SecondCourseName = "Course 2";

        private GetTransferTransactionDetailsQueryHandler _handler;
        private GetTransferTransactionDetailsQuery _query;
        private GetTransferTransactionDetailsResponse _response;
        private Mock<EmployerFinanceDbContext> _db;
        private List<AccountTransfer> _transfers;
        private Mock<IPublicHashingService> _publicHashingService;
        private PeriodEnd _periodEnd;

        [SetUp]
        public void Assign()
        {
            _db = new Mock<EmployerFinanceDbContext>();

            _periodEnd = new PeriodEnd
            {
                Id = 1,
                PeriodEndId = PeriodEnd,
                AccountDataValidAt = DateTime.Now.AddDays(-2),
                CalendarPeriodMonth = 2,
                CalendarPeriodYear = 2018,
                CommitmentDataValidAt = DateTime.Now.AddDays(-1),
                CompletionDateTime = DateTime.Now,
                PaymentsForPeriod = "Test"
            };

            _publicHashingService = new Mock<IPublicHashingService>();

            _query = new GetTransferTransactionDetailsQuery
            {
                AccountId = ReceiverAccountId,
                TargetAccountPublicHashedId = SenderPublicHashedId,
                PeriodEnd = PeriodEnd
            };

            _response = new GetTransferTransactionDetailsResponse();

            _handler = new GetTransferTransactionDetailsQueryHandler(_db.Object, _publicHashingService.Object);

            _transfers = new List<AccountTransfer>
                {
                    new AccountTransfer
                    {
                        SenderAccountId = SenderAccountId,
                        SenderAccountName = SenderAccountName,
                        ReceiverAccountId = ReceiverAccountId,
                        ReceiverAccountName = ReceiverAccountName,
                        CommitmentId = 1,
                        CourseName = FirstCourseName,
                        Amount = 123.4567M,
                        PeriodEnd = PeriodEnd

                    },
                    new AccountTransfer
                    {
                        SenderAccountId = SenderAccountId,
                        SenderAccountName = SenderAccountName,
                        ReceiverAccountId = ReceiverAccountId,
                        ReceiverAccountName = ReceiverAccountName,
                        CommitmentId = 2,
                        CourseName = SecondCourseName,
                        Amount = 346.789M,
                        PeriodEnd = PeriodEnd
                    },
                    new AccountTransfer
                    {
                        SenderAccountId = SenderAccountId,
                        SenderAccountName = SenderAccountName,
                        ReceiverAccountId = ReceiverAccountId,
                        ReceiverAccountName = ReceiverAccountName,
                        CommitmentId = 3,
                        CourseName = SecondCourseName,
                        Amount = 234.56M,
                        PeriodEnd = PeriodEnd
                    }
                };

            _db.Setup(d => d.SqlQueryAsync<AccountTransfer>(
                It.IsAny<string>(), SenderAccountId, ReceiverAccountId, PeriodEnd))
                .ReturnsAsync(_transfers);

            _db.Setup(d => d.SqlQueryAsync<AccountTransfer>(
                    It.IsAny<string>(), ReceiverAccountId, SenderAccountId, PeriodEnd))
                .ReturnsAsync(_transfers);

            _db.Setup(x => x.PeriodEnds).Returns(() => new DbSetStub<PeriodEnd>(_periodEnd));

            _publicHashingService.Setup(x => x.DecodeValue(SenderPublicHashedId))
                .Returns(SenderAccountId);

            _publicHashingService.Setup(x => x.DecodeValue(ReceiverPublicHashedId))
                .Returns(ReceiverAccountId);

            _publicHashingService.Setup(x => x.HashValue(SenderAccountId))
                .Returns(SenderPublicHashedId);

            _publicHashingService.Setup(x => x.HashValue(ReceiverAccountId))
                .Returns(ReceiverPublicHashedId);
        }

        [Test]
        public async Task ThenIShouldReturnCorrectSenderDetails()
        {
            //Act
            var result = await _handler.Handle(_query);

            //Assert
            Assert.AreEqual(SenderAccountName, result.SenderAccountName);
            Assert.AreEqual(SenderPublicHashedId, result.SenderAccountPublicHashedId);
        }

        [Test]
        public async Task ThenIShouldReturnCorrectReceiverDetails()
        {
            //Act
            var result = await _handler.Handle(_query);

            //Assert
            Assert.AreEqual(ReceiverAccountName, result.ReceiverAccountName);
            Assert.AreEqual(ReceiverPublicHashedId, result.ReceiverAccountPublicHashedId);
        }

        [Test]
        public async Task ThenIShouldReturnCorrectCourseDetails()
        {
            //Act
            var result = await _handler.Handle(_query);

            //Assert
            Assert.AreEqual(2, result.TransferDetails.Count());
            Assert.IsTrue(result.TransferDetails.Any(t => t.CourseName.Equals(FirstCourseName)));
            Assert.IsTrue(result.TransferDetails.Any(t => t.CourseName.Equals(SecondCourseName)));
        }

        [Test]
        public async Task ThenIShouldReturnCorrectCourseSubTotals()
        {
            //Act
            var result = await _handler.Handle(_query);

            //Assert
            Assert.AreEqual(2, result.TransferDetails.Count());

            var firstCourseTotal = result.TransferDetails.Single(t => t.CourseName.Equals(FirstCourseName)).PaymentTotal;
            var secondCourseTotal = result.TransferDetails.Single(t => t.CourseName.Equals(SecondCourseName)).PaymentTotal;

            var expectedFirstCourseTotal =
                _transfers.Where(t => t.CourseName.Equals(FirstCourseName)).Sum(x => x.Amount);

            var expectedSecondCourseTotal =
                _transfers.Where(t => t.CourseName.Equals(SecondCourseName)).Sum(x => x.Amount);

            Assert.AreEqual(expectedFirstCourseTotal, firstCourseTotal);
            Assert.AreEqual(expectedSecondCourseTotal, secondCourseTotal);
        }

        [Test]
        public async Task ThenIShouldReturnCorrectCourseApprenticeCount()
        {
            //Act
            var result = await _handler.Handle(_query);

            //Assert

            var firstCourseApprenticeCount = result.TransferDetails.Single(t => t.CourseName.Equals(FirstCourseName))
                .ApprenticeCount;

            var secondCourseApprenticeCount = result.TransferDetails.Single(t => t.CourseName.Equals(SecondCourseName))
                .ApprenticeCount;

            var expectedFirstCourseApprenticeCount =
                _transfers.Count(t => t.CourseName.Equals(FirstCourseName));

            var expectedSecondCourseApprenticeCount =
                _transfers.Count(t => t.CourseName.Equals(SecondCourseName));

            Assert.AreEqual(expectedFirstCourseApprenticeCount, firstCourseApprenticeCount);
            Assert.AreEqual(expectedSecondCourseApprenticeCount, secondCourseApprenticeCount);
        }

        [Test]
        public async Task ThenIShouldReturnTransferPaymentTotal()
        {
            //Act
            var result = await _handler.Handle(_query);

            //Assert
            var expectedPaymentTotal = _transfers.Sum(t => t.Amount);

            Assert.AreEqual(expectedPaymentTotal, result.TransferPaymentTotal);
        }

        [Test]
        public async Task ThenIShouldReturnTransferDate()
        {
            //Act
            var result = await _handler.Handle(_query);

            //Assert
            Assert.AreEqual(_periodEnd.CompletionDateTime, result.DateCreated);
        }

        [Test]
        public async Task ThenIShouldBeToldIfImTheReceiver()
        {
            //Act
            var result = await _handler.Handle(_query);

            //Assert
            Assert.IsFalse(result.IsCurrentAccountSender);
        }

        [Test]
        public async Task ThenIShouldBeToldIfImTheSender()
        {
            //Arrange
            var query = new GetTransferTransactionDetailsQuery
            {
                AccountId = SenderAccountId,
                TargetAccountPublicHashedId = ReceiverPublicHashedId,
                PeriodEnd = PeriodEnd
            };

            //Act
            var result = await _handler.Handle(query);

            //Assert
            Assert.IsTrue(result.IsCurrentAccountSender);
        }
    }
}