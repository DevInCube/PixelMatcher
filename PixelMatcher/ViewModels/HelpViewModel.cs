using PixelMatcher.Common;
using System.Windows;
using System.Windows.Input;

namespace PixelMatcher.ViewModels
{
    internal class HelpViewModel : ObservableObject
    {
        public string HelpText => Resources.WindowResources.HelpText;

        public ICommand ExitCommand { get; }

        public HelpViewModel()
        {
            ExitCommand = new DelegateCommand(ExitCommandHandler);
        }

        private void ExitCommandHandler(object obj)
        {
            if (obj is Window mainWindow)
            {
                mainWindow.Close();
            }
        }
    }
}
