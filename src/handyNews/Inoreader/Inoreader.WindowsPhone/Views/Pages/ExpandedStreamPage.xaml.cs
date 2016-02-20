using System.Collections.Generic;
using Windows.UI.Xaml;
using handyNews.Domain.Services;
using handyNews.Domain.Utils;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.Views.Pages
{
	public sealed partial class ExpandedStreamPage : IView
	{
		private readonly ScrollViewerState _itemsState;

		public ExpandedStreamPage()
		{
			InitializeComponent();
			_itemsState = new ScrollViewerState(ItemsListView);
		}

		protected override void SaveState(Dictionary<string, object> pageState)
		{
			base.SaveState(pageState);

			if (pageState == null || _itemsState == null)
				return;

			_itemsState.Save(pageState, "ItemsListView_");
		}

		protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
		{
			base.LoadState(navigationParameter, pageState);

			if (pageState == null || _itemsState == null)
				return;

			_itemsState.Load(pageState, "ItemsListView_");
		}

		private void BackButton_OnClick(object sender, RoutedEventArgs e)
		{
			ScrollUtils.ScrollItem(ItemsListView, -1);
		}

		private void ForwardButton_OnClick(object sender, RoutedEventArgs e)
		{
			ScrollUtils.ScrollItem(ItemsListView, 1);
		}
	}
}
