using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Merges multiple TIFF files (including multipage TIFFs) into a single multipage TIFF file.
        /// </summary>
        public static void MergeTiff(List<Image> tiffFiles,string outputFile)
        {
            byte[] tiffMerge = null;
            using (var msMerge = new MemoryStream())
            {
                //get the codec for tiff files
                ImageCodecInfo ici = null;
                foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders())
                    if (i.MimeType == "image/tiff")
                        ici = i;

                Encoder enc = Encoder.SaveFlag;
                EncoderParameters ep = new EncoderParameters(1);

                Bitmap pages = null;
                int frame = 0;

                foreach (var tiffFile in tiffFiles)
                {
                    using (var imageStream = new MemoryStream())
                    {
                        tiffFile.Save(imageStream, ImageFormat.Tiff);
                        using (Image tiffImage = Image.FromStream(imageStream))
                        {
                            foreach (Guid guid in tiffImage.FrameDimensionsList)
                            {
                                //create the frame dimension 
                                FrameDimension dimension = new FrameDimension(guid);
                                //Gets the total number of frames in the .tiff file 
                                int noOfPages = tiffImage.GetFrameCount(dimension);

                                for (int index = 0; index < noOfPages; index++)
                                {
                                    FrameDimension currentFrame = new FrameDimension(guid);
                                    tiffImage.SelectActiveFrame(currentFrame, index);
                                    using (MemoryStream tempImg = new MemoryStream())
                                    {
                                        tiffImage.Save(tempImg, ImageFormat.Tiff);
                                        {
                                            if (frame == 0)
                                            {
                                                //save the first frame
                                                pages = (Bitmap)Image.FromStream(tempImg);
                                                ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);
                                                pages.Save(msMerge, ici, ep);
                                            }
                                            else
                                            {
                                                //save the intermediate frames
                                                ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);
                                                pages.SaveAdd((Bitmap)Image.FromStream(tempImg), ep);
                                            }
                                        }
                                        frame++;
                                    }
                                }
                            }
                        }
                    }
                }
                if (frame > 0)
                {
                    //flush and close.
                    ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.Flush);
                    pages.SaveAdd(ep);
                }

                msMerge.Position = 0;
                tiffMerge = msMerge.ToArray();
            }
             
            System.IO.File.WriteAllBytes(outputFile, tiffMerge);
        }
    }

}