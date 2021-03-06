﻿using SFA.DAS.Validation;
using StructureMap;

namespace SFA.DAS.EmployerAccounts.DependencyResolution
{
    public class ValidationRegistry : Registry
    {
        public ValidationRegistry()
        {
            Scan(s =>
            {
                s.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(Constants.ServiceNamespace));
                s.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
            });
        }
    }
}