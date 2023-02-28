using PixelMatcher.Common;
using PixelMatcher.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PixelMatcher.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        public const double MinWindowWidth = 640;
        public const double MinWindowHeight = 480;

        public const double MinimumOpacity = 0.1;
        public const double MaximumOpacity = 0.9;

        public const double MinimumZoomLevel = 1.0;
        public const double MaximumZoomLevel = 12.0;

        private bool _topmost = true;
        private double _imageOpacity = 0.5;
        private int _imageIndex = 0;
        private double _zoomLevel = MinimumZoomLevel;
        private bool _isDragging;
        private Point _clickPosition;
        private double _imagePositionX;
        private double _imagePositionY;

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

        public BitmapSource ImageSource => ImageIndex != 0 ? Images[ImageIndex - 1] : null;

        public double WindowWidth
        {
            get => ImageSource == null ? MinWindowWidth : ImageSource.Width + 4;
            set { } // Ignore resize by user
        }

        public double WindowHeight
        {
            get => ImageSource == null ? MinWindowHeight : ImageSource.Height + 34;
            set { } // Ignore resize by user
        }

        public ObservableCollection<BitmapSource> Images { get; } = new ObservableCollection<BitmapSource>();

        public int ImageIndex
        {
            get => _imageIndex;
            set
            {
                if (SetProperty(ref _imageIndex, value))
                {
                    OnPropertyChanged(nameof(ImageSource));
                    OnPropertyChanged(nameof(WindowWidth));
                    OnPropertyChanged(nameof(WindowHeight));
                }
            }
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (SetProperty(ref _zoomLevel, Math.Round(value)))
                {
                    ImagePositionX = 0;
                    ImagePositionY = 0;
                }
            }
        }

        public double ImagePositionX
        {
            get => _imagePositionX;
            set => SetProperty(ref _imagePositionX, value);
        }

        public double ImagePositionY
        {
            get => _imagePositionY;
            set => SetProperty(ref _imagePositionY, value);
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
        public ICommand ImageMouseDownCommand { get; }
        public ICommand ImageMouseMoveCommand { get; }
        public ICommand ImageMouseUpCommand { get; }
        public ICommand MoveImageUpCommand { get; }
        public ICommand MoveImageDownCommand { get; }
        public ICommand MoveImageLeftCommand { get; }
        public ICommand MoveImageRightCommand { get; }

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
            ImageMouseDownCommand = new DelegateCommand(ImageMouseDownCommandHandler);
            ImageMouseMoveCommand = new DelegateCommand(ImageMouseMoveCommandHandler);
            ImageMouseUpCommand = new DelegateCommand(ImageMouseUpCommandHandler);
            MoveImageUpCommand = new DelegateCommand(MoveImageUpCommandHandler);
            MoveImageDownCommand = new DelegateCommand(MoveImageDownCommandHandler);
            MoveImageLeftCommand = new DelegateCommand(MoveImageLeftCommandHandler);
            MoveImageRightCommand = new DelegateCommand(MoveImageRightCommandHandler);
        }

        private void MoveImageRightCommandHandler(object obj)
        {
            ImagePositionX += 1;
        }

        private void MoveImageLeftCommandHandler(object obj)
        {
            ImagePositionX -= 1;
        }

        private void MoveImageDownCommandHandler(object obj)
        {
            ImagePositionY += 1;
        }

        private void MoveImageUpCommandHandler(object obj)
        {
            ImagePositionY -= 1;
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
            if (obj is MouseButtonEventArgs e &&
                e.ChangedButton == MouseButton.Left &&
                e.OriginalSource is FrameworkElement element)
            {
                e.Handled = true;
                var mainWindow = WindowHelper.GetParentWindow(element);
                mainWindow.DragMove();
            }
        }

        private void ImageMouseDownCommandHandler(object obj)
        {
            if (obj is MouseButtonEventArgs e &&
                e.OriginalSource is FrameworkElement element)
            {
                e.Handled = true;
                _isDragging = true;
                _clickPosition = e.GetPosition(WindowHelper.GetParentWindow(element));
                _clickPosition.X -= ImagePositionX;
                _clickPosition.Y -= ImagePositionY;
                element.CaptureMouse();
            }
        }

        private void ImageMouseMoveCommandHandler(object obj)
        {
            if (_isDragging &&
                obj is MouseEventArgs e &&
                e.OriginalSource is FrameworkElement element)
            {
                var currentPosition = e.GetPosition(WindowHelper.GetParentWindow(element));

                ImagePositionX = currentPosition.X - _clickPosition.X;
                ImagePositionY = currentPosition.Y - _clickPosition.Y;
            }
        }

        private void ImageMouseUpCommandHandler(object obj)
        {
            if (obj is MouseButtonEventArgs e &&
                e.OriginalSource is FrameworkElement element)
            {
                _isDragging = false;
                element.ReleaseMouseCapture();
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
                if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                    Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    var delta = 1 * Math.Sign(e.Delta);
                    var newZoomLevel = ZoomLevel + delta;
                    ZoomLevel = Math.Max(MinimumZoomLevel, Math.Min(MaximumZoomLevel, newZoomLevel));
                }
                else
                {
                    var delta = 0.05 * Math.Sign(e.Delta);
                    var newOpacity = ImageOpacity + delta;
                    ImageOpacity = Math.Max(MinimumOpacity, Math.Min(MaximumOpacity, newOpacity));
                }
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
            ImageIndex += 1;
        }

        private bool CanExecuteNextImageCommand(object _)
        {
            return ImageIndex > 0 && ImageIndex < Images.Count;
        }

        private void PreviousImageCommandHandler(object _)
        {
            ImageIndex -= 1;
        }

        private bool CanExecutePreviousImageCommand(object _)
        {
            return ImageIndex > 1;
        }

        private void DeleteCurrentImageCommandHandler(object obj)
        {
            RemoveCurrentImage();
        }

        private bool CanExecuteDeleteCurrentImageCommand(object obj)
        {
            return ImageIndex > 0;
        }

        private bool CanExecutePasteCommand(object obj)
        {
            return Clipboard.ContainsImage();
        }

        private void PasteCommandHandler(object obj)
        {
            try
            {
                var bitmap = Clipboard.GetData(DataFormats.Bitmap);
                if (bitmap is InteropBitmap image)
                {
                    // Windows Paint (and, maybe, some other apps) puts image to the clipboard as
                    // so-called device dependent bitmap, without the BITMAPFILEHEADER structure in it,
                    // so WPF cannot use it as is.
                    // To overcome this, reconvert the image through MemoryStream
                    var newImage = ImageHelper.Convert(ImageHelper.GetBitmap(image));
                    AddImage(newImage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddImage(BitmapSource image)
        {
            Images.Add(image);
            ImageIndex = Images.Count;
        }

        private void RemoveCurrentImage()
        {
            if (Images.Count == 0) return;

            Images.RemoveAt(ImageIndex - 1);
            ImageIndex = Math.Min(ImageIndex, Images.Count);
        }
    }
}
