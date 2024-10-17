using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;


namespace ScreenShare.Server
{
     
    // Class contains implementation of the screen stitching using threads (tasks)
     
    public class ScreenStitcher
    {
         
        // SharedClientScreen object.
         
        private readonly SharedClientScreen _sharedClientScreen;

         
        // Thread to run stitcher.
         
        private Task? _stitchTask;

         
        // A private variable to store old image.
         
        private Bitmap? _oldImage;

         
        // Old resolution of the image.
         
        private Resolution? _resolution;

         
        // A count to maintain the number of image stitched. Used in
        // trace logs.
         
        private int _cnt = 0;

         
        // Constructor for ScreenSticher.
         
        public ScreenStitcher(SharedClientScreen scs)
        {
            _oldImage = null;
            _stitchTask = null;
            _resolution = null;
            _sharedClientScreen = scs;
        }

         
        // Uses the 'diff' image curr and the previous image to find the
        // current image. This method is used when the client sends a diff
        // instead of entire image to server.
        public static unsafe Bitmap Process(Bitmap curr, Bitmap prev)
        {
            BitmapData currData = curr.LockBits(new Rectangle(0, 0, curr.Width, curr.Height), ImageLockMode.ReadWrite, curr.PixelFormat);
            BitmapData prevData = prev.LockBits(new Rectangle(0, 0, prev.Width, prev.Height), ImageLockMode.ReadWrite, prev.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(curr.PixelFormat) / 8;
            int heightInPixels = currData.Height;
            int widthInBytes = currData.Width * bytesPerPixel;

            byte* currptr = (byte*)currData.Scan0;
            byte* prevptr = (byte*)prevData.Scan0;

            Bitmap newb = new(curr.Width, curr.Height);
            BitmapData bmd = newb.LockBits(new Rectangle(0, 0, 10, 10), System.Drawing.Imaging.ImageLockMode.ReadOnly, newb.PixelFormat);
            byte* ptr = (byte*)bmd.Scan0;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * currData.Stride;

                for (int x = 0; x < widthInBytes; x += bytesPerPixel)
                {
                    int oldBlue = currptr[currentLine + x];
                    int oldGreen = currptr[currentLine + x + 1];
                    int oldRed = currptr[currentLine + x + 2];
                    int oldAlpha = currptr[currentLine + x + 3];

                    int newBlue = prevptr[currentLine + x];
                    int newGreen = prevptr[currentLine + x + 1];
                    int newRed = prevptr[currentLine + x + 2];
                    int newAlpha = prevptr[currentLine + x + 3];

                    ptr[currentLine + x] = (byte)(oldBlue ^ newBlue);
                    ptr[currentLine + x + 1] = (byte)(oldGreen ^ newGreen);
                    ptr[currentLine + x + 2] = (byte)(oldRed ^ newRed);
                    ptr[currentLine + x + 3] = (byte)(oldAlpha ^ newAlpha);
                }
            }

            curr.UnlockBits(currData);
            prev.UnlockBits(prevData);
            newb.UnlockBits(bmd);

            return newb;
        }

         
        // Method to decompress a byte array compressed by processor.
        public static byte[] DecompressByteArray(byte[] data)
        {
            MemoryStream input = new(data);
            MemoryStream output = new();
            using (DeflateStream dstream = new(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

         
        // Creates(if not exist) and start the task `_stitchTask`
        // Will read the image using `_sharedClientScreen.GetFrame`
        // and puts the final image using `_sharedClientScreen.PutFinalImage`.
        public void StartStitching(int taskId)
        {
            if (_stitchTask != null) return;

            _stitchTask = new Task(() =>
            {
                while (taskId == _sharedClientScreen.TaskId)
                {
                    string? newFrame = _sharedClientScreen.GetImage(taskId);

                    if (taskId != _sharedClientScreen.TaskId) break;

                    if (newFrame == null)
                    {
                        Trace.WriteLine(Utils.GetDebugMessage("New frame returned by _sharedClientScreen is null.", withTimeStamp: true));
                        continue;
                    }

                    Bitmap stichedImage = Stitch(_oldImage, newFrame);
                    Trace.WriteLine(Utils.GetDebugMessage($"STITCHED image from client {_cnt++}", withTimeStamp: true));
                    _oldImage = stichedImage;
                    _sharedClientScreen.PutFinalImage(stichedImage, taskId);
                }
            });

            _stitchTask?.Start();

            Trace.WriteLine(Utils.GetDebugMessage($"Successfully created the stitching task with id {taskId} for the client with id {_sharedClientScreen.Id}", withTimeStamp: true));
        }

         
        // Method to stop the stitcher task.
         
        public void StopStitching()
        {
            if (_stitchTask == null) return;

            Task previousStitchTask = _stitchTask;
            _stitchTask = null;

            try
            {
                previousStitchTask?.Wait();
            }
            catch (Exception e)
            {
                Trace.WriteLine(Utils.GetDebugMessage($"Failed to start the stitching: {e.Message}", withTimeStamp: true));
            }

            Trace.WriteLine(Utils.GetDebugMessage($"Successfully stopped the processing task for the client with id {_sharedClientScreen.Id}", withTimeStamp: true));
        }

         
        // Function to stitch new frame over old image. If the data sent from client
        // has '1' in front then it is a complete image and hence the Process function
        // is not used. Otherwise, the data will have a '0' in front of it and we will
        // have to compute the XOR (using process function) in order to find the current
        // image.
         
        private Bitmap Stitch(Bitmap? oldImage, string newFrame)
        {

            char isCompleteFrame = newFrame[^1];
            newFrame = newFrame.Remove(newFrame.Length - 1);

            byte[]? deser;

            deser = Convert.FromBase64String(newFrame);
            deser = DecompressByteArray(deser);

            MemoryStream ms = new(deser);
            var xor_bitmap = new Bitmap(ms);
            var newResolution = new Resolution() { Height = xor_bitmap.Height, Width = xor_bitmap.Width };


            if (oldImage == null || newResolution != _resolution)
            {
                oldImage = new Bitmap(newResolution.Width, newResolution.Height);
            }

            if (isCompleteFrame == '1') oldImage = xor_bitmap;
            else oldImage = Process(xor_bitmap, oldImage);

            _resolution = newResolution;
            return oldImage;
        }
    }
}
