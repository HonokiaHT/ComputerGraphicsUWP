using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;


// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace GraphicsUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            int canvasWidth = (int)myCanvas.ActualWidth;
            int canvasHeight = (int)myCanvas.ActualHeight;
            DrawLine(0, canvasHeight / 2, canvasWidth, canvasHeight / 2, 3);    //X
            DrawLine(canvasWidth / 2, 0, canvasWidth / 2, canvasHeight, 3);     //Y
        }

        public void DrawLine(int x1, int y1, int x2, int y2, int lineWidth)
        //未加颜色
        {
            Line newLine = new Line();
            newLine.X1 = x1;
            newLine.X2 = x2;
            newLine.Y1 = y1;
            newLine.Y2 = y2;
            newLine.StrokeThickness = lineWidth;
            newLine.Stroke = new SolidColorBrush(Windows.UI.Colors.White);
            grids.Children.Add(newLine);
        }

        public void DDALine(int x1, int y1, int x2, int y2)
        {
            double increx, increy, x ,y;
            int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
            increx = (double)(x2 - x1) / steps;     //此两个变量中有一个为1
            increy = (double)(y2 - y1) / steps;
            x = x1;
            y = y1;

            var polyline1 = new Polyline();     //绘制多段线
            polyline1.Stroke = new SolidColorBrush(Windows.UI.Colors.White);
            polyline1.StrokeThickness = 1;

            var points = new PointCollection(); //创建点集并加入点
            for (int i = 1; i <= steps; i++)
            {
                //PutPixel((int)x, (int)y, Colors.White); //在(x，y)处，以color色画点
                points.Add(new Point((int)x, (int)y));
                x += increx * 10;
                y += increy * 10;
                //System.Threading.Thread.Sleep(300);
            }

            polyline1.Points = points;
            grids.Children.Add(polyline1);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            int canvasWidth = (int)myCanvas.ActualWidth;
            int canvasHeight = (int)myCanvas.ActualHeight;
            int.TryParse(x1_text.Text, out int x1);
            int.TryParse(y1_text.Text, out int y1);
            int.TryParse(x2_text.Text, out int x2);
            int.TryParse(y2_text.Text, out int y2);

            if (x1 < -1 * canvasWidth / 2  || x2 < -1 * canvasWidth / 2 || y1 < -1 * canvasHeight / 2 || y2 < -1 * canvasHeight / 2 || x1 > canvasWidth / 2 || x2 > canvasWidth / 2 || y1 > canvasHeight / 2 || y2 > canvasHeight / 2)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                return;
            }

            DDALine(x1 + canvasWidth / 2 , -1 * y1 + canvasHeight / 2, x2 + canvasWidth / 2, -1 * y2 + canvasHeight / 2);
        }

        //private async void DisplayTestDialog(int x, int y)
        //{
        //    ContentDialog testDialog = new ContentDialog
        //    {
        //        Title = "x and y",
        //        Content = " " + x + " " + y,
        //        CloseButtonText = "Ok"
        //    };

        //    ContentDialogResult result = await testDialog.ShowAsync();
        //}

        //public void PutPixel(int x, int y, Color color)
        //{
        //    var ellipse1 = new Ellipse();
        //    var point = new Point(x, y);
        //    grids.Children.Add(point);
        //    SolidColorBrush myBrush = new SolidColorBrush(color);
        //    ellipse1.Fill = myBrush;
        //    ellipse1.Width = 30;
        //    ellipse1.Height = 30;
        //    //ellipse1.CenterPoint = new Vector3(x, y, 0);


        //    grids.Children.Add(ellipse1);
        //}
    }

}
