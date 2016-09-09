using System.Collections.Generic;
using System.Threading.Tasks;
using handyNews.Domain.Models;

namespace handyNews.Domain.Services.Interfaces
{
    public interface IFeedManager
    {
        Task<IReadOnlyCollection<Feed>> GetFeedsAsync();
    }
}