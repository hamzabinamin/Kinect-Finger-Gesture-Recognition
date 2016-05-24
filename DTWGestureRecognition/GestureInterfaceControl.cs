using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DTWGestureRecognition
{
    internal class GestureInterfaceControl
    {

        public static void executeGesture(string gestureName)
        {
            String left = "@Left";
            String right = "@Right";
            String up = "@Up";
            String down = "@Down";
            Debug.WriteLine(gestureName);
           
            if (gestureName.Trim().Equals(left, StringComparison.Ordinal))
            {
                Debug.WriteLine("Left");
                moveLeft();
                return;
            }
            if (gestureName.Trim().Equals(right, StringComparison.Ordinal))
            {
                Debug.WriteLine("Right");
                moveRight();
                return;
            }
            if (gestureName.Trim().Equals(up, StringComparison.Ordinal))
            {
                Debug.WriteLine("Up");
                moveUp();
                return;
            }
            if (gestureName.Trim().Equals(down, StringComparison.Ordinal))
            {
                Debug.WriteLine("Down");
                moveDown();
                return;
            }
        }

        public static void moveLeft(){
            System.Windows.Forms.SendKeys.SendWait("{A}");
        }
        public static void moveRight()
        {
            System.Windows.Forms.SendKeys.SendWait("{D}");
        }
        public static void moveUp()
        {
            System.Windows.Forms.SendKeys.SendWait("{Up}");
        }
        public static void moveDown()
        {
            System.Windows.Forms.SendKeys.SendWait("{Down}");
        }
    }
}
