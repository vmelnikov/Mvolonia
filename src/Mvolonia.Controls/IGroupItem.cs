using Avalonia.Controls;
using Mvolonia.Controls.Collections;

namespace Mvolonia.Controls
{
    internal interface IGroupItem : IControl
    {
        /// <summary>
        /// Indicates whether GroupItem has no children
        /// </summary>
        bool IsEmpty { get; }

        IPanel Panel { get; }
    }
}