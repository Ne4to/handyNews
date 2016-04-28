using System;
using Windows.Storage;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using handyNews.API;
using handyNews.Domain.Services;
using handyNews.Domain.Services.Interfaces;
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
            var authorizationDataStorage = new AuthorizationDataStorage();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(new ApplicationInsightsTelemetryManager(new TelemetryClient()))
                .As<ITelemetryManager>()
                .SingleInstance();

            builder.RegisterInstance<IAuthorizationDataStorage>(authorizationDataStorage);
            //builder.RegisterType<AuthorizationDataStorage>()
            //    .As<IAuthorizationDataStorage>()
            //    .SingleInstance();

            var uri = new Uri("ms-appx:///Assets/AppSecret.json");
            var file = StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().Result;
            var strData = FileIO.ReadTextAsync(file).AsTask().Result;
            var data = JObject.Parse(strData);

            var appId = data["AppId"].ToString();
            var appKey = data["AppKey"].ToString();
            
            builder.RegisterType<ApiClient>()
                .WithParameter("appId", appId)
                .WithParameter("appKey", appKey)
                .WithParameter("authorizationHandler", new AuthorizationHandler(authorizationDataStorage))
                .As<ApiClient>()
                .SingleInstance();

            builder.RegisterType<AuthenticationManager>()
                .As<IAuthenticationManager>()
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
