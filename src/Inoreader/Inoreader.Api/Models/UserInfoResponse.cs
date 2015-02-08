using Newtonsoft.Json;

namespace Inoreader.Api.Models
{
	public class UserInfoResponse
	{
		[JsonProperty(PropertyName = "userId")]
		public string UserId { get; set; }

		[JsonProperty(PropertyName = "userName")]
		public string UserName { get; set; }

		[JsonProperty(PropertyName = "userProfileId")]
		public string UserProfileId { get; set; }

		[JsonProperty(PropertyName = "userEmail")]
		public string UserEmail { get; set; }

		[JsonProperty(PropertyName = "isBloggerUser")]
		public bool IsBloggerUser { get; set; }

		[JsonProperty(PropertyName = "signupTimeSec")]
		public long SignupTimeSec { get; set; }

		[JsonProperty(PropertyName = "isMultiLoginEnabled")]
		public bool IsMultiLoginEnabled { get; set; }		
	}
}