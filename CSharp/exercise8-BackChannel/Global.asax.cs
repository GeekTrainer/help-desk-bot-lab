namespace Exercise7
{
    using System.Web.Http;
    using Autofac;
    using Exercise7.Dialogs;
    using Exercise7.HandOff;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var builder = new ContainerBuilder();

            // Hand Off Scorables, Provider and UserRoleResolver
            builder.Register(c => new RouterScorable(c.Resolve<IBotData>(), c.Resolve<ConversationReference>(), c.Resolve<Provider>()))
                .As<IScorable<IActivity, double>>().InstancePerLifetimeScope();
            builder.Register(c => new CommandScorable(c.Resolve<IBotData>(), c.Resolve<ConversationReference>(), c.Resolve<Provider>()))
                .As<IScorable<IActivity, double>>().InstancePerLifetimeScope();
            builder.RegisterType<Provider>()
                .SingleInstance();

            // Bot Scorables
            builder.Register(c => new AgentLoginScorable(c.Resolve<IBotData>(), c.Resolve<Provider>()))
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();            
            builder.RegisterType<SearchScorable>()
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
            builder.RegisterType<ShowArticleDetailsScorable>()
                .As<IScorable<IActivity, double>>()
                .InstancePerLifetimeScope();
            builder.Update(Microsoft.Bot.Builder.Dialogs.Conversation.Container);
        }
    }
}
