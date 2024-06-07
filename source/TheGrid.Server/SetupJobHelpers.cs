using TheGrid.Services.Jobs;

namespace TheGrid.Server
{
    public static class SetupJobHelpers
    {
        public static IServiceCollection AddSetupJobs(this IServiceCollection services)
        {
            //services.AddHostedService<SeedData>();

            return services;
        }
    }
}
