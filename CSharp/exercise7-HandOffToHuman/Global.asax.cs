namespace Exercise7
{
    using System.Web.Http;
    using Autofac;
    using Exercise7.Dialogs;    
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;
    using HandOff;
    using Util;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var builder = new ContainerBuilder();
                        
            builder.Register(c => new ElevateMeScorable())
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
            
            builder.RegisterType<SearchScorable>()
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ShowArticleDetailsScorable>()
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();

            builder.RegisterType<SampleUserRole>()
                    .As<IUserRoleResolver>();

            builder.RegisterModule<HandOffModule>();

            builder.Update(Microsoft.Bot.Builder.Dialogs.Conversation.Container);
        }
    }
}
