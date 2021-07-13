using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Styling;

namespace Mvolonia.Controls
{
    public class GroupStyle : AvaloniaObject
    {
        /// <summary>
        /// Defines the <see cref="HeaderTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate> HeaderTemplateProperty =
            AvaloniaProperty.Register<GroupStyle, IDataTemplate>(nameof(HeaderTemplate));

        public static readonly DirectProperty<GroupStyle, IStyle> ContainerStyleProperty =
            AvaloniaProperty.RegisterDirect<GroupStyle, IStyle>(nameof(ContainerStyle), o => o.ContainerStyle);

        private IStyle _containerStyle;

        /// <summary>
        /// Gets or sets the data template used to display the group headers in GroupableListBox.
        /// </summary>
        public IDataTemplate HeaderTemplate
        {
            get => GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        /// <summary>
        ///     ContainerStyle is the style that is applied to the GroupItem generated
        ///     for each item.
        /// </summary>
        public IStyle ContainerStyle
        {
            get => _containerStyle;
            set => SetAndRaise(ContainerStyleProperty, ref _containerStyle, value);
        }
    }
}