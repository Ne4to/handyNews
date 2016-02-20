using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using handyNews.Domain.Models;

namespace Inoreader.Resources
{
	public class SubscriptionItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate CategoryTemplate { get; set; }
		public DataTemplate ItemTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			if (item == null)
				return base.SelectTemplateCore(null);			

			var type = item.GetType();
			if (type == typeof (CategoryItem))
				return CategoryTemplate;

			if (type == typeof (SubscriptionItem))
				return ItemTemplate;

			return null;
		}
	}
}