using System;
using System.Net.Http;
using System.Reflection;
using Windows.Storage;
using Autofac;
using Autofac.Core;
using Autofac.Extras.CommonServiceLocator;
using handyNews.API;
using handyNews.Domain.Services;
using handyNews.Domain.Services.Interfaces;
using handyNews.UWP.ViewModels;
using handyNews.UWP.ViewModels.Controls;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json.Linq;

namespace handyNews.UWP.Services
{
    class AutofacModule
    {
        public IContainer Register()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new ApplicationInsightsTelemetryManager(new TelemetryClient()))
                .As<ITelemetryManager>()
                .SingleInstance();

            builder.RegisterType<AuthorizationDataStorage>()
                .As<IAuthorizationDataStorage>()
                .SingleInstance();

            builder.RegisterType<AuthenticationManager>()
                .As<IAuthenticationManager>()
                .SingleInstance();

            builder.RegisterType<AuthorizationHandler>()
                .SingleInstance();

            builder.RegisterType<ApiClient>()
                .As<ApiClient>()
                .WithParameter(new ResolvedParameter(ApiClientParameterPredicate, ApiClientParameterAccessor))
                .SingleInstance();

            builder.RegisterType<SettingsManager>()
                .As<ISettingsManager>();

            builder.RegisterType<SubscriptionsManager>()
                .As<ISubscriptionsManager>();

            builder.RegisterType<StreamManager>()
                .As<IStreamManager>();

            builder.RegisterType<ImageManager>()
                .As<ImageManager>();

            builder.RegisterType<NavigationService>()
                .As<INavigationService>();

            builder.RegisterType<SubscriptionsTreeViewModel>()
                .As<ISubscriptionsTreeViewModel>()
                .PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);

            builder.RegisterType<StreamViewViewModel>()
                .As<IStreamViewViewModel>()
                .PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);

            builder.RegisterType<MainPageViewModel>()
                .SingleInstance();

            // Perform registrations and build the container.
            var container = builder.Build();

            RegisterServiceLocator(container);

            return container;
        }

        private bool ApiClientParameterPredicate(ParameterInfo parameterInfo, IComponentContext context)
        {
            return parameterInfo.ParameterType == typeof(DelegatingHandler);
        }

        private object ApiClientParameterAccessor(ParameterInfo parameterInfo, IComponentContext context)
        {
            if (parameterInfo.ParameterType == typeof(DelegatingHandler))
                return context.Resolve<AuthorizationHandler>();

            return null;
        }

        private static void RegisterServiceLocator(IContainer container)
        {
            // Set the service locator to an AutofacServiceLocator.
            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }
    }
}
