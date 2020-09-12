using Microsoft.Extensions.Configuration;

namespace Northwind.ITTests
{
    internal static class TestConstants
    {
        public const string ItTests = "ITTests";
        public const string TestDbConnectionKey = "ITTestDB";
        public const string TestConfig = "testsettings.json";
        public const string AuthScheme = "Bearer";

        public static IConfigurationRoot GetTestConfiguration(string testConfigFile)
        {
            return new ConfigurationBuilder().AddJsonFile(testConfigFile).Build();
        }
    }
}