using System;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.Diagnostics;

namespace DTWGestureRecognition
{
    internal class MouseControl
    {
        private const float ClickHoldingRectThreshold = 0.05f;
        private Rect _clickHoldingLastRect;
        private readonly Stopwatch _clickHoldingTimer;
        private int cursorX = 850;
        private int cursorY = 520;
         private const float SkeletonMaxX = 0.60f;
        private const float SkeletonMaxY = 0.40f;

        public MouseControl(){
            _clickHoldingTimer = new Stopwatch();

    }
        public void moveMouse(float y, float x)
        {
            //var cursorX = (int)((float)x*1900/20  );
            //var cursorY = (int)((float)y*1080/20 );
            if (cursorX + x < 0)
            {
                cursorX = 0;
            }else
                if (cursorX + x > 1920)
                {
                    cursorX = 1920;
                }
                else
                {
                    cursorX = ((int)((float)cursorX + x));
                }
            if (cursorX + y < 0)
            {
                cursorX = 0;
            }else
                if (cursorY + y > 1050)
                {
                    cursorY = 1050;
                }
                else
                {
                    cursorY = ((int)((float)cursorY + y));
                }
            
           
            var leftClick = CheckForClickHold(cursorX, cursorY);
            MouseNativeMethods.SendMouseInput(cursorX, cursorY, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, leftClick);
        }

        private bool CheckForClickHold(int xx, int yy)
        {
            // This does one handed click when you hover for the allotted time.  It gives a false positive when you hover accidentally.
            var x = xx;
            var y = yy;

            var screenwidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenheight = (int)SystemParameters.PrimaryScreenHeight;
            var clickwidth = (int)(screenwidth * ClickHoldingRectThreshold);
            var clickheight = (int)(screenheight * ClickHoldingRectThreshold);

            var newClickHold = new Rect(x - clickwidth, y - clickheight, clickwidth * 2, clickheight * 2);

            if (_clickHoldingLastRect != Rect.Empty)
            {
                if (newClickHold.IntersectsWith(_clickHoldingLastRect))
                {
                    if ((int)_clickHoldingTimer.ElapsedMilliseconds > (1000 * 1000))
                    {
                        _clickHoldingTimer.Stop();
                        _clickHoldingLastRect = Rect.Empty;
                        return true;
                    }

                    if (!_clickHoldingTimer.IsRunning)
                    {
                        _clickHoldingTimer.Reset();
                        _clickHoldingTimer.Start();
                    }
                    return false;
                }

                _clickHoldingTimer.Stop();
                _clickHoldingLastRect = newClickHold;
                return false;
            }

            _clickHoldingLastRect = newClickHold;
            if (!_clickHoldingTimer.IsRunning)
            {
                _clickHoldingTimer.Reset();
                _clickHoldingTimer.Start();
            }
            return false;
        }

    }

}
