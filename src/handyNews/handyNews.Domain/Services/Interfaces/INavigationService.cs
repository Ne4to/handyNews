namespace handyNews.Domain.Services.Interfaces
{
    public interface INavigationService
    {
        void Navigate(string viewToken, object parameter);
    }
}