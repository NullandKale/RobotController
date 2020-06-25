using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;

namespace RobotController.Robot
{
    [Serializable]
    public struct datapoint
    {
        public int tick;
        public float battery;
        public float range;
        public float AngleX;
        public float AngleY;
        public float AngleZ;
        public float KAngleX;
        public float KAngleY;
        public float KAngleZ;
        public float aX;
        public float aY;
        public float aZ;
        public float gX;
        public float gY;
        public float gZ;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 57600)]
        public byte[] imageData;
        public int currentMotorL;
        public int currentMotorR;
        public int currentServo1;
        public int currentServo2;

        public static datapoint Deserialize(byte[] rawData, int position)
        {
            int rawsize = Marshal.SizeOf(typeof(datapoint));
            if (rawsize > rawData.Length)
                return default(datapoint);

            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            datapoint obj = (datapoint)Marshal.PtrToStructure(buffer, typeof(datapoint));
            Marshal.FreeHGlobal(buffer);
            return obj;
        }

        public static byte[] Serialize(datapoint item)
        {
            int rawSize = Marshal.SizeOf(typeof(datapoint));
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(item, buffer, false);
            byte[] rawData = new byte[rawSize];
            Marshal.Copy(buffer, rawData, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawData;
        }

        public static byte[] ConvertBytestoJpegBytes(byte[] pixels24bpp, int W, int H)
        {
            GCHandle gch = GCHandle.Alloc(pixels24bpp, GCHandleType.Pinned);
            int stride = 4 * ((24 * W + 31) / 32);
            Bitmap bmp = new Bitmap(W, H, stride, PixelFormat.Format24bppRgb, gch.AddrOfPinnedObject());
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            gch.Free();
            return ms.ToArray();
        }

        public static Bitmap BitmapFromJPEGData(byte[] data)
        {
            Bitmap bmp = new Bitmap(160, 120);

            using (Image raw = Image.FromStream(new MemoryStream(data)))
            {
                bmp = raw.Clone() as Bitmap;
                //bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            }

            return bmp;
        }

        public static Bitmap BitmapFromData(byte[] data, Color[,] blackOffset)
        {
            Bitmap bmp = new Bitmap(160, 120);

            for (int i = 0; i < 160 * 120 * 3; i += 3)
            {
                Color c = Utils.FromUnboundedRGB(data[i], data[i + 1], data[i + 2]);

                int x = ((i / 3) % 160);
                int y = ((i / 3) / 160);

                if ((i / 3) / 160 >= 48)
                {
                    c = Color.FromArgb(c.B, c.G, c.R);
                }

                if (blackOffset != null)
                {
                    float correctedR = c.R;
                    float correctedB = c.B;
                    float correctedG = c.G;

                    int xR = 159 - x;
                    int yR = 119 - y;

                    correctedR = correctedR.Remap(blackOffset[xR, yR].R, 255, 0, 255);
                    correctedG = correctedG.Remap(blackOffset[xR, yR].G, 255, 0, 255);
                    correctedB = correctedB.Remap(blackOffset[xR, yR].B, 255, 0, 255);

                    c = Utils.FromUnboundedRGB((int)correctedR, (int)correctedG, (int)correctedB);
                }

                bmp.SetPixel(x, y, c);
            }

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY);

            return bmp;
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}
