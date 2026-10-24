using System.Windows;
using System.Windows.Controls;

namespace MoneyDashboard.App.Behaviors;

public static class NumericTextBoxBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(NumericTextBoxBehavior),
            new PropertyMetadata(
                false,
                OnIsEnabledChanged));

    public static bool GetIsEnabled(
        DependencyObject obj)
    {
        return (bool)obj.GetValue(
            IsEnabledProperty);
    }

    public static void SetIsEnabled(
        DependencyObject obj,
        bool value)
    {
        obj.SetValue(
            IsEnabledProperty,
            value);
    }

    private static void OnIsEnabledChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            textBox.PreviewTextInput +=
                TextBox_PreviewTextInput;

            DataObject.AddPastingHandler(
                textBox,
                OnPaste);
        }
        else
        {
            textBox.PreviewTextInput -=
                TextBox_PreviewTextInput;

            DataObject.RemovePastingHandler(
                textBox,
                OnPaste);
        }
    }

    private static void TextBox_PreviewTextInput(
        object sender,
        System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !IsNumeric(e.Text);
    }

    private static void OnPaste(
        object sender,
        DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(
            DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        var text =
            e.DataObject.GetData(
                DataFormats.Text) as string;

        if (string.IsNullOrEmpty(text) ||
            !IsNumeric(text))
        {
            e.CancelCommand();
        }
    }

    private static bool IsNumeric(string text)
    {
        return text.All(
            c => c >= '0' &&
                 c <= '9');
    }
}