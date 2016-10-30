using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AuraLedHelper.Extensions;

namespace AuraLedHelper
{
    public class EnumRadioButton : RadioButton
    {
        public EnumRadioButton()
        {
            this.OnDependencyPropertyChanged(r => r.IsChecked).Subscribe(c => Fuun(c));
        }

        private void Fuun(bool? b)
        {
            var a = 9;
        }
    }
}
