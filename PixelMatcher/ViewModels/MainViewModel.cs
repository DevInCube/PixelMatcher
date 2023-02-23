using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using PixelMatcher.Common;
using PixelMatcher.Helpers;

namespace PixelMatcher.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        private const double MinimumOpacity = 0.1;
        private const double MaximumOpacity = 0.9;

        private bool _topmost = true;
        private double _imageOpacity = 0.5;
        private int _imagePosition = 0;

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty(ref _topmost, value);
        }

        public double ImageOpacity
        {
            get => _imageOpacity;
            set => SetProperty(ref _imageOpacity, value);
        }

        public BitmapSource ImageSource => ImagePosition != 0 ? Images[ImagePosition - 1] : null;
        
        public double WindowWidth
        {
            get => ImageSource == null ? 0 : ImageSource.Width + 4;
            set { } // This needs to be TwoWay to work.
        }

        public double WindowHeight
        {
            get => ImageSource == null ? 0 : ImageSource.Height + 34;
            set { } // This needs to be TwoWay to work.
        }

        public double MinWindowWidth => 400;

        public double MinWindowHeight => 400;

        public ObservableCollection<BitmapSource> Images { get; } = new ObservableCollection<BitmapSource>();

        public int ImagePosition
        {
            get => _imagePosition;
            set
            {
                if (SetProperty(ref _imagePosition, value))
                {
                    OnPropertyChanged(nameof(ImageSource));
                    OnPropertyChanged(nameof(WindowWidth));
                    OnPropertyChanged(nameof(WindowHeight));
                }
            }
        }

        public ICommand ExitCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand ToggleTopmostCommand { get; }
        public ICommand DeleteCurrentImageCommand { get; }
        public ICommand PreviousImageCommand { get; }
        public ICommand NextImageCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand PreviewDropCommand { get; }
        public ICommand MouseWheelCommand { get; }
        public ICommand MouseDownCommand { get; }

        public MainViewModel()
        {
            ExitCommand = new DelegateCommand(ExitCommandHandler);
            HelpCommand = new DelegateCommand(HelpCommandHandle);
            PasteCommand = new DelegateCommand(PasteCommandHandler, CanExecutePasteCommand);
            DeleteCurrentImageCommand = new DelegateCommand(DeleteCurrentImageCommandHandler, CanExecuteDeleteCurrentImageCommand);
            PreviousImageCommand = new DelegateCommand(PreviousImageCommandHandler, CanExecutePreviousImageCommand);
            NextImageCommand = new DelegateCommand(NextImageCommandHandler, CanExecuteNextImageCommand);
            PreviewDropCommand = new DelegateCommand(PreviewDropCommandHandler, CanExecutePreviewDropCommand);
            MouseWheelCommand = new DelegateCommand(MouseWheelCommandHandler);
            ToggleTopmostCommand = new DelegateCommand(ToggleTopmostCommandHandler);
            MouseDownCommand = new DelegateCommand(MouseDownCommandHandler);
        }

        private void ExitCommandHandler(object obj)
        {
            if (obj is Window mainWindow)
            {
                mainWindow.Close();
            }
        }

        private void MouseDownCommandHandler(object obj)
        {
            if (obj is Window mainWindow)
            {
                mainWindow.DragMove();
            }
        }

        private void ToggleTopmostCommandHandler(object obj)
        {
            Topmost = !Topmost;
        }

        private void HelpCommandHandle(object obj)
        {
            if (obj is Window mainWindow)
            {
                var helpWindow = new HelpWindow
                {
                    Owner = mainWindow,
                };
                helpWindow.ShowDialog();
            }
        }

        private void MouseWheelCommandHandler(object obj)
        {
            if (obj is MouseWheelEventArgs e)
            {
                var delta = 0.05 * Math.Sign(e.Delta);
                var newOpacity = ImageOpacity + delta;
                ImageOpacity = Math.Max(MinimumOpacity, Math.Min(MaximumOpacity, newOpacity));
            }
        }

        private void PreviewDropCommandHandler(object obj)
        {
            if (obj is DragEventArgs e)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                try
                {
                    var image = new BitmapImage(new Uri(files[0], UriKind.Absolute));
                    AddImage(image);
                }
                catch (NotSupportedException)
                {
                }
            }
        }

        private bool CanExecutePreviewDropCommand(object obj)
        {
            return
                obj is DragEventArgs e &&
                e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        private void NextImageCommandHandler(object obj)
        {
            ImagePosition += 1;
        }

        private bool CanExecuteNextImageCommand(object _)
        {
            return ImagePosition > 0 && ImagePosition < Images.Count;
        }

        private void PreviousImageCommandHandler(object _)
        {
            ImagePosition -= 1;
        }

        private bool CanExecutePreviousImageCommand(object _)
        {
            return ImagePosition > 1;
        }

        private void DeleteCurrentImageCommandHandler(object obj)
        {
            RemoveCurrentImage();
        }

        private bool CanExecuteDeleteCurrentImageCommand(object obj)
        {
            return ImagePosition > 0;
        }

        private bool CanExecutePasteCommand(object obj)
        {
            return Clipboard.ContainsImage();
        }

        private void PasteCommandHandler(object obj)
        {
            var bitmap = Clipboard.GetData(DataFormats.Bitmap);
            if (bitmap is InteropBitmap image)
            {
                // Don't know why original InteropBitmap is not shown as Image.Source.
                var newImage = ImageHelper.Convert(ImageHelper.GetBitmap(image));
                AddImage(newImage);
            }
        }

        private void AddImage(BitmapSource image)
        {
            Images.Add(image);
            ImagePosition = Images.Count;
        }

        private void RemoveCurrentImage()
        {
            if (ImagePosition == 0) return;

            Images.RemoveAt(ImagePosition - 1);
            ImagePosition = Math.Min(ImagePosition + 1, Images.Count);
        }
    }
}
