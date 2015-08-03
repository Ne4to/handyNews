using System.Threading.Tasks;
using Inoreader.Domain.Models;

namespace Inoreader.Domain.Services.Interfaces
{
    public interface IStreamManager
    {
        Task<GetItemsResult> GetItemsAsync(GetItemsOptions options);
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