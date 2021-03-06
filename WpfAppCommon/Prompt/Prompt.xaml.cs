using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfAppCommon.Prompt;

public enum MessageBoxType
{
    MessageBox = 0,
    InputBox = 1,
    PasswordBox = 2
}

public partial class Prompt
{
    public string prompt = "";
    public string title = "";
    public MessageBoxButton button = MessageBoxButton.OK;
    public MessageBoxImage icon = MessageBoxImage.None;
    public MessageBoxType type = 0;
    public string defaultResponse = "";
    public MessageBoxResult messageboxResult;

    public static string? OkText { get; set; }
    public static string? CancelText { get; set; }
    public static string? YesText { get; set; }
    public static string? NoText { get; set; }

    private readonly Dictionary<string, List<string>> lang = new()
    {
        {"ru-RU", new List<string> {"OK","Отмена","Да","Нет" } },
        {"en-US", new List<string> {"OK","Cancel","Yes","No" } }
    };

    public Prompt()
    {
        InitializeComponent();
    }

    private void Promt_Loaded(object sender, RoutedEventArgs e)
    {
        btnOK.Visibility = Visibility.Collapsed;
        btnYes.Visibility = Visibility.Collapsed;
        btnNo.Visibility = Visibility.Collapsed;
        btnCancel.Visibility = Visibility.Collapsed;

        tbInput.Visibility = Visibility.Collapsed;
        btnGenerate.Visibility = Visibility.Collapsed;
            
        Title = string.IsNullOrEmpty(title) ? "" : title;
        tbContent.Text = string.IsNullOrEmpty(prompt) ? "" : prompt;

        LocalizeButtons();

        switch (button)
        {
            case MessageBoxButton.OK:
                btnOK.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.OKCancel:
                btnOK.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.YesNo:
                btnYes.Visibility = Visibility.Visible;
                btnNo.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.YesNoCancel:
                btnYes.Visibility = Visibility.Visible;
                btnNo.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                break;
        }

        switch (icon)
        {
            case MessageBoxImage.Information:
                imgIcon.Source = new BitmapImage(new Uri("img/information.png", UriKind.Relative));
                break;
            case MessageBoxImage.Error:
                imgIcon.Source = new BitmapImage(new Uri("img/critical.png", UriKind.Relative));
                break;
            case MessageBoxImage.Exclamation:
                imgIcon.Source = new BitmapImage(new Uri("img/exclamation.png", UriKind.Relative));
                break;
            case MessageBoxImage.Question:
                imgIcon.Source = new BitmapImage(new Uri("img/question.png", UriKind.Relative));
                break;
        }

        switch (type)
        {
            case MessageBoxType.MessageBox:

                break;
            case MessageBoxType.InputBox:
                tbInput.Visibility = Visibility.Visible;
                tbInput.Text = string.IsNullOrEmpty(defaultResponse) ? "" : defaultResponse;
                tbInput.SelectAll();
                tbInput.Focus();
                break;
            case MessageBoxType.PasswordBox:
                tbInput.Visibility = Visibility.Visible;
                btnGenerate.Visibility = Visibility.Visible;
                tbInput.Text = string.IsNullOrEmpty(defaultResponse) ? "12345" : defaultResponse;
                tbInput.SelectAll();
                tbInput.Focus();
                break;
        }

    }

    private void LocalizeButtons()
    {
        if (lang.ContainsKey(CultureInfo.CurrentCulture.Name) && lang[CultureInfo.CurrentCulture.Name].Count == 4)
        {
            btnOK.Content = lang[CultureInfo.CurrentCulture.Name][0];
            btnCancel.Content = lang[CultureInfo.CurrentCulture.Name][1];
            btnYes.Content = lang[CultureInfo.CurrentCulture.Name][2];
            btnNo.Content = lang[CultureInfo.CurrentCulture.Name][3];
        }

        if (!string.IsNullOrEmpty(OkText)) { btnOK.Content = OkText; }
        if (!string.IsNullOrEmpty(CancelText)) { btnCancel.Content = CancelText; }
        if (!string.IsNullOrEmpty(YesText)) { btnYes.Content = YesText; }
        if (!string.IsNullOrEmpty(NoText)) { btnNo.Content = NoText; }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        messageboxResult = MessageBoxResult.Cancel;
        DialogResult = true;
    }

    private void BtnNo_Click(object sender, RoutedEventArgs e)
    {
        messageboxResult = MessageBoxResult.No;
        DialogResult = true;
    }

    private void BtnYes_Click(object sender, RoutedEventArgs e)
    {
        messageboxResult = MessageBoxResult.Yes;
        DialogResult = true;
    }

    private void BtnOK_Click(object sender, RoutedEventArgs e)
    {
        messageboxResult = MessageBoxResult.OK;
        DialogResult = true;
    }

    private void BtnGenerate_Click(object sender, RoutedEventArgs e)
    {
        tbInput.Text = "12345";
        tbInput.SelectAll();
        tbInput.Focus();
    }
}