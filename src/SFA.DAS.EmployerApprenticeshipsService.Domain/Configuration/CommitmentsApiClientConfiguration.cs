using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EAS.Domain.Configuration
{
    public class CommitmentsApiClientConfiguration : ICommitmentsApiClientConfiguration, IJwtClientConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }
    }
}