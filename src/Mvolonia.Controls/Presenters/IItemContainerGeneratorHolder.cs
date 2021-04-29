using Avalonia.Controls.Generators;

namespace Mvolonia.Controls.Presenters
{
    public interface IItemContainerGeneratorHolder
    {
        IItemContainerGenerator ItemContainerGenerator { get; }
    }
}