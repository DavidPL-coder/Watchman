﻿using Autofac;

using LiteDB;

using MongoDB.Driver;

using System.Reflection;

using Watchman.Integrations.Database;

namespace Watchman.Discord.IoC.Modules
{
    public class DatabaseModule : Autofac.Module
    {
        private readonly DiscordConfiguration configuration;

        public DatabaseModule(DiscordConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var assembly = typeof(DatabaseModule)
                .GetTypeInfo()
                .Assembly;

            builder.Register((c, p) => new MongoClient(this.configuration.MongoDbConnectionString).GetDatabase("devscord"))
                .As<IMongoDatabase>()
                .InstancePerLifetimeScope();

            builder.Register((c, p) =>
            {
                var mapper = BsonMapper.Global.UseCamelCase();
                mapper.Entity<Entity>().Id(x => x.Id);
                return new LiteDatabase(this.configuration.LiteDbConnectionString, mapper);
            }).As<ILiteDatabase>().SingleInstance();

            builder.RegisterType<SessionFactory>()
                .As<ISessionFactory>()
                .InstancePerLifetimeScope();
        }
    }
}
