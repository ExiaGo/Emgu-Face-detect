using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Runtime.InteropServices;
using System.Drawing;

namespace WpfFaceDetectionTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Capture capture;
        private HaarCascade haarCascade;
        int brightness;               //用于控制亮度
        int frameIndex;               //用于优化程序
        MCvAvgComp[] detectedFaces;
        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            brightness = 0;
            frameIndex = 0;
            capture = new Capture();
            haarCascade = new HaarCascade(@"haarcascade_frontalface_default.xml"); //获得人脸特征
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1); //内置间隔已经是最快，1毫秒
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            frameIndex += 1;
            Image<Bgr, Byte> currentFrame = capture.QueryFrame();
            if (currentFrame == null)
            {
                return;
            }
            //以下是亮度倍率控制
            int brightnessRate = 1;
            if (brightness > 0)
            {
                brightnessRate *= brightness;
            }
            if (brightness < 0)
            {
                brightnessRate /= (-brightness);
            }
            //以下是优化控制，每 10 帧就识别一次人脸
            if (frameIndex > 10)
            {
                Image<Gray, Byte> grayFrame = currentFrame.Convert<Gray, Byte>();

                detectedFaces = grayFrame.DetectHaarCascade(haarCascade)[0];
                foreach (var face in detectedFaces)
                    //下面这句是用于修改识别框的颜色
                    //currentFrame.Draw(face.rect, new Bgr(0, double.MaxValue, 0), 3);
                    currentFrame.Draw(face.rect, new Bgr(255, 0, 0), 3);

                image1.Source = ToBitmapSource(currentFrame * brightnessRate);//currentFrame乘以一个数可以改变光暗度
                frameIndex = 0;
            }
            else
            {
                //当不识别的时候（就是前 10 帧），也画框，用来延时达到更好的效果
                if (detectedFaces != null)
                {             
                    foreach (var face in detectedFaces)
                        //下面这句是用于修改识别框的颜色
                        //currentFrame.Draw(face.rect, new Bgr(0, double.MaxValue, 0), 3);
                        currentFrame.Draw(face.rect, new Bgr(255, 0, 0), 3);
                }

                image1.Source = ToBitmapSource(currentFrame * brightnessRate);
            }
            
            

            
        }

        

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        //将视频每一帧解释成一张图，即将 Bitmap 转换成 BitmapSource，这样 WPF 才能将视频读成图片，并在 image1 中显示
        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop
                  .Imaging.CreateBitmapSourceFromHBitmap(
                  ptr,
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }

        private void Increase(object sender, RoutedEventArgs e)
        {
            brightness += 1;
        }

        private void Decrease(object sender, RoutedEventArgs e)
        {
            // Image<Bgr, Byte> currentFrame = capture.QueryFrame();
            // image1.Source = ToBitmapSource(currentFrame/2);
            brightness -= 1;
        }

        private void Screenshoot(object sender, RoutedEventArgs e)
        {
            Image<Bgr, Byte> currentFrame = capture.QueryFrame();
            image2.Source = ToBitmapSource(currentFrame);
        }
    }
}
