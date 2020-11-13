using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.CustomAttributes;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using muxc = Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GraphicsUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FillPage : Page
    {
        private int pointCount;
        private Stack<NumberBox> numberBoxes = new Stack<NumberBox>();
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);
        public FillPage()
        {
            this.InitializeComponent();
            pointCount = 3;
            numberBoxes.Push(x1_text);
            numberBoxes.Push(y1_text);
            numberBoxes.Push(x2_text);
            numberBoxes.Push(y2_text);
            numberBoxes.Push(x3_text);
            numberBoxes.Push(y3_text);
        }

        //用于切换页面
        private MainPage _parentWin;
        public MainPage ParentWindow
        {
            get { return _parentWin; }
            set { _parentWin = value; }
        }

        //在当前扫描线上下两端寻找新的种子
        //stack为系统堆栈
        void FindNewSeed(Stack<Point> s, int left, int right, int y, Color borderColor, Color innerColor)
        {
            for (int i = left + 1; i < right; i++)
            {
                if (GetPixelColor(i, y) != borderColor && GetPixelColor(i, y) != innerColor)
                {
                    int j = i + 1;
                    while (GetPixelColor(j, y) != borderColor)
                        j++;
                    i = j--;
                    s.Push(new Point(j, y));
                }
            }
        }
        //扫描线种子填充算法
        void ScanLineBoundryFill(int x, int y, Color borderColor, Color innerColor)
        {
            Stack<Point> s = new Stack<Point>();
            Point p;
            int left, right;
            s.Push(new Point(x, y));
            while (s.Count() != 0)
            {
                //栈顶元素出栈
                p = s.Pop();

                //向左填充
                for (left = (int)p.X; GetPixelColor(left, (int)p.Y) != borderColor; left--)
                    DrawPoint(left, (int)p.Y, innerColor);
                //向右填充
                for (right = (int)p.X + 1; GetPixelColor(right, (int)p.Y) != borderColor; right++)
                    DrawPoint(right, (int)p.Y, innerColor);

                //在当前行的下一行寻找确定新的种子点
                FindNewSeed(s, left, right, (int)p.Y + 1, borderColor, innerColor);
                //在当前行的上一行寻找确定新的种子点
                FindNewSeed(s, left, right, (int)p.Y - 1, borderColor, innerColor);
            }
        }
        public Color GetPixelColor(int x, int y)
        {
            int halfCanvasWidth = (int)ParentWindow.myCanvas.ActualWidth / 2;
            int halfCanvasHeight = (int)ParentWindow.myCanvas.ActualHeight / 2;

            //Graphics g = ParentWindow.myCanvas.CreateGraphics();
            //Point p = new Point(x * 20 + halfCanvasWidth, y * -20 + halfCanvasHeight);

            //IntPtr hdc = GetDC(ParentWindow.myCanvas.Handle);
            IntPtr hdc = GetWindowDC(this.Handle);
            uint pixel = GetPixel(hdc, x * 20 + halfCanvasWidth, y * -20 + halfCanvasHeight + 40);
            //ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromArgb(255,
                         (byte)(pixel & 0x000000FF),
                         (byte)((pixel & 0x0000FF00) >> 8),
                         (byte)((pixel & 0x00FF0000) >> 16));
            return color;
        }


        //增加一个点
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (pointCount >= 8)   //限制点的数量
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                return;
            }

            pointCount++;
            NumberBox pointX = new NumberBox
            {
                Name = "x" + pointCount + "_text",
                Header = "Enter x" + pointCount + ":",
                Width = 100,
                Value = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                SmallChange = 2,
                LargeChange = 10,
            };
            NumberBox pointY = new NumberBox
            {
                Name = "y" + pointCount + "_text",
                Header = "Enter y" + pointCount + ":",
                Width = 100,
                Value = 0,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                SmallChange = 2,
                LargeChange = 10,
            };
            //点xy坐标顺序入栈
            numberBoxes.Push(pointX);
            numberBoxes.Push(pointY);

            PointsGrid.Children.Add(pointX);
            Grid.SetRow(pointX, pointCount - 1);

            PointsGrid.Children.Add(pointY);
            Grid.SetRow(pointY, pointCount - 1);
            Grid.SetColumn(pointY, 1);


        }
        //减少一个点
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (pointCount == 3)   //限制点的数量
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                return;
            }
            pointCount--;
            NumberBox deleteY = numberBoxes.Pop();
            NumberBox deleteX = numberBoxes.Pop();
            PointsGrid.Children.Remove(deleteX);
            PointsGrid.Children.Remove(deleteY);
            deleteX = null;
            deleteY = null;
        }

        //该方法加async使得主线程（主界面）继续运行，不会未响应
        //连接所有点
        private async void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            //NumberBox p1X, p1Y, p2X, p2Y;
            //foreach(NumberBox p in PointsGrid)

            // 遍历Grid下前pointCount个控件
            for (int i = 3; i < pointCount * 2; i += 2) 
            {
                //p1X = PointsGrid.Children[i] as NumberBox;
                //p1Y = PointsGrid.Children[i] as NumberBox;

                int.TryParse((PointsGrid.Children[i - 3] as NumberBox).Text, out int x1);
                int.TryParse((PointsGrid.Children[i - 2] as NumberBox).Text, out int y1);
                int.TryParse((PointsGrid.Children[i - 1] as NumberBox).Text, out int x2);
                int.TryParse((PointsGrid.Children[i] as NumberBox).Text, out int y2);

                ////---重点---：用t记录该异步方法执行状态
                //Task t = DDALineAsync(x1, y1, x2, y2, Colors.SkyBlue);
                ////等待t（即DDALineAsync）执行完毕
                //t.Wait();

                //此处加上await等待该方法执行完毕后再进行
                await DDALineAsync(x1, y1, x2, y2, Colors.SkyBlue);
            }
            //画最后一笔，封闭图形
            int.TryParse((PointsGrid.Children[0] as NumberBox).Text, out int x0);
            int.TryParse((PointsGrid.Children[1] as NumberBox).Text, out int y0);
            int.TryParse((PointsGrid.Children[pointCount * 2 - 2] as NumberBox).Text, out int xEnd);
            int.TryParse((PointsGrid.Children[pointCount * 2 - 1] as NumberBox).Text, out int yEnd);
            //Task t1 = DDALineAsync(xEnd, yEnd, x0, y0, Colors.SkyBlue);
            //t1.Wait();
            await DDALineAsync(xEnd, yEnd, x0, y0, Colors.SkyBlue);

        }
        //填充
        private void FillButton_Click(object sender, RoutedEventArgs e)
        {
            //ScanLineBoundryFill(0, 0, Colors.SkyBlue, Colors.Red);
        }






                    //以下内容多次使用，待抽象为基类
        //画点
        public void DrawPoint(int x, int y, Color myColor)
        {
            int halfCanvasWidth = (int)ParentWindow.myCanvas.ActualWidth / 2;
            int halfCanvasHeight = (int)ParentWindow.myCanvas.ActualHeight / 2;

            Ellipse myPoint = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(myColor),
                Stroke = new SolidColorBrush(myColor)
            };
            ParentWindow.myCanvas.??????????;

            //关键句——设置作为点的椭圆位置
            myPoint.SetValue(Canvas.ZIndexProperty, 1);
            myPoint.SetValue(Canvas.LeftProperty, x * 20 - 3 + halfCanvasWidth);        //在此处传入的参数为抽象坐标对应的实际坐标
            myPoint.SetValue(Canvas.TopProperty, y * -20 - 3 + halfCanvasHeight);       //-3取消点自身大小(6)造成的误差

            ParentWindow.myCanvas.Children.Add(myPoint);
        }

        public async Task DDALineAsync(int x1, int y1, int x2, int y2, Color myColor)
        {
            double increx, increy, x, y;
            int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));

            increx = (double)(x2 - x1) / steps;     //此两个变量中有一个为1
            increy = (double)(y2 - y1) / steps;
            x = x1;
            y = y1;

            for (int i = 0; i <= steps; i++)
            {
                await Task.Delay(150);

                //此处需取绝对值，避免负数问题
                if (Math.Abs(Math.Abs(increx) - 1) <= 1e-6)  //斜率-1~1
                    if (y >= 0)
                        DrawPoint((int)x, (int)(y + 0.5), myColor);
                    else
                        DrawPoint((int)x, (int)Math.Ceiling(y - 0.5), myColor);  //Ceiling——取比当前数大的最小整数
                else
                    if (x >= 0)
                    DrawPoint((int)(x + 0.5), (int)y, myColor);
                else
                    DrawPoint((int)Math.Ceiling(x - 0.5), (int)y, myColor);
                x += increx;
                y += increy;
            }
        }
    }
}
