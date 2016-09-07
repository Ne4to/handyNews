using Windows.UI.Xaml;
using AdaptiveTriggerLibrary.ConditionModifiers.ComparableModifiers;
using AdaptiveTriggerLibrary.Triggers;

namespace handyNews.UWP.StateTriggers
{
    public class ConditionTrigger : AdaptiveTriggerBase<bool, IComparableModifier>,
        IDynamicTrigger
    {
        public static readonly DependencyProperty IsConditionMetProperty = DependencyProperty.Register(
            "IsConditionMet", typeof(bool), typeof(ConditionTrigger), new PropertyMetadata(default(bool), OnIsConditionMetPropertyChanged));

        private static void OnIsConditionMetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var conditionTrigger = (ConditionTrigger)d;
            conditionTrigger.OnIsConditionMetChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        public bool IsConditionMet
        {
            get { return (bool)GetValue(IsConditionMetProperty); }
            set { SetValue(IsConditionMetProperty, value); }
        }

        public ConditionTrigger()
            : base(true, new EqualToModifier())
        {
            // Set initial value
            CurrentValue = GetCurrentValue();
        }

        private bool GetCurrentValue()
        {
            return IsConditionMet;
        }

        public void ForceValidation()
        {
            CurrentValue = GetCurrentValue();
        }

        public void SuspendUpdates()
        {
            //throw new System.NotImplementedException();
        }

        public void ResumeUpdates()
        {
            //throw new System.NotImplementedException();
        }

        private void OnIsConditionMetChanged(bool oldValue, bool newValue)
        {
            ForceValidation();
        }
    }
}