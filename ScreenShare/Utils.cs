// This file has Frame and the related structures which are used for storing
// the difference in the pixel and the resolution of image. It also has some other
// general utilities.

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
//using Windows.UI.Xaml.Media.Imaging;
using System.Windows.Media.Imaging;

namespace ScreenShare
{
      
    // struct for storing the resolution of a image
       
    public struct Resolution
    {
        public int Height { get; set; }
        public int Width { get; set; }

        public bool Equals(Resolution p) => Height == p.Height && Width == p.Width;
        public static bool operator ==(Resolution lhs, Resolution rhs) => lhs.Equals(rhs);
        public static bool operator !=(Resolution lhs, Resolution rhs) => !(lhs == rhs);
    }

      
    // Defines various general utilities.
       
    public static class Utils
    {
          
        // The string representing the module identifier for screen share.
        
        public const string ModuleIdentifier = "ScreenShare";

        // Printing debug message
         
        public static string GetDebugMessage(string message, bool withTimeStamp = false)
        {
            // Get the class name and the name of the caller function
            StackFrame? stackFrame = (new StackTrace()).GetFrame(1);
            string className = stackFrame?.GetMethod()?.DeclaringType?.Name ?? "SharedClientScreen";
            string methodName = stackFrame?.GetMethod()?.Name ?? "GetDebugMessage";

            string prefix = withTimeStamp ? $"{System.DateTimeOffset.Now:F} | " : "";

            return $"{prefix}[{className}::{methodName}] : {message}";
        }

         
        // Convert an object of "Bitmap" to an object of type "BitmapSource"
           
        
        public static BitmapSource BitmapToBitmapSource(this Bitmap bitmap)
        {
            // Create new memory stream to temporarily save the bitmap there
            using System.IO.MemoryStream stream = new();
            bitmap.Save(stream, ImageFormat.Bmp);

            // Create a new BitmapSource and populate it with Bitmap
            stream.Position = 0;
            BitmapImage result = new();
            result.BeginInit();

            // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
            // Force the bitmap to load right now so we can dispose the stream.
            result.CacheOption = BitmapCacheOption.OnLoad;

            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }

        
        // Convert an object to type "BitmapSource" to an object of type "BitmapImage"
           
        public static BitmapImage BitmapSourceToBitmapImage(BitmapSource bitmapSource)
        {
            // Check if BitmapSource is already a BitmapImage
            if (bitmapSource is not BitmapImage bitmapImage)
            {
                // If not, then create a new BitmapImage
                bitmapImage = new BitmapImage();

                // Create an encoder and add BitmapSource to it
                BmpBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                // Save the encoder temporarily to a memory stream
                using System.IO.MemoryStream memoryStream = new();
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                // Populate the BitmapImage
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        // Convert an object of "Bitmap" to an object of "BitmapImage"
        
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            return BitmapSourceToBitmapImage(BitmapToBitmapSource(bitmap));
        }
    }
}