using SFA.DAS.EmployerAccounts.Data;

namespace SFA.DAS.EAS.TestCommon.ScenarioCommonSteps
{
    using System;
    using System.Linq;

    using MediatR;

    using Moq;

    using SFA.DAS.Authentication;
    using SFA.DAS.Authorization;
    using SFA.DAS.Commitments.Api.Client.Interfaces;
    using SFA.DAS.EmployerAccounts.Commands.UpsertRegisteredUser;
    using SFA.DAS.EmployerAccounts.Interfaces;
    using SFA.DAS.EmployerAccounts.Models.Account;
    using SFA.DAS.EmployerAccounts.Models.UserProfile;
    using SFA.DAS.EmployerAccounts.TestCommon.DependencyResolution;
    using SFA.DAS.EmployerAccounts.Web.Orchestrators;
    using SFA.DAS.EmployerAccounts.Web.ViewModels;
    using SFA.DAS.Events.Api.Client;
    using SFA.DAS.Messaging.Interfaces;

    using StructureMap;

    using TechTalk.SpecFlow;

    using IUserRepository = SFA.DAS.EmployerAccounts.Interfaces.IUserRepository;

    public class UserSteps
    {
        private Mock<IEmployerCommitmentApi> _commitmentsApi;

        private IContainer _container;

        private Mock<ICookieStorageService<EmployerAccountData>> _cookieService;

        private Mock<IEventsApi> _eventsApi;

        private Mock<IMessagePublisher> _messagePublisher;

        private Mock<IAuthenticationService> _owinWrapper;

        public UserSteps()
        {
            this._messagePublisher = new Mock<IMessagePublisher>();
            this._owinWrapper = new Mock<IAuthenticationService>();
            this._cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();
            this._eventsApi = new Mock<IEventsApi>();
            this._commitmentsApi = new Mock<IEmployerCommitmentApi>();

            this._container = IoC.CreateContainer(
                this._messagePublisher,
                this._owinWrapper,
                this._cookieService,
                this._eventsApi,
                this._commitmentsApi);
        }

        public long CreateUserWithRole(User user, Role role, long accountId)
        {
            var userRepository = this._container.GetInstance<IUserRepository>();
            var membershipRepository = this._container.GetInstance<IMembershipRepository>();
            var userRecord = userRepository.GetUserByRef(user.UserRef).Result;

            membershipRepository.Create(userRecord.Id, accountId, role).Wait();

            return userRecord.Id;
        }

        public UserViewModel GetExistingUserAccount()
        {
            this._owinWrapper.Setup(x => x.GetClaimValue("sub"))
                .Returns(ScenarioContext.Current["AccountOwnerUserRef"].ToString());
            var orchestrator = this._container.GetInstance<HomeOrchestrator>();
            var user = orchestrator.GetUsers().Result.AvailableUsers.FirstOrDefault(
                c => c.UserRef.Equals(
                    ScenarioContext.Current["AccountOwnerUserRef"].ToString(),
                    StringComparison.CurrentCultureIgnoreCase));
            return user;
        }

        public UserViewModel GetExistingUserAccount(string userRef)
        {
            this._owinWrapper.Setup(x => x.GetClaimValue("sub")).Returns(userRef);
            var orchestrator = this._container.GetInstance<HomeOrchestrator>();
            var user = orchestrator.GetUsers().Result.AvailableUsers.FirstOrDefault(
                c => c.UserRef.Equals(userRef, StringComparison.CurrentCultureIgnoreCase));
            return user;
        }

        public void UpsertUser(UserViewModel userView)
        {
            var mediator = this._container.GetInstance<IMediator>();

            mediator.SendAsync(
                new UpsertRegisteredUserCommand
                    {
                        UserRef = userView.UserRef,
                        FirstName = userView.FirstName,
                        LastName = userView.LastName,
                        EmailAddress = userView.Email
                    }).Wait();
        }
    }
}