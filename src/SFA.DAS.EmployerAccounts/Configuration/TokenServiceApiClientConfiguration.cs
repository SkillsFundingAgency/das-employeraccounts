using Microsoft.Azure;
using SFA.DAS.TokenService.Api.Client;
using System.Security.Cryptography.X509Certificates;

namespace SFA.DAS.EmployerAccounts.Configuration
{
    public class TokenServiceApiClientConfiguration : ITokenServiceApiClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
        public string Tenant { get; set; }
        public X509Certificate TokenCertificate
        {
            get
            {
                var store = new X509Store(StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                try
                {
                    var thumbprint = CloudConfigurationManager.GetSetting("TokenServiceCertificateThumbprint");

                    if (string.IsNullOrEmpty(thumbprint))
                    {
                        return null;
                    }

                    var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                    return certificates[0];
                }
                finally
                {
                    store.Close();
                }
            }
            set { }
        }
    }
}