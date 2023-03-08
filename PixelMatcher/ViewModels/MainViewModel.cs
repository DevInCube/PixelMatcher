using PixelMatcher.Common;
using PixelMatcher.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PixelMatcher.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        public const double MinWindowWidth = 800;
        public const double MinWindowHeight = 600;

        public const double MinimumOpacity = 0.1;
        public const double MaximumOpacity = 0.9;

        public const double MinimumContrast = -200;
        public const double MaximumContrast = 200;

        public const double MinimumZoomLevel = 1.0;
        public const double MaximumZoomLevel = 12.0;

        private BitmapSource _backgroundImageSource;

        private bool _topmost = true;
        private double _imageOpacity = 0.5;
        private int _imageContrast = 0;
        private int _imageIndex = 0;
        private double _zoomLevel = MinimumZoomLevel;
        private bool _isDragging;
        private System.Windows.Point _clickPosition;
        private double _imagePositionX;
        private double _imagePositionY;
        private double _backgroundImagePositionX;
        private double _backgroundImagePositionY;
        private WindowState _lastWindowState;

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

        public int ImageContrast
        {
            get => _imageContrast;
            set
            {
                if (SetProperty(ref _imageContrast, value))
                {
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        public BitmapSource ImageSource
        {
            get
            {
                if (ImageIndex == 0) return null;

                var image = Images[ImageIndex - 1];
                var cloneImage = (Image)image.Clone();
                ImageHelper.AdjustContrast(cloneImage, ImageContrast);
                return ImageHelper.Convert(cloneImage);
            }
        }

        public BitmapSource BackgroundImageSource
        {
            get => _backgroundImageSource;
            set => SetProperty(ref _backgroundImageSource, value);
        }

        public double WindowWidth
        {
            get => ImageSource == null ? MinWindowWidth : ImageSource.Width / ZoomLevel + 4;
            set { } // Ignore resize by user
        }

        public double WindowHeight
        {
            get => ImageSource == null ? MinWindowHeight : ImageSource.Height / ZoomLevel + 34;
            set { } // Ignore resize by user
        }

        public ObservableCollection<Image> Images { get; } = new ObservableCollection<Image>();

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
                var oldZoomLevel = _zoomLevel;
                if (SetProperty(ref _zoomLevel, Math.Round(value)))
                {
                    ImagePositionX = 0;
                    ImagePositionY = 0;
                    if (oldZoomLevel == 1 && _zoomLevel > 1)
                    {
                        GetScreenshot();
                    }
                    else if (_zoomLevel == 1)
                    {
                        BackgroundImageSource = null;
                    }
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

        public double BackgroundImagePositionX
        {
            get => _backgroundImagePositionX;
            set => SetProperty(ref _backgroundImagePositionX, value);
        }

        public double BackgroundImagePositionY
        {
            get => _backgroundImagePositionY;
            set => SetProperty(ref _backgroundImagePositionY, value);
        }

        public ICommand ExitCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand ToggleTopmostCommand { get; }
        public ICommand MinimizeToTrayCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
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
        public ICommand ResetImageContrastCommand { get; }

        public MainViewModel()
        {
            ExitCommand = new DelegateCommand(ExitCommandHandler);
            HelpCommand = new DelegateCommand(HelpCommandHandler);
            PasteCommand = new DelegateCommand(PasteCommandHandler, CanExecutePasteCommand);
            DeleteCurrentImageCommand = new DelegateCommand(DeleteCurrentImageCommandHandler, CanExecuteDeleteCurrentImageCommand);
            PreviousImageCommand = new DelegateCommand(PreviousImageCommandHandler, CanExecutePreviousImageCommand);
            NextImageCommand = new DelegateCommand(NextImageCommandHandler, CanExecuteNextImageCommand);
            PreviewDropCommand = new DelegateCommand(PreviewDropCommandHandler, CanExecutePreviewDropCommand);
            MouseWheelCommand = new DelegateCommand(MouseWheelCommandHandler);
            ToggleTopmostCommand = new DelegateCommand(ToggleTopmostCommandHandler);
            MinimizeToTrayCommand = new DelegateCommand(MinimizeToTrayCommandHandler);
            MinimizeCommand = new DelegateCommand(MinimizeCommandHandler);
            MaximizeCommand = new DelegateCommand(MaximizeCommandHandler);
            MouseDownCommand = new DelegateCommand(MouseDownCommandHandler);
            ImageMouseDownCommand = new DelegateCommand(ImageMouseDownCommandHandler);
            ImageMouseMoveCommand = new DelegateCommand(ImageMouseMoveCommandHandler);
            ImageMouseUpCommand = new DelegateCommand(ImageMouseUpCommandHandler);
            MoveImageUpCommand = new DelegateCommand(MoveImageUpCommandHandler);
            MoveImageDownCommand = new DelegateCommand(MoveImageDownCommandHandler);
            MoveImageLeftCommand = new DelegateCommand(MoveImageLeftCommandHandler);
            MoveImageRightCommand = new DelegateCommand(MoveImageRightCommandHandler);
            ResetImageContrastCommand = new DelegateCommand(ResetImageContrastCommandHandler);
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

                if (e.ClickCount > 1)
                {
                    MaximizeCommand.Execute(mainWindow);
                }
                else
                {
                    mainWindow.DragMove();
                }
            }
        }

        private void MaximizeCommandHandler(object obj)
        {
            if (obj is Window mainWindow)
            {
                if (mainWindow.WindowState == WindowState.Normal)
                {
                    mainWindow.WindowState = WindowState.Maximized;
                }
                else if (mainWindow.WindowState == WindowState.Maximized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
            }
        }

        private void ImageMouseDownCommandHandler(object obj)
        {
            if (obj is MouseButtonEventArgs e &&
                e.OriginalSource is FrameworkElement element)
            {
                e.Handled = true;
                element.Focus();
                _isDragging = true;
                _clickPosition = e.GetPosition(WindowHelper.GetParentWindow(element));
                if (BackgroundImageSource != null &&
                    IsCtrlPressed())
                {
                    _clickPosition.X -= BackgroundImagePositionX;
                    _clickPosition.Y -= BackgroundImagePositionY;
                }
                else
                {
                    _clickPosition.X -= ImagePositionX;
                    _clickPosition.Y -= ImagePositionY;
                }
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

                if (BackgroundImageSource != null &&
                    IsCtrlPressed())
                {
                    BackgroundImagePositionX = currentPosition.X - _clickPosition.X;
                    BackgroundImagePositionY = currentPosition.Y - _clickPosition.Y;
                }
                else
                {
                    ImagePositionX = currentPosition.X - _clickPosition.X;
                    ImagePositionY = currentPosition.Y - _clickPosition.Y;
                }
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

        private void MinimizeToTrayCommandHandler(object obj)
        {
            if (obj is Window mainWindow)
            {
                if (!mainWindow.IsVisible)
                {
                    mainWindow.Show();
                    if (mainWindow.WindowState == WindowState.Minimized)
                    {
                        mainWindow.WindowState = _lastWindowState;
                    }
                    mainWindow.BringIntoView();
                    mainWindow.Activate();
                }
                else
                {
                    if (mainWindow.WindowState != WindowState.Minimized)
                    {
                        _lastWindowState = mainWindow.WindowState;
                    }
                    mainWindow.WindowState = WindowState.Minimized;
                    mainWindow.Hide();
                }
            }
        }

        private void MinimizeCommandHandler(object obj)
        {
            if (obj is Window mainWindow)
            {
                _lastWindowState = mainWindow.WindowState;
                mainWindow.WindowState = WindowState.Minimized;
            }
        }

        private void HelpCommandHandler(object obj)
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
                if (IsCtrlPressed())
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
                    var image = Bitmap.FromFile(files[0]);
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
                    AddImage(ImageHelper.GetBitmap(image));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetImageContrastCommandHandler(object obj)
        {
            ImageContrast = 0;
        }

        private void AddImage(Image image)
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

        private void GetScreenshot()
        {
            var window = Application.Current.MainWindow;
            if (window == null) return;

            BackgroundImageSource = null;
            window.Opacity = 0;
            var previousWindowState = window.WindowState;
            window.WindowState = WindowState.Normal; // To be excluded from the screenshot, window should not be maximized

            // Wait for Opacity to apply to take screenshot without self
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

            var screenBeginPoint = window.PointToScreen(new System.Windows.Point(2, 32)); // omit window header

            BackgroundImageSource = ScreenHelper.GetScreenshot();

            // Return window to visible state
            window.WindowState = previousWindowState;
            window.Opacity = 1;
            BackgroundImagePositionX = -screenBeginPoint.X;
            BackgroundImagePositionY = -screenBeginPoint.Y;
        }

        private static bool IsCtrlPressed()
        {
            return
                Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl);
        }
    }
}
