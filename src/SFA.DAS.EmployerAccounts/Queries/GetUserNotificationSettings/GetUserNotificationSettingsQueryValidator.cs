﻿using System;
using System.Threading.Tasks;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.Queries.GetUserNotificationSettings
{
    public class GetUserNotificationSettingsQueryValidator: IValidator<GetUserNotificationSettingsQuery>
    {
        public ValidationResult Validate(GetUserNotificationSettingsQuery item)
        {
            var result = new ValidationResult();

            if(string.IsNullOrWhiteSpace(item.UserRef))
            {
                result.AddError(nameof(item.UserRef));
            }

            return result;
        }

        public Task<ValidationResult> ValidateAsync(GetUserNotificationSettingsQuery item)
        {
            throw new NotImplementedException();
        }
    }
}
