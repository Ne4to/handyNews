using System.Runtime.Serialization;

namespace Inoreader.Models.States
{
	[DataContract]
	public class StreamItemCollectionState
	{
		[DataMember]
		public string StreamId { get; set; }

		[DataMember]
		public string Continuation { get; set; }

		[DataMember]
		public StreamItem[] Items { get; set; } 
	}
}