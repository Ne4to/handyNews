using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inoreader.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Practices.ServiceLocation;

namespace Inoreader.Behaviors
{
	public class GifExtensions
	{
		public static readonly DependencyProperty GifSourceProperty = DependencyProperty.RegisterAttached("GifSource", typeof(Uri), typeof(GifExtensions), new PropertyMetadata(default(Uri), OnGifSourcePropertyChanged));

		public static void SetGifSource(DependencyObject element, Uri value)
		{
			element.SetValue(GifSourceProperty, value);
		}

		public static Uri GetGifSource(DependencyObject element)
		{
			return (Uri)element.GetValue(GifSourceProperty);
		}

		public static readonly DependencyProperty GifBuilderProperty = DependencyProperty.RegisterAttached("GifBuilder", typeof(GifBuilder), typeof(GifExtensions), new PropertyMetadata(default(GifBuilder)));

		public static void SetGifBuilder(DependencyObject element, GifBuilder value)
		{
			element.SetValue(GifBuilderProperty, value);
		}

		public static GifBuilder GetGifBuilder(DependencyObject element)
		{
			return (GifBuilder)element.GetValue(GifBuilderProperty);
		}

		private static async void OnGifSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (DesignMode.DesignModeEnabled)
				return;

			var image = d as Image;
			if (image == null)
				return;

			var telemetry = ServiceLocator.Current.GetInstance<TelemetryClient>();

			try
			{
				var gifBuilder = GetGifBuilder(image) ?? new GifBuilder(image);
				SetGifBuilder(image, gifBuilder);
				await gifBuilder.UpdateSourceAsync(e.NewValue as Uri);
			}
			catch (Exception exception)
			{
				telemetry.TrackException(exception);
			}
		}
	}
}