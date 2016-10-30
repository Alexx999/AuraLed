using System;
using System.Windows;
using System.Windows.Controls;
using AuraLedHelper.Extensions;

namespace AuraLedHelper.Controls
{
    public class EnumRadioButton : RadioButton
    {
        public static DependencyProperty EnumValueProperty = DependencyProperty.Register("EnumValue", typeof(object), typeof(EnumRadioButton));
        public static DependencyProperty TargetValueProperty = DependencyProperty.Register("TargetValue", typeof(object), typeof(EnumRadioButton), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        
        public EnumRadioButton()
        {
            this.OnDependencyPropertyChanged(rb => rb.IsChecked).Subscribe(IsCheckedChanged);
        }

        private void IsCheckedChanged(bool? b)
        {
            if(b != true) return;

            if (!Equals(EnumValue, TargetValue))
            {
                TargetValue = EnumValue;
            }
        }

        public object EnumValue
        {
            get { return GetValue(EnumValueProperty); }
            set { SetValue(EnumValueProperty, value); }
        }

        public object TargetValue
        {
            get { return GetValue(TargetValueProperty); }
            set
            {
                SetValue(TargetValueProperty, value);

                var tgtValue = Equals(EnumValue, TargetValue);
                if (IsChecked != tgtValue)
                {
                    IsChecked = tgtValue;
                }
            }
        }
    }
}
