using Autofac;
using Autofac.Extras.CommonServiceLocator;
using handyNews.UWP.ViewModels.Controls;
using Inoreader.Domain.Services;
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
        
            builder.RegisterType<SignInDialogViewModel>()
                .As<SignInDialogViewModel>().
                PropertiesAutowired(PropertyWiringOptions.PreserveSetValues);

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
