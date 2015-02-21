using System;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Inoreader.Views.Controls
{
	public sealed class SwipeControl : ContentControl
	{
		enum ExecuteCommand
		{
			None,
			Left,
			Right
		}

		private TranslateTransform _contentTransform;
		private Grid _leftRoot;
		private TranslateTransform _leftActionTransform;
		private Grid _rightRoot;
		private TranslateTransform _rightActionTransform;
		private ExecuteCommand _executeCommand;

		public static readonly DependencyProperty WorkAreaWidthProperty = DependencyProperty.Register(
			"WorkAreaWidth", typeof(double), typeof(SwipeControl), new PropertyMetadata(default(double)));

		public double WorkAreaWidth
		{
			get { return (double)GetValue(WorkAreaWidthProperty); }
			set { SetValue(WorkAreaWidthProperty, value); }
		}

		public static readonly DependencyProperty LeftCommandProperty = DependencyProperty.Register(
			"LeftCommand", typeof(ICommand), typeof(SwipeControl), new PropertyMetadata(default(ICommand)));

		public ICommand LeftCommand
		{
			get { return (ICommand)GetValue(LeftCommandProperty); }
			set { SetValue(LeftCommandProperty, value); }
		}

		public static readonly DependencyProperty RightCommandProperty = DependencyProperty.Register(
			"RightCommand", typeof(ICommand), typeof(SwipeControl), new PropertyMetadata(default(ICommand)));

		public ICommand RightCommand
		{
			get { return (ICommand)GetValue(RightCommandProperty); }
			set { SetValue(RightCommandProperty, value); }
		}

		public static readonly DependencyProperty LeftContentProperty = DependencyProperty.Register(
			"LeftContent", typeof(object), typeof(SwipeControl), new PropertyMetadata(default(object)));

		public object LeftContent
		{
			get { return (object)GetValue(LeftContentProperty); }
			set { SetValue(LeftContentProperty, value); }
		}

		public static readonly DependencyProperty LeftContentTemplateProperty = DependencyProperty.Register(
			"LeftContentTemplate", typeof(DataTemplate), typeof(SwipeControl), new PropertyMetadata(default(DataTemplate)));

		public DataTemplate LeftContentTemplate
		{
			get { return (DataTemplate)GetValue(LeftContentTemplateProperty); }
			set { SetValue(LeftContentTemplateProperty, value); }
		}

		public static readonly DependencyProperty RightContentProperty = DependencyProperty.Register(
			"RightContent", typeof(object), typeof(SwipeControl), new PropertyMetadata(default(object)));

		public object RightContent
		{
			get { return (object)GetValue(RightContentProperty); }
			set { SetValue(RightContentProperty, value); }
		}

		public static readonly DependencyProperty RightContentTemplateProperty = DependencyProperty.Register(
			"RightContentTemplate", typeof(DataTemplate), typeof(SwipeControl), new PropertyMetadata(default(DataTemplate)));

		public DataTemplate RightContentTemplate
		{
			get { return (DataTemplate)GetValue(RightContentTemplateProperty); }
			set { SetValue(RightContentTemplateProperty, value); }
		}

		public SwipeControl()
		{
			DefaultStyleKey = typeof(SwipeControl);

			Loaded += SwipeControl_Loaded;
			SizeChanged += SwipeControl_SizeChanged;

			ManipulationStarted += SwipeControl_ManipulationStarted;
			ManipulationDelta += SwipeControl_ManipulationDelta;
			ManipulationCompleted += SwipeControl_ManipulationCompleted;
		}

		void SwipeControl_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "LeftTranslationState", false);
			VisualStateManager.GoToState(this, "CenterTranslationState", false);
			VisualStateManager.GoToState(this, "RightTranslationState", false);

			var x = e.Cumulative.Translation.X;
			UpdateValues(x);
		}

		void SwipeControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			var x = e.Cumulative.Translation.X;
			UpdateValues(x);
		}

		private void UpdateValues(double x)
		{
			_contentTransform.X = x;

			if (_leftRoot.Clip != null)
				((TranslateTransform)_leftRoot.Clip.Transform).X = Math.Min(x - ActualWidth, 0D);

			if (_rightRoot.Clip != null)
				((TranslateTransform)_rightRoot.Clip.Transform).X = Math.Max(ActualWidth + x, 0D);

			if (x > ActualWidth * 0.35D)
			{
				_leftActionTransform.X = 15D;
				_executeCommand = ExecuteCommand.Left;
			}
			else
			{
				if (x < ActualWidth * -0.35D)
				{
					_rightActionTransform.X = -15D;
					_executeCommand = ExecuteCommand.Right;
				}
				else
				{
					_leftActionTransform.X = x * 0.35D;
					_rightActionTransform.X = x * 0.35D;
					_executeCommand = ExecuteCommand.None;
				}
			}
		}

		void SwipeControl_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			ICommand executeCommand = null;

			switch (_executeCommand)
			{
				case ExecuteCommand.None:
					VisualStateManager.GoToState(this, "LeftCancelState", true);
					VisualStateManager.GoToState(this, "CenterNormalState", true);
					VisualStateManager.GoToState(this, "RightCancelState", true);
					break;

				case ExecuteCommand.Left:
					VisualStateManager.GoToState(this, "LeftExecuteState", true);
					VisualStateManager.GoToState(this, "CenterExecuteLeftState", true);
					executeCommand = LeftCommand;
					break;

				case ExecuteCommand.Right:
					VisualStateManager.GoToState(this, "RightExecuteState", true);
					VisualStateManager.GoToState(this, "CenterExecuteRightState", true);
					executeCommand = RightCommand;
					break;
			}

			if (executeCommand != null)
				executeCommand.Execute(null);
		}

		void SwipeControl_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateClip();
		}

		void SwipeControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateClip();
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_contentTransform = (TranslateTransform)GetTemplateChild("PART_RootTransform");
			_leftRoot = (Grid)GetTemplateChild("PART_LeftRoot");
			_leftActionTransform = (TranslateTransform)GetTemplateChild("PART_LeftActionTransform");
			_rightRoot = (Grid)GetTemplateChild("PART_RightRoot");
			_rightActionTransform = (TranslateTransform)GetTemplateChild("PART_RightActionTransform");

			UpdateClip();
		}

		private void UpdateClip()
		{
			WorkAreaWidth = ActualWidth;

			var rect = new Rect(0, 0, ActualWidth, ActualHeight);
			Clip = new RectangleGeometry()
			{
				Rect = rect
			};

			if (_leftRoot.Clip != null)
				_leftRoot.Clip.Rect = rect;

			if (_rightRoot.Clip != null)
				_rightRoot.Clip.Rect = rect;
		}
	}
}
