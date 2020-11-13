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
    public sealed partial class FillPage : BasicNavPage
    {
        private int pointCount;                     //记录多边形由几个点构成，从而控制控件数量
        private bool isDrawn = false;               //记录是否绘制了多边形，从而避免未绘制多边形就点击填充按钮
        private bool isFilled = false;              //记录是否填充了多边形

        private Stack<NumberBox> numberBoxes = new Stack<NumberBox>();  //存NumberBox控件

        private bool[,] isEdge = new bool[45, 45];  //记录-22~22的某点处是否为多边形的边
        private int[] limits = new int[4]           //构造矩形框架，分别记录xMin,yMin,xMax,yMax，用于判断种子点有效性时限定范围    （待改进）
        {
            22,22,-22,-22
        };


        //[DllImport("user32.dll")]
        //static extern IntPtr GetDC(IntPtr hwnd);

        //[DllImport("user32.dll")]
        //static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        //[DllImport("gdi32.dll")]
        //static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);
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


                                /*------------------------算法---------------------*/
        //扫描线种子填充算法
        private async Task ScanLineSeedFill(int x, int y, Color innerColor)
        {
            Stack<Point> s = new Stack<Point>();    //用于保存种子
            Point p;
            int left, right;
            s.Push(new Point(x, y));    //初始种子入栈
            while (s.Count() != 0)      //栈不为空持续循环
            {
                p = s.Pop();            //栈顶元素出栈

                //向左填充
                for (left = (int)p.X; !isEdge[left + 22, (int)p.Y + 22]; left--)
                {
                    DrawPoint(left, (int)p.Y, innerColor);
                    await Task.Delay(30);
                }
                //向右填充
                for (right = (int)p.X + 1; !isEdge[right + 22, (int)p.Y + 22]; right++)
                {
                    DrawPoint(right, (int)p.Y, innerColor);
                    await Task.Delay(30);
                }

                FindNewSeed(s, left, right, (int)p.Y + 1);     //在当前行的下一行寻找新的种子点
                FindNewSeed(s, left, right, (int)p.Y - 1);     //在当前行的上一行寻找新的种子点
            }
        }

        //在当前扫描线上下两端寻找新的种子
        private void FindNewSeed(Stack<Point> s, int left, int right, int y)
        {
            for (int i = left + 1; i < right; i++)      //从左至右，两段循环
            {
                if (!isEdge[i + 22, y + 22])        // && GetPixelColor(i, y) != innerColor
                {
                    int j = i + 1;
                    while (!isEdge[j + 22, y + 22])     //当碰到边时内循环结束
                        j++;
                    i = j--;                            //取边界前一个点为种子点，入栈
                    s.Push(new Point(j, y));
                }
            }
        }
        
        //判断种子是否在多边形内
        private bool IsSeedInPolygon(int seedX, int seedY)
        {
            if (isEdge[seedX + 22, seedY + 22])
                return false;

            //bool lastIsEdge = false;
            bool result = false;
            for (int arrayX = seedX + 22, arrayY = seedY + 22; arrayX < limits[2] + 22; arrayX++)  //向右发出一条射线，从种子点遍历至最右侧点
            {
                if(isEdge[arrayX, arrayY])   //当前位置是边
                {
                    //必须保证经过的点对应的两条边不在同侧
                    //（未处理U字形，当前逻辑下U字形将重新获取种子）
                    if ((isEdge[arrayX - 1, arrayY + 1] || isEdge[arrayX, arrayY + 1] || isEdge[arrayX + 1, arrayY + 1]) &&    //当前位置上三个点
                        (isEdge[arrayX - 1, arrayY - 1] || isEdge[arrayX, arrayY - 1] || isEdge[arrayX + 1, arrayY - 1]))      //当前位置下三个点
                        result = !result;
                }
            }

            if(result)//若此时判断为有效种子点，为了增大判断成功概率（处理点挤在一起的U形情况），再向左再发出一条射线，进行同样的判断
            {
                result = false;
                for (int arrayX = seedX + 22, arrayY = seedY + 22; arrayX > limits[0] + 22; arrayX--)
                {
                    if (isEdge[arrayX, arrayY])   //当前位置是边
                    {
                        //必须保证经过的点对应的两条边不在同侧
                        if ((isEdge[arrayX - 1, arrayY + 1] || isEdge[arrayX, arrayY + 1] || isEdge[arrayX + 1, arrayY + 1]) &&    //当前位置上三个点
                            (isEdge[arrayX - 1, arrayY - 1] || isEdge[arrayX, arrayY - 1] || isEdge[arrayX + 1, arrayY - 1]))      //当前位置下三个点
                            result = !result;
                    }
                }
            }
            return result;
        }

        //public Color GetPixelColor(int x, int y)
        //{
        //    int halfCanvasWidth = (int)ParentWindow.myCanvas.ActualWidth / 2;
        //    int halfCanvasHeight = (int)ParentWindow.myCanvas.ActualHeight / 2;

        //    //Graphics g = ParentWindow.myCanvas.CreateGraphics();
        //    //Point p = new Point(x * 20 + halfCanvasWidth, y * -20 + halfCanvasHeight);

        //    IntPtr hdc = GetDC(IntPtr.Zero);
        //    uint pixel = GetPixel(hdc, x * 20 + halfCanvasWidth, y * -20 + halfCanvasHeight + 40);
        //    //ReleaseDC(IntPtr.Zero, hdc);
        //    Color color = Color.FromArgb(255,
        //                 (byte)(pixel & 0x000000FF),
        //                 (byte)((pixel & 0x0000FF00) >> 8),
        //                 (byte)((pixel & 0x00FF0000) >> 16));
        //    return color;
        //}


                            /*------------------------按钮---------------------*/
        //按钮Add——增加一个点
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

            numberBoxes.Push(pointX);            //点xy坐标顺序入栈
            numberBoxes.Push(pointY);

            PointsGrid.Children.Add(pointX);
            Grid.SetRow(pointX, pointCount - 1);

            PointsGrid.Children.Add(pointY);
            Grid.SetRow(pointY, pointCount - 1);
            Grid.SetColumn(pointY, 1);


        }

        //按钮Delete——减少一个点
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

        //按钮LinkPoints——连接所有点
        private async void LinkButton_Click(object sender, RoutedEventArgs e)            //该方法加async使得主线程（主界面）继续运行，不会未响应
        {
            if (isDrawn || isFilled)    //clean
                ;

            // 遍历Grid下前pointCount * 2个控件
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


                await DDALineAsync(x1, y1, x2, y2, polygonEdgeColor);     //此处加上await等待该方法执行完毕后再进行
            }
            //画最后一笔，封闭图形
            int.TryParse((PointsGrid.Children[0] as NumberBox).Text, out int x0);
            int.TryParse((PointsGrid.Children[1] as NumberBox).Text, out int y0);
            int.TryParse((PointsGrid.Children[pointCount * 2 - 2] as NumberBox).Text, out int xEnd);
            int.TryParse((PointsGrid.Children[pointCount * 2 - 1] as NumberBox).Text, out int yEnd);
            //Task t1 = DDALineAsync(xEnd, yEnd, x0, y0, Colors.SkyBlue);
            //t1.Wait();
            await DDALineAsync(xEnd, yEnd, x0, y0, polygonEdgeColor);

            isDrawn = true;
        }

        //按钮Fill——填充
        private async void FillButton_Click(object sender, RoutedEventArgs e)
        {
            if(!isDrawn)    //若还没绘制多边形，调出浮窗并返回
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                return;
            }

            int seedX, seedY;
            int count = 0;
            do
            {
                Random rd = new Random(unchecked((int)DateTime.Now.Ticks));
                seedX = rd.Next(limits[0] + 1, limits[2]);  //矩形范围内随机取点
                seedY = rd.Next(limits[1] + 1, limits[3]);

                if (count++ > 100)
                {
                    DisplayFailedToFindSeedDialog();
                    return;
                }
            }
            while (!IsSeedInPolygon(seedX, seedY));  //随机出有效种子

            await ScanLineBoundryFill(seedX, seedY, innerColor);

            isEdge = new bool[45, 45];  //绘制结束，重置多边形边界数组
            limits = new int[4]         //          重置矩形框架边界
            {
                22,22,-22,-22
            };
            isDrawn = false;
            isFilled = true;
        }


        private async void DisplayFailedToFindSeedDialog()
        {
            ContentDialog noSeedDialog = new ContentDialog
            {
                Title = "未找到合适的种子点",
                Content = "请检查多边形构成并尝试重绘。",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noSeedDialog.ShowAsync();
        }





        //重写DrawPoint——画点*且在isEdge数组中标点*
        private new void DrawPoint(int x, int y, Color myColor)
        {
            isEdge[x + 22, y + 22] = true;  //为当前点设置边标志
            
            //更新边框
            limits[0] = Math.Min(x - 1, limits[0]);
            limits[1] = Math.Min(y - 1, limits[1]);
            limits[2] = Math.Max(x + 1, limits[2]);
            limits[3] = Math.Max(y + 1, limits[3]);

            Ellipse myPoint = new Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = new SolidColorBrush(myColor),
                Stroke = new SolidColorBrush(myColor)
            };

            //关键句——设置作为点的椭圆位置
            //myPoint.SetValue(Canvas.ZIndexProperty, 1);
            myPoint.SetValue(Canvas.LeftProperty, x * 20 - 3 + halfCanvasWidth);        //在此处传入的参数为抽象坐标对应的实际坐标
            myPoint.SetValue(Canvas.TopProperty, y * -20 - 3 + halfCanvasHeight);       //-3取消点自身大小(6)造成的误差

            ParentWindow.myCanvas.Children.Add(myPoint);
        }

        //重写DDA算法画线_Async版本——为了调用该类中的DrawLine函数
        //更好的解决方法？
        public new async Task DDALineAsync(int x1, int y1, int x2, int y2, Color myColor)
        {
            double increx, increy, x, y;
            int steps = Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));

            increx = (double)(x2 - x1) / steps;     //此两个变量中有一个为1
            increy = (double)(y2 - y1) / steps;
            x = x1;
            y = y1;

            for (int i = 0; i <= steps; i++)
            {
                await Task.Delay(80);

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
