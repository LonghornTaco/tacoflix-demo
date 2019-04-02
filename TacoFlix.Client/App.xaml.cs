using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Framework.Messaging;
using Sitecore.Processing.Tasks.Messaging;
using Sitecore.Processing.Tasks.Messaging.Buses;
using TacoFlix.Client.Configuration;
using TacoFlix.Client.Generators;
using TacoFlix.Client.Generators.Contacts;
using TacoFlix.Client.Logging;
using TacoFlix.Client.ViewModels;
using TacoFlix.Client.Views;
using TacoFlix.Model.Configuration;
using TacoFlix.Model.Logging;
using TacoFlix.Model.Providers;
using TacoFlix.Providers.TheMovieDb;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using Sitecore.Framework.Messaging.Rebus.Configuration;
using Sitecore.Processing.Engine.Abstractions.Messages;
using Sitecore.Processing.Tasks.Messaging.Handlers;
using Microsoft.Extensions.Configuration;

namespace TacoFlix.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging();
            var appConfig = new AppConfiguration();

            serviceCollection.AddSingleton<IXconnectConfiguration>(x => appConfig);
            serviceCollection.AddSingleton<IMovieInfoProviderConfiguration>(x => appConfig);
            serviceCollection.AddSingleton<ILogger, ConsoleLogger>();
            serviceCollection.AddSingleton<IMovieInfoProvider, TheMovieDbMovieInfoProvider>();

            var rebusConfigSection = GetConfigSection("rebus");
            serviceCollection.AddMessaging(config => config.AddBuses(rebusConfigSection, _ => { }));
            new RebusConfigurationServices(serviceCollection).AddSqlServerConfigurators();

            serviceCollection.AddSingleton<ISynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>>, SynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>>>();
            serviceCollection.AddSingleton<ISynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>>, SynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>>>();

            serviceCollection.AddTransient(typeof(IMessageHandler<TaskRegistrationStatus>), typeof(TaskRegistrationStatusHandler));
            serviceCollection.AddTransient(typeof(IMessageHandler<TaskProgressResponse>), typeof(TaskProgressResponseHandler));

            serviceCollection.AddSingleton<IRandomGenerator, RandomGenerator>();
            serviceCollection.AddSingleton<IContactGenerator, ContactGenerator>();
            serviceCollection.AddTransient<MainViewViewModel>();
            serviceCollection.AddTransient<MainView>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var viewModel = serviceProvider.GetService<MainViewViewModel>();
            var view = serviceProvider.GetService<MainView>();
            view.DataContext = viewModel;

            view.Show();
        }

        private IConfiguration GetConfigSection(string key)
        {
            var configBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            configBuilder.AddFile("busSettings.xml");
            var configRoot = configBuilder.Build();
            return configRoot.GetSection(key);
        }
    }
}
