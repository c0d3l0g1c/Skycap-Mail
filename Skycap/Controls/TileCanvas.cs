using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Skycap.Controls
{
    /// <summary>
    /// Defines an area within which you can explicitly tile images, using coordinates that are relative to the Canvas area.
    /// </summary>
    public class TileCanvas : Canvas
    {
        /// <summary>
        /// Identifies the TileCanvas.ImageSource XAML attached property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(TileCanvas), new PropertyMetadata(null, ImageSourceChanged));

        /// <summary>
        /// The last actual size.
        /// </summary>
        private Size lastActualSize;

        /// <summary>
        /// Initialises a new instance of the Skycap.Controls.TileCanvas.
        /// </summary>
        public TileCanvas()
        {
            // Subscribe to the layout updated event
            LayoutUpdated += OnLayoutUpdated;
        }

        /// <summary>
        /// Gets or sets the image source.
        /// </summary>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        /// <summary>
        /// Occurs when the layout of the visual tree changes.
        /// </summary>
        /// <param name="sender">The object that raised the event (TileCanvas).</param>
        /// <param name="o">The event data (Object).</param>
        private void OnLayoutUpdated(object sender, object o)
        {
            // Create a new size
            var newSize = new Size(ActualWidth, ActualHeight);
            // If this size has changed
            if (lastActualSize != newSize)
            {
                // Rebuild the canvas
                lastActualSize = newSize;
                Rebuild();
            }
        }

        /// <summary>
        /// Called when the ImageSource property changes.
        /// </summary>
        /// <param name="o">The dependency object.</param>
        /// <param name="args">The dependency property arguments.</param>
        private static void ImageSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            // Get the reference to TileCanvas
            TileCanvas self = (TileCanvas)o;
            // Get the source
            var src = self.ImageSource;
            // If the source is available
            if (src != null)
            {
                // Create the image and subscribe to events
                var image = new Image { Source = src };
                image.ImageOpened += self.ImageOnImageOpened;
                image.ImageFailed += self.ImageOnImageFailed;

                // Add it to the visual tree to kick off ImageOpened
                self.Children.Add(image);
            }
        }

        /// <summary>
        /// Occurs when the image source is downloaded and decoded with no failure. You can use this event to determine the size of an image before rendering it.
        /// </summary>
        /// <param name="sender">The object that raised the event (Image).</param>
        /// <param name="exceptionRoutedEventArgs">The event data (ExceptionRoutedEventArgs).</param>
        private void ImageOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            // Unsubscribe to image events
            var image = (Image)sender;
            image.ImageOpened -= ImageOnImageOpened;
            image.ImageFailed -= ImageOnImageFailed;
            // Add red background on error
            Children.Add(new TextBlock { Text = exceptionRoutedEventArgs.ErrorMessage, Foreground = new SolidColorBrush(Colors.Red) });
        }

        /// <summary>
        /// Occurs when there is an error associated with image retrieval or format.
        /// </summary>
        /// <param name="sender">The object that raised the event (Image).</param>
        /// <param name="routedEventArgs">The event data (RoutedEventArgs).</param>
        private void ImageOnImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            // Unsubscribe to image events
            var image = (Image)sender;
            image.ImageOpened -= ImageOnImageOpened;
            image.ImageFailed -= ImageOnImageFailed;
            // Rebuild the tile image
            Rebuild();
        }

        /// <summary>
        /// Rebuildd the tiled image.
        /// </summary>
        private void Rebuild()
        {
            // Get the bitmap
            var bmp = ImageSource as BitmapSource;
            if (bmp == null)
                return;

            // Get the dimensions
            var width = bmp.PixelWidth;
            var height = bmp.PixelHeight;
            if (width == 0
             || height == 0)
                return;

            // Clear the children
            Children.Clear();

            // Redraw the tiles
            for (int x = 0; x < ActualWidth; x += width)
            {
                for (int y = 0; y < ActualHeight; y += height)
                {
                    var image = new Image { Source = ImageSource };
                    Canvas.SetLeft(image, x);
                    Canvas.SetTop(image, y);
                    Children.Add(image);
                }
            }

            // Clip image to specified dimensions
            Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
        }
    }
}