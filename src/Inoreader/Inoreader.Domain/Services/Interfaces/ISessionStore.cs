namespace Inoreader.Domain.Services.Interfaces
{
    public interface ISessionStore
    {
        string Auth { get; set; }
        string LSID { get; set; }
        string SID { get; set; }

        void Clear();
        void Save();
    }
}