
using System.Collections.Generic;
using SFA.DAS.EmployerAccounts.Models.HmrcLevy;

namespace SFA.DAS.EmployerAccounts.TestCommon.ObjectMothers
{
    public static class EnglishFractionObjectMother
    {
        public static EnglishFractionDeclarations Create(string empRef)
        {
            var declarations = new EnglishFractionDeclarations
            {
                Empref = empRef,
                FractionCalculations = new List<FractionCalculation>
                {
                    new FractionCalculation
                    {
                        CalculatedAt = "2017 06 01",
                        Fractions = new List<Fraction>
                        {
                            new Fraction
                            {
                                Region = "England",
                                Value = "0.67",
                            }
                        }
                    },
                    new FractionCalculation
                    {
                        CalculatedAt = "2017 10 01",
                        Fractions = new List<Fraction>
                        {
                            new Fraction
                            {
                                Region = "England",
                                Value = "0.97",
                            }
                        }
                    }
                }
            };
            
            return declarations;
        }

        
    }
}