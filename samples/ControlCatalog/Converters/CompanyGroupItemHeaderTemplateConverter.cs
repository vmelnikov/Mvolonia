using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Mvolonia.Controls.Collections;

namespace ControlCatalog.Converters
{
    public class CompanyGroupItemHeaderTemplateConverter : MarkupExtension, IValueConverter
    {
   
        public IDataTemplate? OnlyMaleGroupHeaderTemplate { get; set; }
        public IDataTemplate? OnlyFemaleGroupHeaderTemplate { get; set; }
        public IDataTemplate? DefaultGroupHeaderTemplate { get; set; }
        
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return value switch
            {
                "Only Male Company" => OnlyMaleGroupHeaderTemplate?.Build(value),
                "Only Female Company" => OnlyFemaleGroupHeaderTemplate?.Build(value),
                _ => DefaultGroupHeaderTemplate?.Build(value),
            };

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) =>
            this;
    }
}