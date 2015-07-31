using Autofac;
using Autofac.Extras.CommonServiceLocator;
using handyNews.UWP.ViewModels.Controls;
using Inoreader;
using Inoreader.Domain.Services;
using Inoreader.Domain.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.ServiceLocation;

namespace handyNews.UWP.Services
{
    class AutofacModule
    {
        public IContainer Register()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<CredentialService>()
                .As<ICredentialService>();

            builder.RegisterInstance(new ApplicationInsightsTelemetryManager(new TelemetryClient()))
                .As<ITelemetryManager>()
                .SingleInstance();
            //builder.RegisterType<ApplicationInsightsTelemetryManager>()
            //    .As<ITelemetryManager>()
            //    .SingleInstance();

            builder.RegisterType<SignInDialogViewModel>()
                .As<SignInDialogViewModel>()
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
