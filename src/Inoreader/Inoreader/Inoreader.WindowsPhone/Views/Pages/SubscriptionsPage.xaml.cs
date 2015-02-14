using System.Collections.Generic;
using Inoreader.Services;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.Views.Pages
{
	public sealed partial class SubscriptionsPage : IView
	{
		private readonly ScrollViewerState _itemsState;

		public SubscriptionsPage()
		{
			InitializeComponent();
			_itemsState = new ScrollViewerState(TreeItemsListView);
		}

		protected override void SaveState(Dictionary<string, object> pageState)
		{
			base.SaveState(pageState);

			if (pageState == null || _itemsState == null)
				return;

			_itemsState.Save(pageState, "TreeItemsListView_");
		}

		protected override void LoadState(object navigationParameter, Dictionary<string, object> pageState)
		{
			base.LoadState(navigationParameter, pageState);

			if (pageState == null || _itemsState == null)
				return;

			_itemsState.Load(pageState, "TreeItemsListView_");
		}
	}
}
