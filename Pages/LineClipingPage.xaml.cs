using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GraphicsUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LineClipingPage : BasicNavPage
    {
        public const int LEFT = 1;
        public const int RIGHT = 2;
        public const int BOTTOM = 4;
        public const int TOP = 8;

        private bool isDrawn = false;

        public LineClipingPage()
        {
            this.InitializeComponent();
        }

            //以下参数统一按照 左->上->右->下 排列
        //分区编码
        private int Encode(int x, int y, int xL, int yT, int xR, int yB)
        {
            int c = 0;
            if (x < xL) c |= LEFT;      //把c的二进制数对应位置置为1
            if (x > xR) c |= RIGHT;
            if (y < yB) c |= BOTTOM;
            if (y > yT) c |= TOP;
            return c;
        }

        //CS直线裁剪算法
        private void CohenSutherlandLineClip(int x1, int y1, int x2, int y2, int xL, int yT, int xR, int yB)        //(x1,y1)(x2,y2)为线段的端点坐标，其他四个参数定义窗口的边界
        {
            int code1, code2, code;
            int xNew = 0, yNew = 0;
            code1 = Encode(x1, y1, xL, yT, xR, yB);
            code2 = Encode(x2, y2, xL, yT, xR, yB);

            while (code1 != 0 || code2 != 0)        //若两编码不全为0，继续循环
            {
                if ((code1 & code2) != 0)   //若两编码相与后不为0，说明构成直线的两点在某个裁剪边框的同侧，直接舍去
                    return;

                code = code1;
                if (code1 == 0) 
                    code = code2;           //取不为0（即不在裁剪框内）的点的编码进行运算

                if ((LEFT & code) != 0)     //若该点在裁剪框左侧，下同
                {
                    xNew = xL;
                    yNew = y1 + (y2 - y1) * (xL - x1) / (x2 - x1);      //相似三角形法，加上原始y1，得到在裁剪框上的新y1，下类似
                }
                else if ((RIGHT & code) != 0)
                {
                    xNew = xR;
                    yNew = y1 + (y2 - y1) * (xR - x1) / (x2 - x1);
                }
                else if ((BOTTOM & code) != 0)
                {
                    yNew = yB;
                    xNew = x1 + (x2 - x1) * (yB - y1) / (y2 - y1);
                }
                else if ((TOP & code) != 0)
                {
                    yNew = yT;
                    xNew = x1 + (x2 - x1) * (yT - y1) / (y2 - y1);
                }

                if (code == code1)          //若参与运算的编码是点1的编码，更新点1及点1编码
                {
                    x1 = xNew;
                    y1 = yNew;
                    code1 = Encode(xNew, yNew, xL, yT, xR, yB);
                }
                else                        //否则更新点2
                {
                    x2 = xNew;
                    y2 = yNew;
                    code2 = Encode(xNew, yNew, xL, yT, xR, yB);
                }
            }
            _ = DDALineAsync(x1, y1, x2, y2, newLineColor);
        }


        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(x1_control.Text, out int x1);
            int.TryParse(y1_control.Text, out int y1);
            int.TryParse(x2_control.Text, out int x2);
            int.TryParse(y2_control.Text, out int y2);

            if (x1 < -21 || x2 < -21 || y1 < -21 || y2 < -21 || x1 > 21 || x2 > 21 || y1 > 21 || y2 > 21)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                return;
            }

            _ = DDALineAsync(x1, y1, x2, y2, originalLineColor);
            isDrawn = true;
        }

        private async void ClipButton_Click(object sender, RoutedEventArgs e)
        {
            //若还未绘制直线，给出提示
            if (!isDrawn)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                return;
            }

            //取直线两点
            int.TryParse(x1_control.Text, out int x1);
            int.TryParse(y1_control.Text, out int y1);
            int.TryParse(x2_control.Text, out int x2);
            int.TryParse(y2_control.Text, out int y2);

            //取裁剪框两点（左上右下）
            int.TryParse(xLeft_control.Text, out int xLeft);
            int.TryParse(yTop_control.Text, out int yTop);
            int.TryParse(xRight_control.Text, out int xRight);
            int.TryParse(yBottom_control.Text, out int yBottom);

            //从左上点顺时针绘制裁剪框
            await DDALineAsync(xLeft, yTop, xRight, yTop, clipRectColor);
            await DDALineAsync(xRight, yTop, xRight, yBottom, clipRectColor);
            await DDALineAsync(xRight, yBottom, xLeft, yBottom, clipRectColor);
            await DDALineAsync(xLeft, yBottom, xLeft, yTop, clipRectColor);

            //调用CS算法裁剪直线
            CohenSutherlandLineClip(x1, y1, x2, y2, xLeft, yTop, xRight, yBottom);

            isDrawn = false;
        }

    }
}
