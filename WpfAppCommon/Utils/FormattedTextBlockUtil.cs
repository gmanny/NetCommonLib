using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace WpfAppCommon.Utils;

public class FormattedTextBlockUtil
{
    // taken from https://stackoverflow.com/a/18076638/579817
    public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
        "FormattedText", 
        typeof(string), 
        typeof(FormattedTextBlockUtil), 
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure, FormattedTextPropertyChanged));

    public static void SetFormattedText(DependencyObject textBlock, string value)
    {
        textBlock.SetValue(FormattedTextProperty, value);
    }

    public static string GetFormattedText(DependencyObject textBlock)
    {
        return (string)textBlock.GetValue(FormattedTextProperty);
    }

    private static void FormattedTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        InlineCollection inlines;
        if (d is TextBlock tb)
        {
            inlines = tb.Inlines;
        }
        else if (d is Span sp)
        {
            inlines = sp.Inlines;
        }
        else
        {
            throw new Exception($"Unknown inline type {d.GetType().FullName}");
        }

        string formattedText = (string)e.NewValue ?? string.Empty;
        formattedText = $"<Span xml:space=\"preserve\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{formattedText}</Span>";

        inlines.Clear();
        
        using var xmlReader = XmlReader.Create(new StringReader(formattedText));
        Span result = (Span) XamlReader.Load(xmlReader);
        inlines.Add(result);
    }
}