using Emgu.CV.UI;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CV {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        long begin = DateTime.Now.Ticks;
        int totalFrams = 0;
        long fps = 0;
        long maxFPS = 8;
        public MainWindow() {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            DateTime begin = DateTime.Now;
            LoaclWebCamsCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            LocalWebCam = new VideoCaptureDevice(LoaclWebCamsCollection[0].MonikerString);
            LocalWebCam.NewFrame += new NewFrameEventHandler(Cam_NewFrame);

            LocalWebCam.Start();
        }
        VideoCaptureDevice LocalWebCam;
        public FilterInfoCollection LoaclWebCamsCollection;
        public async void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs) {
            if (fps > maxFPS) {
                fps--;
                await Task.Delay(30);
                return;
            }
            try {

                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();

                MemoryStream ms = new MemoryStream();
                int maxIndex = 0;
                float maxDiff = 0;
                var last = ((Bitmap)img).GetPixel(0, img.Height / 2);
                for (int i = 0; i < img.Width/2-10; i++) {
                    var pixel = ((Bitmap)img).GetPixel(i+10, img.Height / 2);
                    var cDiff = ColorDiff(pixel, last);
                    if (cDiff>maxDiff) {
                        maxDiff = cDiff;
                        maxIndex = i;
                    }
                }
                
                for (int i = 0; i < 40; i++) {
                    
                    ((Bitmap)img).SetPixel(maxIndex+7, img.Height / 2 - 10+i, System.Drawing.Color.OrangeRed);
                }
                for (int i = maxIndex-20; i < maxIndex+20; i++) {
                    ((Bitmap)img).SetPixel(i+7, img.Height / 2+10, System.Drawing.Color.OrangeRed);

                }
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                await Dispatcher.BeginInvoke(new ThreadStart(delegate {
                    frameHolder.Source = bi;
                }));
                totalFrams++;
                var current = DateTime.Now.Ticks;
                var diff = current - begin;
                fps = totalFrams / (diff / 10000000);
                Console.WriteLine("FPS = " + fps.ToString());
            } catch (Exception ex) {
            }
        }
        private float ColorDiff(System.Drawing.Color c0, System.Drawing.Color c1) {
            var rDiff = Math.Abs( c0.R - c1.R);
            var gDiff = Math.Abs( c0.G - c1.G);
            var bDiff = Math.Abs( c0.B - c1.B);
            //Console.WriteLine(rDiff+" "+ gDiff +" "+bDiff);
            return Math.Abs(c0.GetBrightness()-c1.GetBrightness());
        }
        
    }
}
