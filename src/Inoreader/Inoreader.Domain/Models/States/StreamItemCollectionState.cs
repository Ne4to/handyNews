using System.Runtime.Serialization;

namespace Inoreader.Domain.Models.States
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

		[DataMember]
		public bool ShowNewestFirst { get; set; }

		[DataMember]
		public int StreamTimestamp { get; set; }

		[DataMember]
		public bool Fault { get; set; }

		[OnDeserializing]
		private void SetDefaultValues(StreamingContext c)
		{
			ShowNewestFirst = true;
		}
	}
}