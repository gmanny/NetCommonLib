﻿// from https://gist.github.com/ryanvs/8059757
// This is free and unencumbered software released into the public domain.
// For more information, please refer to <http://unlicense.org/>

using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace WpfAppCommon.Utils;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
public class DropDownButtonBehavior : Behavior<Button>
{
    private long attachedCount;
    private bool isContextMenuOpen;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AddHandler(Button.ClickEvent, new RoutedEventHandler(AssociatedObject_Click), true);
    }

    public bool SetPlacement { get; set; } = true;

    void AssociatedObject_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button source && source.ContextMenu != null)
        {
            // Only open the ContextMenu when it is not already open. If it is already open,
            // when the button is pressed the ContextMenu will lose focus and automatically close.
            if (!isContextMenuOpen)
            {
                source.ContextMenu.AddHandler(ContextMenu.ClosedEvent, new RoutedEventHandler(ContextMenu_Closed), true);
                Interlocked.Increment(ref attachedCount);
                // If there is a drop-down assigned to this button, then position and display it 
                source.ContextMenu.PlacementTarget = source;
                if (SetPlacement)
                {
                    source.ContextMenu.Placement = PlacementMode.Bottom;
                }
                source.ContextMenu.IsOpen = true;
                isContextMenuOpen = true;
            }
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(AssociatedObject_Click));
    }

    void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        isContextMenuOpen = false;
        if (sender is not ContextMenu contextMenu)
        {
            return;
        }
        contextMenu.RemoveHandler(ContextMenu.ClosedEvent, new RoutedEventHandler(ContextMenu_Closed));
        Interlocked.Decrement(ref attachedCount);
    }
}