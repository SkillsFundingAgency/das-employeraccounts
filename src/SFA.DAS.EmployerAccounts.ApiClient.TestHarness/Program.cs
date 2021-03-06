﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerAccounts.Api.Client;
using SFA.DAS.EmployerAccounts.ApiClient.TestHarness.DependencyResolution;
using SFA.DAS.EmployerAccounts.Types.Models;

namespace SFA.DAS.EmployerAccounts.ApiClient.TestHarness
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Task.Run(Test).Wait();

            Console.WriteLine("Press ENTER to finish");
            Console.ReadLine();
        }

        private static async Task Test()
        {
            using (var container = IoC.Initialize())
            {
                try
                {
                    var apiClient = container.GetInstance<IEmployerAccountsApiClient>();
                    var userInRoleRequest = new IsUserInRoleRequest { AccountId = 2134, UserRef = Guid.Parse("45f8e859-337c-4a4f-a184-1e794ec91f4f"), Roles = new HashSet<UserRole>{ UserRole.Owner } } ;
                    var inroleResponse = await apiClient.IsUserInRole(userInRoleRequest, CancellationToken.None);

                    Console.WriteLine("IsUserInRole: " + inroleResponse);

                    var anyRoleRequest = new IsUserInAnyRoleRequest { AccountId = 2134, UserRef = Guid.Parse("45f8e859-337c-4a4f-a184-1e794ec91f4f") };
                    var response = await apiClient.IsUserInAnyRole(anyRoleRequest, CancellationToken.None);

                    Console.WriteLine("IsUserAnyRole: " + response);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }
        }
    }
}
