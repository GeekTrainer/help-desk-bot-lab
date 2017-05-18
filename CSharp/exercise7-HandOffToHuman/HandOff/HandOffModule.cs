namespace Exercise7.HandOff
{
    using System;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;    

    public class HandOffModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder
                .Register(c => new CommandScorable(c.Resolve<IBotState>(), c.Resolve<ConversationReference>(), c.Resolve<IUserRoleResolver>(), c.Resolve<Provider>()))
                .As<IScorable<IActivity, double>>().InstancePerLifetimeScope();

            builder
                .Register(c => new RouterScorable(c.Resolve<IBotState>(), c.Resolve<ConversationReference>(), c.Resolve<IUserRoleResolver>(), c.Resolve<Provider>()))
                .As<IScorable<IActivity, double>>().InstancePerLifetimeScope();

            builder
                .RegisterType<Provider>()
                .SingleInstance();
        }
    }
}
