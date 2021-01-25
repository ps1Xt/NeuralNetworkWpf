using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeuralNetworkWpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NeuralNetwork Neural { get; set; }
        string path = @"C:\Users\ivan.scoropad\Source\Repos\ps1Xt\NeuralNetworkWpf\Kursach NeuralNetwork\2";
        Matrix m;
        public MainWindow()
        {
            InitializeComponent();
            Neural = new NeuralNetwork(784, 2, 2, 10, 0.2);
            Neural.Load(path);
            m = new Matrix();
            Canvas.DefaultDrawingAttributes.Height = 9;
            Canvas.DefaultDrawingAttributes.Width = 9;
        }
        private ImageSource MakeImage(byte[] arr)
        {
            Bitmap bmp = new Bitmap(28, 28);
            int k = 0;
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    bmp.SetPixel(j, i, System.Drawing.Color.FromArgb(arr[k], arr[k], arr[k]));
                    k++;
                }
            }
            return ImageSourceFromBitmap(bmp);
        }
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject([In] IntPtr hObject);

        private ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            }
            finally { DeleteObject(handle); }
        }
        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private void Train_Click(object sender, RoutedEventArgs e)
        {
            new Task(() => TrainMethod()).Start();

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Canvas.Strokes.Clear();
        }
        public System.Drawing.Bitmap BitmapSourceToBitmap2(BitmapSource srs)
        {
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width * ((srs.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height * stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height * stride, stride);
                using (var btm = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format1bppIndexed, ptr))
                {
                    // Clone the bitmap so that we can dispose it and
                    // release the unmanaged memory at ptr
                    return new System.Drawing.Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
        private byte[] SignatureToBitmapBytes()
        {
            //get the dimensions of the ink control
            int margin = (int)Canvas.Margin.Left;
            int width = (int)Canvas.ActualWidth - margin;
            int height = (int)Canvas.ActualHeight - margin;

            //render ink to bitmap
            RenderTargetBitmap rtb =
               new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(Canvas);

            //save the ink to a memory stream
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            byte[] bitmapBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);

                //get the bitmap bytes from the memory stream
                ms.Position = 0;
                bitmapBytes = ms.ToArray();
            }
            return bitmapBytes;
        }
        public static Bitmap ConvertToBitmap(BitmapSource bitmapSource)
        {
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;
            var stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var memoryBlockPointer = Marshal.AllocHGlobal(height * stride);
            bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), memoryBlockPointer, height * stride, stride);
            var bitmap = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, memoryBlockPointer);
            return bitmap;
        }
        private System.Drawing.Bitmap ConvertInkCanvasToImage()
        {

            //render bitmap
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)Canvas.ActualWidth, (int)Canvas.ActualHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            rtb.Render(Canvas);
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            rtb.Render(Canvas);

            //save to memory stream or file 
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                Bitmap bitmap = new Bitmap(ms);
                return bitmap;

            }

        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {


            Bitmap bmp = ConvertInkCanvasToImage();
            Picture.Source = ImageSourceFromBitmap(bmp);
            byte[] array = new byte[784];
            int q = 0;
            for (int i = 0; i < 140; i += 5)
            {
                for (int j = 0; j < 140; j += 5)
                {
                    int sum = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        for (int z = 0; z < 5; z++)
                        {
                            sum += bmp.GetPixel(j + k, i + z).R;
                        }
                    }
                    array[q++] = (byte)(255 - (sum / 25));
                }
            }
            double[] resultArray = Neural.Query(Normalize(array));
            Result0.Content = string.Format("0 - {0:0.00}", resultArray[0] * 100);
            Result1.Content = string.Format("1- {0:0.00}", resultArray[1] * 100);
            Result2.Content = string.Format("2 - {0:0.00}", resultArray[2] * 100);
            Result3.Content = string.Format("3 - {0:0.00}", resultArray[3] * 100);
            Result4.Content = string.Format("4 - {0:0.00}", resultArray[4] * 100);
            Result5.Content = string.Format("5 - {0:0.00}", resultArray[5] * 100);
            Result6.Content = string.Format("6 - {0:0.00}", resultArray[6] * 100);
            Result7.Content = string.Format("7 - {0:0.00}", resultArray[7] * 100);
            Result8.Content = string.Format("8 - {0:0.00}", resultArray[8] * 100);
            Result9.Content = string.Format("9 - {0:0.00}", resultArray[9] * 100);
            //var result = Array.IndexOf(resultArray, resultArray.Max());
            //Result.Content = result;
            Picture.Source = MakeImage(array);


        }
        private void TrainMethod()
        {
            double[] array = new double[784];
            string number;
            using (var sr = new StreamReader(@"C:\Users\ivan.scoropad\Source\Repos\ps1Xt\NeuralNetworkWpf\Kursach NeuralNetwork\mnist_train.txt"))
            {
                int plus = 0;
                int minus = 0;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                for (int j = 0; j < 100; j++)
                {
                    double error = 0;
                    for (int k = 0; k < 600; k++)
                    {
                        double[] numbers = new double[10];
                        //разбиваем на байты(стринги)
                        var strarray = sr.ReadLine().Split(',');
                        //получаем число котороые ожидаем
                        number = strarray[0];
                        numbers[Convert.ToInt32(number)] = 0.99;
                        array = Normalize(strarray);
                        Neural.Train(array, numbers);

                    }
                    Neural.Save(path);
                    Dispatcher.Invoke(() => Progress.Content = $"Progress: {j}%");
                }
                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("Time: {0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                Dispatcher.Invoke(() => Time.Content = elapsedTime);

            }

        }
        private double[] Normalize(byte[] arr)
        {
            double[] array = new double[arr.Length];
            for (int i = 1; i < arr.Length; i++)
            {

                //нормализуем данные, чтобы были между 0 и 1
                array[i - 1] = Convert.ToDouble(arr[i - 1]) / 256 + 0.01;
            }
            return array;
        }
        private double[] Normalize(string[] arr)
        {
            double[] array = new double[arr.Length];
            for (int i = 1; i < arr.Length; i++)
            {

                //нормализуем данные, чтобы были между 0 и 1
                array[i - 1] = Convert.ToDouble(arr[i - 1]) / 256 + 0.01;
            }
            return array;
        }

        private void TestQuery_Click(object sender, RoutedEventArgs e)
        {
            new Task(() => TestQueryMethod()).Start();
        }
        private void TestQueryMethod()
        {
            double[] array = new double[784];
            string number;
            int plus = 0;
            int minus = 0;
            using (var sr = new StreamReader(@"C:\Users\ivan.scoropad\Source\Repos\ps1Xt\NeuralNetworkWpf\Kursach NeuralNetwork\mnist_test.txt"))
            {
                for (int j = 0; j < 100; j++)
                {
                    for (int k = 0; k < 100; k++)
                    {
                        double[] numbers = new double[10];
                        var strarray = sr.ReadLine().Split(',');
                        number = strarray[0];

                        array = Normalize(strarray);
                        var resultArray = Neural.Query(array);
                        /*Dispatcher.Invoke(() => Result0.Content = string.Format("0 - {0:0.00}", resultArray[0] * 100));
                        Dispatcher.Invoke(() => Result1.Content = string.Format("1 - {0:0.00}", resultArray[1] * 100));
                        Dispatcher.Invoke(() => Result2.Content = string.Format("2 - {0:0.00}", resultArray[2] * 100));
                        Dispatcher.Invoke(() => Result3.Content = string.Format("3 - {0:0.00}", resultArray[3] * 100));
                        Dispatcher.Invoke(() => Result4.Content = string.Format("4 - {0:0.00}", resultArray[4] * 100));
                        Dispatcher.Invoke(() => Result5.Content = string.Format("5 - {0:0.00}", resultArray[5] * 100));
                        Dispatcher.Invoke(() => Result6.Content = string.Format("6 - {0:0.00}", resultArray[6] * 100));
                        Dispatcher.Invoke(() => Result7.Content = string.Format("7 - {0:0.00}", resultArray[7] * 100));
                        Dispatcher.Invoke(() => Result8.Content = string.Format("8 - {0:0.00}", resultArray[8] * 100));
                        Dispatcher.Invoke(() => Result9.Content = string.Format("9 - {0:0.00}", resultArray[9] * 100));
                        byte[] imgArray = new byte[784];

                        for (int h = 0; h < 784; h++)
                        {
                            imgArray[h] = Convert.ToByte(strarray[h + 1]);
                        }
                        Dispatcher.Invoke(() => Picture.Source = MakeImage(imgArray));*/

                        var index = Array.IndexOf(resultArray, resultArray.Max());
                        if (index == Convert.ToInt32(number))
                        {
                            plus++;
                        }
                        else
                        {
                            minus++;
                        }
                        Dispatcher.Invoke(() => Error.Content = $"{plus}/{minus}");

                      //  Thread.Sleep(5000);
                    }
                }
            }
        }

        private void ViewQuery()
        {
            double[] array = new double[784];
            string number;
            int plus = 0;
            int minus = 0;
            using (var sr = new StreamReader(@"C:\Users\ivan.scoropad\Source\Repos\ps1Xt\NeuralNetworkWpf\Kursach NeuralNetwork\mnist_test.txt"))
            {
                for (int j = 0; j < 100; j++)
                {
                    for (int k = 0; k < 100; k++)
                    {
                        double[] numbers = new double[10];
                        var strarray = sr.ReadLine().Split(',');
                        number = strarray[0];

                        array = Normalize(strarray);
                        var resultArray = Neural.Query(array);
                        Dispatcher.Invoke(() => Result0.Content = string.Format("0 - {0:0.00}", resultArray[0] * 100));
                        Dispatcher.Invoke(() => Result1.Content = string.Format("1 - {0:0.00}", resultArray[1] * 100));
                        Dispatcher.Invoke(() => Result2.Content = string.Format("2 - {0:0.00}", resultArray[2] * 100));
                        Dispatcher.Invoke(() => Result3.Content = string.Format("3 - {0:0.00}", resultArray[3] * 100));
                        Dispatcher.Invoke(() => Result4.Content = string.Format("4 - {0:0.00}", resultArray[4] * 100));
                        Dispatcher.Invoke(() => Result5.Content = string.Format("5 - {0:0.00}", resultArray[5] * 100));
                        Dispatcher.Invoke(() => Result6.Content = string.Format("6 - {0:0.00}", resultArray[6] * 100));
                        Dispatcher.Invoke(() => Result7.Content = string.Format("7 - {0:0.00}", resultArray[7] * 100));
                        Dispatcher.Invoke(() => Result8.Content = string.Format("8 - {0:0.00}", resultArray[8] * 100));
                        Dispatcher.Invoke(() => Result9.Content = string.Format("9 - {0:0.00}", resultArray[9] * 100));
                        byte[] imgArray = new byte[784];

                        for (int h = 0; h < 784; h++)
                        {
                            imgArray[h] = Convert.ToByte(strarray[h + 1]);
                        }
                        Dispatcher.Invoke(() => Picture.Source = MakeImage(imgArray));

                        var index = Array.IndexOf(resultArray, resultArray.Max());
                        if (index == Convert.ToInt32(number))
                        {
                            plus++;
                        }
                        else
                        {
                            minus++;
                        }
                        Dispatcher.Invoke(() => Error.Content = $"{plus}/{minus}");

                        Thread.Sleep(5000);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Task(() => ViewQuery()).Start();
        }
    }
}
