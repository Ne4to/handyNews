using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.Mvvm;

namespace Inoreader.ViewModels.Pages
{
	public class GifImagePageViewModel : ViewModel
	{
		private Uri _imageUri;

		public Uri ImageUri
		{
			get { return _imageUri; }
			set { SetProperty(ref _imageUri, value); }
		}

		public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
		{
			ImageUri = new Uri((string)navigationParameter);
		}
	}
}