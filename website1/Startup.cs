using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(website1.Startup))]
namespace website1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
