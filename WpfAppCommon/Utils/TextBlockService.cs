using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfAppCommon.Utils;

// taken from https://stackoverflow.com/a/25436070 w/https://stackoverflow.com/a/45766452
//Based on the project from http://web.archive.org/web/20130316081653/http://tranxcoder.wordpress.com/2008/10/12/customizing-lookful-wpf-controls-take-2/
public static class TextBlockService
{
    private static readonly DependencyPropertyKey IsTextTrimmedKey = DependencyProperty.RegisterAttachedReadOnly("IsTextTrimmed", 
        typeof(bool), 
        typeof(TextBlockService), 
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

    [AttachedPropertyBrowsableForType(typeof(TextBlock))]
    public static bool GetIsTextTrimmed(TextBlock target)
    {
        return (bool) target.GetValue(IsTextTrimmedProperty);
    }

    public static readonly DependencyProperty AutomaticToolTipEnabledProperty = DependencyProperty.RegisterAttached(
        "AutomaticToolTipEnabled",
        typeof(bool),
        typeof(TextBlockService),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnAutomaticToolTipEnabledChanged));

    private static void OnAutomaticToolTipEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock)
        {
            return;
        }

        if (e.NewValue is true)
        {
            textBlock.SizeChanged += OnTextBlockSizeChanged;
        }
        else
        {
            textBlock.SizeChanged -= OnTextBlockSizeChanged;
        }
    }

    [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
    public static bool GetAutomaticToolTipEnabled(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);

        return (bool) element.GetValue(AutomaticToolTipEnabledProperty);
    }

    public static void SetAutomaticToolTipEnabled(DependencyObject element, bool value)
    {
        ArgumentNullException.ThrowIfNull(element);
        
        element.SetValue(AutomaticToolTipEnabledProperty, value);
    }

    private static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
    {
        TriggerTextRecalculation(sender);
    }

    private static void TriggerTextRecalculation(object sender)
    {
        if (sender is not TextBlock textBlock)
        {
            return;
        }

        if (TextTrimming.None == textBlock.TextTrimming)
        {
            textBlock.SetValue(IsTextTrimmedKey, false);
        }
        else
        {
            var isTextTrimmed = CalculateIsTextTrimmed(textBlock);
            textBlock.SetValue(IsTextTrimmedKey, isTextTrimmed);
        }
    }

    private static bool CalculateIsTextTrimmed(TextBlock textBlock)
    {
        if (textBlock == null || textBlock.TextTrimming == TextTrimming.None || textBlock.TextWrapping != TextWrapping.NoWrap)
        {
            return false;
        }
        if (!textBlock.IsArrangeValid)
        {
            return GetIsTextTrimmed(textBlock);
        }

        double actualWidth = textBlock.ActualWidth;
        textBlock.Measure(new Size(double.MaxValue, double.MaxValue));
        double width = textBlock.DesiredSize.Width;
        return actualWidth < width;
    }
}