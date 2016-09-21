﻿using System.Collections.Generic;
using SFA.DAS.Tasks.Api.Types;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.Models
{
    public class TaskListViewModel
    {
        public long AccountId { get; set; }
        public List<Task> Tasks { get; set; }
    }
}