using System.Collections.Generic;
using Inoreader.Services;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.Views.Pages
{
	public sealed partial class StreamPage : IView
	{
		private readonly ScrollViewerState _itemsState;

		public StreamPage()
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
	}
}
