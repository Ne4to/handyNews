using System;
using System.Collections.Generic;

namespace Inoreader.Domain.Services
{
	public class HtmlTagLexeme : ILexeme
	{
		public string Name { get; set; }
		public bool IsOpen { get; set; }
		public bool IsClose { get; set; }
		public Dictionary<string, string> Attributes { get; set; }

		public HtmlTagLexeme()
		{
			Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}
	}
}