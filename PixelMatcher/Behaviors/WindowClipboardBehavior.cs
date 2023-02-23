using System.Windows;
using System.Windows.Input;

namespace PixelMatcher.Behaviors
{
    public static class WindowClipboardBehavior
    {
        public static readonly DependencyProperty PasteCommandProperty =
            DependencyProperty.RegisterAttached(
                "PasteCommand",
                typeof(ICommand),
                typeof(WindowClipboardBehavior),
                new UIPropertyMetadata(null, PasteCommandChanged));

        private static void PasteCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            var command = e.NewValue as ICommand;

            CommandBinding PasteCommandBinding = new CommandBinding(
                ApplicationCommands.Paste,
                PasteCommandExecuted,
                PasteCommandCanExecute);
            window.CommandBindings.Add(PasteCommandBinding);
        }

        public static ICommand GetPasteCommand(Window window)
        {
            return (ICommand)window.GetValue(PasteCommandProperty);
        }

        public static void SetPasteCommand(Window window, ICommand command)
        {
            window.SetValue(PasteCommandProperty, command);
        }

        private static void PasteCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            GetPasteCommand(target as Window)?.Execute(null);
        }

        private static void PasteCommandCanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GetPasteCommand(target as Window)?.CanExecute(null) ?? false;
            e.Handled = true;
        }
    }
}
