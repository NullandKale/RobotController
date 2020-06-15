using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;

namespace RobotController.Robot
{
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

        public static Bitmap BitmapFromData(byte[] data)
        {
            Bitmap bmp = new Bitmap(160, 120);

            for (int i = 0; i < 160 * 120 * 3; i += 3)
            {
                Color c = Color.FromArgb(data[i], data[i + 1], data[i + 2]);
                bmp.SetPixel((i / 3) % 160, (i / 3) / 160, c);
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
