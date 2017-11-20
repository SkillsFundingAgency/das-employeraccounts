﻿using System;
using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.EmployerAccounts.Events.Messages
{
    [Serializable]
    [MessageGroup("user_joined")]
    public class UserJoinedMessage : AccountMessageBase
    {
        public UserJoinedMessage()
        { }

        public UserJoinedMessage(long accountId, string createdBy) : base(accountId, createdBy)
        {
           
        }
    }
}
