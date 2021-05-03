using System;
using System.Globalization;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace ControlCatalog.Converters
{
    public class GroupButtonsConverter : MarkupExtension, IValueConverter
    {
   
        public IDataTemplate? RogaAndKopytaHeader { get; set; }
        
        
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return RogaAndKopytaHeader?.Build(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) =>
            this;
    }
}