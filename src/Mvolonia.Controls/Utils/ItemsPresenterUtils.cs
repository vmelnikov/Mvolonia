using System;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Mvolonia.Controls.Presenters;

namespace Mvolonia.Controls.Utils
{
    public static class ItemsPresenterUtils
    {
        public static IItemContainerGenerator GetItemContainerGenerator(this IItemsPresenter itemsPresenter)
        {
            if (itemsPresenter is IItemContainerGeneratorHolder itemContainerGeneratorHolder)
                return itemContainerGeneratorHolder.ItemContainerGenerator;
            if (itemsPresenter is ItemsPresenter itemsPresenterImpl)
                return itemsPresenterImpl.ItemContainerGenerator;
            throw new NotSupportedException();
        }
    }
}