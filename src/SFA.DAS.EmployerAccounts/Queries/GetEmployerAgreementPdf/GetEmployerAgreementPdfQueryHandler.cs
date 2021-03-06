﻿using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.HashingService;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementPdf
{
    public class GetEmployerAgreementPdfQueryHandler : IAsyncRequestHandler<GetEmployerAgreementPdfRequest, GetEmployerAgreementPdfResponse>
    {
        private readonly IValidator<GetEmployerAgreementPdfRequest> _validator;
        private readonly IPdfService _pdfService;
        private readonly IEmployerAgreementRepository _employerAgreementRepository;
        private readonly IHashingService _hashingService;

        public GetEmployerAgreementPdfQueryHandler(IValidator<GetEmployerAgreementPdfRequest> validator, IPdfService pdfService, IEmployerAgreementRepository employerAgreementRepository, IHashingService hashingService)
        {
            _validator = validator;
            _pdfService = pdfService;
            _employerAgreementRepository = employerAgreementRepository;
            _hashingService = hashingService;
        }

        public async Task<GetEmployerAgreementPdfResponse> Handle(GetEmployerAgreementPdfRequest message)
        {
            var validationResult = await _validator.ValidateAsync(message);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            if (validationResult.IsUnauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            var employerAgreementId = _hashingService.DecodeValue(message.HashedLegalAgreementId);

            var agreement = await _employerAgreementRepository.GetEmployerAgreement(employerAgreementId);

            var file = await _pdfService.SubsituteValuesForPdf($"{agreement.TemplatePartialViewName}.pdf");


            return new GetEmployerAgreementPdfResponse {FileStream = file};
        }
    }
}
