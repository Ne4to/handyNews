using System.Collections.Generic;
using handyNews.Domain.Services;

namespace Inoreader.Views.Pages
{

	public sealed partial class SavedPage 
	{
		private readonly ScrollViewerState _itemsState;

		public SavedPage()
		{
			this.InitializeComponent();
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
	}
}
