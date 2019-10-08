using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace FileProcessing
{
    public static class ExecutionContextExtension
    {
        public static string GetConfig(this ExecutionContext context, string sectionName)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return config[sectionName];
        }
    }
}