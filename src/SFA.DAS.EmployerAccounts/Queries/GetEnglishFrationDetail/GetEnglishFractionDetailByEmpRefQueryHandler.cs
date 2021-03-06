﻿using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.Queries.GetEnglishFrationDetail
{
    public class GetEnglishFractionDetailByEmpRefQueryHandler : IAsyncRequestHandler<GetEnglishFractionDetailByEmpRefQuery, GetEnglishFractionDetailResposne>
    {
        private readonly IValidator<GetEnglishFractionDetailByEmpRefQuery> _validator;
        private readonly IDasLevyRepository _dasLevyRepository;

        public GetEnglishFractionDetailByEmpRefQueryHandler(IValidator<GetEnglishFractionDetailByEmpRefQuery> validator, IDasLevyRepository dasLevyRepository)
        {
            _validator = validator;
            _dasLevyRepository = dasLevyRepository;
        }

        public async Task<GetEnglishFractionDetailResposne> Handle(GetEnglishFractionDetailByEmpRefQuery message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            var fractionDetail = await _dasLevyRepository.GetEnglishFractionHistory(message.AccountId, message.EmpRef);

            return new GetEnglishFractionDetailResposne {FractionDetail = fractionDetail};
        }
    }
}
