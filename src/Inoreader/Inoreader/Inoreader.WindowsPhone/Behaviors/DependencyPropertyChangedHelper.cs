using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Inoreader.Behaviors
{
	public class DependencyPropertyChangedHelper : DependencyObject
	{
		public DependencyPropertyChangedHelper(DependencyObject source, string propertyPath)
		{
			Binding binding = new Binding
			{
				Source = source,
				Path = new PropertyPath(propertyPath)
			};
			BindingOperations.SetBinding(this, HelperProperty, binding);
		}

		public static readonly DependencyProperty HelperProperty =
			DependencyProperty.Register("Helper", typeof(object), typeof(DependencyPropertyChangedHelper), new PropertyMetadata(null, OnPropertyChanged));

		public object Helper
		{
			get { return (object)GetValue(HelperProperty); }
			set { SetValue(HelperProperty, value); }
		}

		private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var handler = ((DependencyPropertyChangedHelper)d).PropertyChanged;
			if (handler != null)
			{
				handler(d, e);
			}
		}

		public event DependencyPropertyChangedEventHandler PropertyChanged;
	}
}