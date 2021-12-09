﻿using Autofac;
using Service.Circle.Webhooks;

namespace Service.Fireblocks.Webhook.Modules
{
    public class SettingsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(Program.Settings).AsSelf().SingleInstance();
        }
    }
}