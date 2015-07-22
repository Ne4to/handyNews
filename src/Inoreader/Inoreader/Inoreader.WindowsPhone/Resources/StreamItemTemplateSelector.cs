using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inoreader.Domain.Models;

namespace Inoreader.Resources
{
	public class StreamItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate HeaderSpaceTemplate { get; set; }
		public DataTemplate StreamItemTemplate { get; set; }
		public DataTemplate EmptySpaceTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item == null)
				return base.SelectTemplateCore(null);

			var type = item.GetType();
			if (type == typeof(EmptySpaceStreamItem))
				return EmptySpaceTemplate;

			if (type == typeof (HeaderSpaceStreamItem))
				return HeaderSpaceTemplate;

			return StreamItemTemplate;
		}
	}
}