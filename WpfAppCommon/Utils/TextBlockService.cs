using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfAppCommon.Utils
{
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
        public static Boolean GetIsTextTrimmed(TextBlock target)
        {
            return (Boolean)target.GetValue(IsTextTrimmedProperty);
        }

        public static readonly DependencyProperty AutomaticToolTipEnabledProperty = DependencyProperty.RegisterAttached(
            "AutomaticToolTipEnabled",
            typeof(bool),
            typeof(TextBlockService),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnAutomaticToolTipEnabledChanged));

        private static void OnAutomaticToolTipEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBlock textBlock))
            {
                return;
            }

            if (e.NewValue is bool b && b)
            {
                textBlock.SizeChanged += OnTextBlockSizeChanged;
            }
            else
            {
                textBlock.SizeChanged -= OnTextBlockSizeChanged;
            }
        }

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static Boolean GetAutomaticToolTipEnabled(DependencyObject element)
        {
            if (null == element)
            {
                throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(AutomaticToolTipEnabledProperty);
        }

        public static void SetAutomaticToolTipEnabled(DependencyObject element, bool value)
        {
            if (null == element)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(AutomaticToolTipEnabledProperty, value);
        }

        private static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            TriggerTextRecalculation(sender);
        }

        private static void TriggerTextRecalculation(object sender)
        {
            var textBlock = sender as TextBlock;
            if (null == textBlock)
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
}