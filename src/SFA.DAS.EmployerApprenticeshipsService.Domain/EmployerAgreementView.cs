﻿namespace SFA.DAS.EmployerApprenticeshipsService.Domain
{
    public class EmployerAgreementView
    {
        public long Id { get; set; }
        public short StatusId { get; set; }
        public long LegalEntityId { get; set; }
        public string LegalEntityName { get; set; } 
        public int TemplateId { get; set; }
    }
}