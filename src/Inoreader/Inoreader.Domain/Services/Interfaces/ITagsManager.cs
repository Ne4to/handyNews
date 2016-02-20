namespace handyNews.Domain.Services.Interfaces
{
    public interface ITagsManager
    {
        void AddTag(string itemId, string tag);
        void RemoveTag(string itemId, string tag);
    }
}