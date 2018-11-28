﻿using System;
using System.Collections.Generic;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class UserRolesUpdatedEvent : Event
    {
        public long AccountId { get; }
        public string UserRef { get; }
        public HashSet<UserRole> Roles { get; }
        public UserRolesUpdatedEvent(long accountId, string userRef, HashSet<UserRole> roles, DateTime created)
        {
            AccountId = accountId;
            UserRef = userRef;
            Roles = roles;
            Created = created;
        }
    }
}
