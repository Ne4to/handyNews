namespace Inoreader.Api.Models
{
	public class SignInResponse
	{
	    public bool Success { get; set; }
	    public string ErrorMessage { get; set; }
		public string SID { get; set; }
		public string LSID { get; set; }
		public string Auth { get; set; }
	}
}