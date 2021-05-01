using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls.Templates;
using JetBrains.Annotations;

namespace Mvolonia.Controls
{
    public class GroupStyle : AvaloniaObject
    {
        /// <summary>
        /// Defines the <see cref="HeaderTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
            AvaloniaProperty.Register<GroupStyle, IDataTemplate>(nameof(HeaderTemplate));
        
        /// <summary>
        /// Gets or sets the data template used to display the group headers in GroupingListBox.
        /// </summary>
        public IDataTemplate HeaderTemplate
        {
            get => GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }
    }
}