using System.Collections.Generic;
using System.Threading.Tasks;
using Inoreader.Domain.Models;

namespace Inoreader.Domain.Services.Interfaces
{
    public interface ISubscriptionsManager
    {
        Task<List<SubscriptionItemBase>> LoadSubscriptionsAsync();
    }
}