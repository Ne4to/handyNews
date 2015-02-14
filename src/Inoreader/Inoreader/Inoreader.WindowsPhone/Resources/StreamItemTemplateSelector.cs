using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inoreader.Models;

namespace Inoreader.Resources
{
	public class StreamItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StreamItemTemplate { get; set; }
		public DataTemplate EmptySpaceTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item == null)
				return base.SelectTemplateCore(null);

			var type = item.GetType();
			if (type == typeof(EmptySpaceStreamItem))
				return EmptySpaceTemplate;

			return StreamItemTemplate;
		}
	}
}