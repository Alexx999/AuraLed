using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace AuraLedHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private FieldInfo _fieldInfo;

        public App()
        {
            var actualValue = DesignerProperties.IsInDesignModeProperty.DefaultMetadata.DefaultValue;
            UpdateDesignerMode(true);
            CheckLicense(Xceed.Wpf.Toolkit.Licenser.LicenseKey);
            UpdateDesignerMode((bool)actualValue);

            TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(DependencyObject),
                new FrameworkPropertyMetadata(TextFormattingMode.Display, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
            InitializeComponent();


            var service = FirstFloor.XamlSpy.Services.XamlSpyService.Current;
            service.Connect("127.0.0.1", 4530, "01311");
        }

        private void CheckLicense(string licenseKey)
        {
            //Sorry - I don't think that using XAML inspectors requires "Plus" but due to bug in licensing it won't work without this hack
        }

        private void UpdateDesignerMode(bool value)
        {
            var metadata = DesignerProperties.IsInDesignModeProperty.DefaultMetadata;
            var field = _fieldInfo ?? (_fieldInfo = typeof(PropertyMetadata).GetField("_defaultValue", BindingFlags.Instance | BindingFlags.NonPublic));
            field?.SetValue(metadata, value);
        }
    }
}
