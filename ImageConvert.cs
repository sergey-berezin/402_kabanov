using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using YOLOv4MLNet.DataStructures;


namespace WpfAppDKab
{
 
    public class ImageConvert : YoloV4Result
    {
        public CroppedBitmap Image { get; private set; }
        public ImageConvert(YoloV4Result res) : base(res.BBox, res.Label)
        {
            filename = res.Filename;
            CreateImage();
        }

        private void CreateImage()
        {
            Uri filePath = new(filename, UriKind.RelativeOrAbsolute);
            BitmapImage fileImage = new(filePath);
            fileImage.Freeze();
            Int32Rect newArea = new Int32Rect((int)BBox[0], (int)BBox[1], (int)(BBox[2] - BBox[0]), (int)(BBox[3] - BBox[1]));
            Image = new CroppedBitmap(fileImage, newArea);
            Image.Freeze();
        }

        public static byte[] ByteConverter(BitmapSource bitmapSource)
        {
            byte[] byteImage;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 100;
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(ms);
                byteImage = ms.ToArray();
                ms.Close();
            }
            return byteImage;
        }
    }
    public class YoloItem : Row
    {
        public YoloItem(YoloV4Result res)
        {
            ImageConvert imageRes = new(res);
            x = imageRes.BBox[0];
            x = imageRes.BBox[1];
            l = imageRes.BBox[2] - y;
            w = imageRes.BBox[3] - y;
            Image = ImageConvert.ByteConverter(imageRes.Image);
            Label = imageRes.Label;
        }
    }
}
