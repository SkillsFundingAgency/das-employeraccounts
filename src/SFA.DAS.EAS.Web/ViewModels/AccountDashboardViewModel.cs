﻿using SFA.DAS.EAS.Domain.Data.Entities.Account;
using SFA.DAS.EAS.Domain.Models.UserProfile;

namespace SFA.DAS.EAS.Web.ViewModels
{
    public class AccountDashboardViewModel
    {
        public string UserFirstName { get; set; }
        public Account Account { get; set; }
        public int RequiresAgreementSigning { get; set; }
        public Role UserRole { get; set; }
        public string EmployerAccountType { get; set; }
        public bool HideWizard { get; set; }
    }
}