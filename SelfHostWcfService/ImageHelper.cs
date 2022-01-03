using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SelfHost
{
    public class ImageHelper
    {
        public static string ImageToBase64(Image image, ImageFormat format)
        {
            if (image == null)
            {
                return string.Empty;
            }

            using (var ms = new MemoryStream())
            {
                image.Save(ms, format);
                var imageBytes = ms.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
    }
}