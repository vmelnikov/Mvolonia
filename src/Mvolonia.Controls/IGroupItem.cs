using Avalonia.Controls;

namespace Mvolonia.Controls
{
    public interface IGroupItem : IControl
    {
        /// <summary>
        /// Indicates whether GroupItem has no children
        /// </summary>
        bool IsEmpty { get; }
        
        IPanel Panel { get; }
    }
}