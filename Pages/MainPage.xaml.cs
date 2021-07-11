//using Microsoft.AspNetCore.Components.Routing;
//using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Drawing;
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
using muxc = Microsoft.UI.Xaml.Controls;


// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace GraphicsUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    public sealed partial class MainPage : Page
    {
        static bool isInit = false;
        object lastPage;

        private ObservableCollection<NavLink> _navLinks = new ObservableCollection<NavLink>()
        {
            new NavLink() { Label = "People", Symbol = Symbol.People  },
            new NavLink() { Label = "Globe", Symbol = Symbol.Globe },
            new NavLink() { Label = "Message", Symbol = Symbol.Message },
            new NavLink() { Label = "Mail", Symbol = Symbol.Mail },
        };

        public ObservableCollection<NavLink> NavLinks
        {
            get { return _navLinks; }
        }
        public MainPage()
        {
            this.InitializeComponent();
            lastPage = NavView.MenuItems[0];
            //int canvasWidth = (int)myCanvas.ActualWidth;
            //int canvasHeight = (int)myCanvas.ActualHeight;
            //Line Xp = new Line(0, canvasHeight / 2, canvasWidth, canvasHeight / 2);    //X
            //DrawLine(canvasWidth / 2, 0, canvasWidth / 2, canvasHeight, 3);     //Y
        }
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        //导航加载时自动运行（？），选定起始选中导航页
        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
            var settings = (Microsoft.UI.Xaml.Controls.NavigationViewItem)NavView.SettingsItem;

            var fontIcon = new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = "\xE1C5"
            };

            settings.Icon = fontIcon;
        }

        //关键函数——导航页切换
        private void NavView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)    //按下重绘按钮
            {
                //是否应该释放原Canvas？——是

                myCanvas.Children.Clear();
                isInit = false;
                NavView.SelectedItem = lastPage;
                return;
                //Rectangle bg = new Rectangle
                //{
                //    Fill = new SolidColorBrush(Colors.Black),
                //    Width = myCanvas.ActualWidth,
                //    Height = myCanvas.ActualHeight + 40,        //解决画出界问题——nav导航高度为40
                //};
                ////bg.SetValue(Canvas.ZIndexProperty, 1);
                //bg.SetValue(Canvas.LeftProperty, 0);
                //bg.SetValue(Canvas.TopProperty, -40);
                //myCanvas.Children.Add(bg);

                //isInit = false;

                ////Object temp = NavView.SelectedItem;
                //NavView.SelectedItem = NavView.MenuItems[0];

            }
            if (!isInit)                    //绘制坐标轴
            {
                for (int i = -50; i <= 50; i++)
                {
                    DrawLine(i, 50, i, -50, 2, Colors.Gray, 0.4);    //垂直
                    DrawLine(-50, i, 50, i, 2, Colors.Gray, 0.4);    //水平
                }

                DrawLine(-50, 0, 50, 0, 2, Colors.White, 1);    //X轴
                DrawLine(0, 50, 0, -50, 2, Colors.White, 1);     //Y轴

                isInit = true;
            }

            var navItemTag = args.SelectedItemContainer.Tag.ToString(); //取被选中内容的Tag值并转化为String
                                                                        //以下代码冗余，待优化


            BasicNavPage drawOpsPage = new BasicNavPage();

            if (navItemTag == "line")
            {
                drawOpsPage = new LinePage();           //实例化导航栏中被选中内容对应的Page
                //Frame.Navigate(typeof(LinePage));                drawOpsPage.GetMyCanvasSize();
                //drawOpsPage.ParentWindow = this;                 //将主窗口设置为该Page的父窗口，从而使其可以调用主窗口内控件
                //drawOpsPage.GetMyCanvasSize();
                //drawingOpsFrame.Content = drawOpsPage;           //将该Page加载进主窗口的Frame中
                lastPage = NavView.MenuItems[0];
            }
            else if(navItemTag == "ellipse")
            {
                drawOpsPage = new EllipsePage();           //实例化该页
                //drawOpsPage.ParentWindow = this;                 //将主窗口设置为父窗口，从而调用主窗口内控件
                //drawOpsPage.GetMyCanvasSize(); 
                //drawingOpsFrame.Content = drawOpsPage;           //切换Frame内容
                lastPage = NavView.MenuItems[1];

            }
            else if(navItemTag == "boundryFill")            //特殊处理，因其需要调用重写的DrawPoint
            {
                drawOpsPage = new FillPage();
                //fillPage.ParentWindow = this;
                //fillPage.GetMyCanvasSize();
                //drawingOpsFrame.Content = fillPage;
                lastPage = NavView.MenuItems[2];
            }
            else if (navItemTag == "lineCliping")
            {
                drawOpsPage = new LineClipingPage();
                //drawOpsPage.ParentWindow = this;
                //drawOpsPage.GetMyCanvasSize();
                //drawingOpsFrame.Content = drawOpsPage;
                lastPage = NavView.MenuItems[3];
            }
            drawOpsPage.ParentWindow = this;
            drawOpsPage.GetMyCanvasSize();
            drawingOpsFrame.Content = drawOpsPage;
        }

        //private void drawingFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        //{

        //}

        //改变窗口大小自动重绘画布
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isInit)
                NavView.SelectedItem = NavView.SettingsItem;
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryResizeView(new Size(this.Width, this.Height));
        }

            //函数冗余，待优化
        public void DrawLine(int x1, int y1, int x2, int y2, int lineWidth, Color myColor, double op)
        {
            int halfCanvasWidth = (int)myCanvas.ActualWidth / 2;
            int halfCanvasHeight = (int)myCanvas.ActualHeight / 2;

            Line newLine = new Line
            {
                X1 = x1 * 20 + halfCanvasWidth,
                X2 = x2 * 20 + halfCanvasWidth,
                Y1 = y1 * -20 + halfCanvasHeight,
                Y2 = y2 * -20 + halfCanvasHeight,
                StrokeThickness = lineWidth,
                Opacity = op,
                Stroke = new SolidColorBrush(myColor)
            };
            //newLine.SetValue(Canvas.ZIndexProperty, 1);
            myCanvas.Children.Add(newLine);
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
    public class NavLink
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }
    }
}
