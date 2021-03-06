using Microsoft.Azure.Documents;
using SFA.DAS.EmployerAccounts.ReadStore.Data;
using StructureMap;

namespace SFA.DAS.EmployerAccounts.ReadStore.DependencyResolution
{
    internal class ReadStoreDataRegistry : Registry
    {
        public ReadStoreDataRegistry()
        {
            For<IDocumentClient>().Add(c => c.GetInstance<IDocumentClientFactory>().CreateDocumentClient()).Named(GetType().FullName).Singleton();
            For<IDocumentClientFactory>().Use<DocumentClientFactory>();
            For<IAccountUsersRepository>().Use<AccountUsersRepository>().Ctor<IDocumentClient>().IsNamedInstance(GetType().FullName);
        }
    }
}