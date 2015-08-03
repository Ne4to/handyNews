using System;
using Windows.Storage;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using handyNews.UWP.ViewModels.Controls;
using handyNews.UWP.ViewModels.Controls.Interfaces;
using Inoreader;
using Inoreader.Api;
using Inoreader.Domain.Services;
using Inoreader.Domain.Services.Interfaces;
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

            builder.RegisterType<CredentialService>()
                .As<ICredentialService>();

            builder.RegisterType<SessionStore>()
                .As<ISessionStore>()
                .SingleInstance();

            builder.RegisterType<ApiSession>()
                .As<IApiSession>();

            var uri = new Uri("ms-appx:///Assets/ApiAuth.json");
            var file = StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().Result;
            var strData = FileIO.ReadTextAsync(file).AsTask().Result;
            var data = JObject.Parse(strData);

            var appId = data["AppId"].ToString();
            var appKey = data["AppKey"].ToString();
            
            builder.RegisterType<ApiClient>()
                .WithParameter("appId", appId)
                .WithParameter("appKey", appKey)
                .As<ApiClient>()
                .SingleInstance();

            builder.RegisterType<SignInManager>()
                .As<ISignInManager>()
                .SingleInstance();

            builder.RegisterType<AppSettingsService>()
                .As<ISettingsManager>();

            builder.RegisterType<SubscriptionsManager>()
                .As<ISubscriptionsManager>();

            builder.RegisterType<SignInDialogViewModel>()
                .As<ISignInDialogViewModel>()
                .PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);

            builder.RegisterType<SubscriptionsTreeViewModel>()
                .As<ISubscriptionsTreeViewModel>()
                .PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);

            // Perform registrations and build the container.
            var container = builder.Build();

            RegisterServiceLocator(container);

            return container;
        }

        private static void RegisterServiceLocator(IContainer container)
        {
            // Set the service locator to an AutofacServiceLocator.
            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }
    }
}
