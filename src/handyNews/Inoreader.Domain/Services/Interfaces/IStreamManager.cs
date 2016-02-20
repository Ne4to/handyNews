using System.Threading.Tasks;
using handyNews.Domain.Models;

namespace handyNews.Domain.Services.Interfaces
{
    public interface IStreamManager
    {
        Task<GetItemsResult> GetItemsAsync(GetItemsOptions options);
        Task MarkAllAsReadAsync(string streamId, int streamTimestamp);
    }

    public class GetItemsOptions
    {
        public int Count { get; set; }
        public string Continuation { get; set; }
        public string StreamId { get; set; }
        public bool ShowNewestFirst { get; set; }
        public bool IncludeRead { get; set; }
    }

    public class GetItemsResult
    {
        public StreamItem[] Items { get; set; }
        public string Continuation { get; set; }
        public int Timestamp { get; set; }
    }
}