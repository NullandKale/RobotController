using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace RobotController
{
    public static class Utils
    {

        public static Color FromUnboundedRGB(int R, int G, int B)
        {
            if (R < 0)
            {
                R = 0;
            }
            else if (R > 255)
            {
                R = 255;
            }

            if (G < 0)
            {
                G = 0;
            }
            else if (G > 255)
            {
                G = 255;
            }

            if (B < 0)
            {
                B = 0;
            }
            else if (B > 255)
            {
                B = 255;
            }

            return Color.FromArgb(R, G, B);
        }
    }

    public static class ExtensionMethods
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }



    }

}
