using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AzureSqlDisasterRecoveryDemo.Startup))]
namespace AzureSqlDisasterRecoveryDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
