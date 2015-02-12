using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inoreader.Models;

namespace Inoreader.Resources
{
	public class SteamItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate SteamItemTemplate { get; set; }
		public DataTemplate EmptySpaceTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item == null)
				return base.SelectTemplateCore(null);

			var type = item.GetType();
			if (type == typeof(EmptySpaceSteamItem))
				return EmptySpaceTemplate;

			return SteamItemTemplate;
		}
	}
}