using System;
using System.Threading.Tasks;
using System.Windows;
using AForge.Video;
using Accord.Video.FFMPEG;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CV
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        long begin = DateTime.Now.Ticks;
        int totalFrams = 0;
        long fps = 0;
        long maxFPS = 30;
        VideoFileWriter writer = new VideoFileWriter();
        bool isRecording = false;
        DateTime? StartTime = null;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime begin = DateTime.Now;
            LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[0].MonikerString);
            LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);
            LocalWebCam.VideoResolution=LocalWebCam.VideoCapabilities[18];
            LocalWebCam.Start();

            
        }

        

        VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LoaclWebCamsCollection;
        public async void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            //if (fps > maxFPS) {
            //    fps--;
            //    await Task.Delay(30);
            //    return;
            //}
            try
            {

                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                if (isRecording)
                {
                    using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                    {
                        try
                        {
                            if (StartTime != null)
                            {
                                writer.WriteVideoFrame(bitmap, DateTime.Now - StartTime.Value);
                            }
                            else
                            {
                                StartTime = DateTime.Now;
                                writer.WriteVideoFrame(bitmap);
                            }
                        }
                        catch (Exception)
                        {

                            
                        }
                        
                    }
                }


                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    MemoryStream ms = new MemoryStream();


                    bitmap.Save(ms, ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();

                    bi.Freeze();
                    Dispatcher.Invoke(() =>
                    {
                        frameHolder.Source = bi;
                    });
                    //Dispatcher.CurrentDispatcher.Invoke(() => frameHolder.Source = bi);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error" + ex.Message);
            }

        }
        private float ColorDiff(System.Drawing.Color c0, System.Drawing.Color c1)
        {
            var rDiff = Math.Abs(c0.R - c1.R);
            var gDiff = Math.Abs(c0.G - c1.G);
            var bDiff = Math.Abs(c0.B - c1.B);
            //Console.WriteLine(rDiff+" "+ gDiff +" "+bDiff);
            return Math.Abs(c0.GetBrightness() - c1.GetBrightness());
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                isRecording = true;
                //StartTime = DateTime.Now;
                
                var codec = VideoCodec.Default;
                var a = codec.ToString();
                writer.Open(DateTime.Now.ToString("yyyyMMdd-hhmmss") +".mp4", 1280, 960, 30, codec,2000000);
                
                Record.Content = "Stop";

            }
            else
            {
                Record.Content = "Record";
                isRecording = false;
                StartTime = null;
                Task.Delay(500);
                writer.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LocalWebCam.SignalToStop();
            LocalWebCam.NewFrame -= Cam_NewFrame;
            writer.Dispose();
        }

        public void Dispose()
        {
            
            writer.Dispose();
        }
    }
}
