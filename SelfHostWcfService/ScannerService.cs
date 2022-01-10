//reflector -------------------------------------------------------------------------------

using Microsoft.CSharp.RuntimeBinder;
using SelfHost;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using WIA;

namespace Arian.Core
{
    public class ScannerService : IScannerService
    {


        public void writeToLog(string logPath, string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Environment.NewLine);
            sb.Append(s);
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            string path = Path.Combine(logPath, "Logs2.log");
            File.AppendAllText(path, sb.ToString());
        }

        public string GetScan(string logPath)
        {
            try
            {
                string deviceName = null;
                try
                {
                    var dialog = new CommonDialogClass();
                    var temp = dialog.ShowSelectDevice(WIA.WiaDeviceType.UnspecifiedDeviceType, true, false);
                    if (temp != null)
                    {
                        deviceName = temp.DeviceID;
                        writeToLog(logPath, "line 47 - deviceName: " + deviceName);
                    }
                    else
                    {
                        return "koاسکنر انتخاب نشده است";
                    }
                }
                catch (Exception ex)
                {
                    if (ex.StackTrace != null)
                    {
                        writeToLog(logPath, "line 58 - ex.StackTrace: " + ex.StackTrace.ToString());
                    }
                    writeToLog(logPath, "line 60 - ex.Message: " + ex.Message);
                    throw ex;
                }

                //get list of devices available
                //List<string> devices = WIAScanner.GetDevices();
                //string deviceName = null;
                //foreach (string device in devices)
                //{
                //    deviceName = device;
                //    break;
                //}

                //List<string> deviceNameList = new List<string>() ;
                //foreach (string device in devices)
                //{
                //     deviceNameList.Add(device);
                //}



                //check if device is not available
                if (deviceName == null)
                {
                    //return "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";
                    return "koدستگاهی برای اسکن پیدا نشد!";

                }

                List<System.Drawing.Image> images = WIAScanner.Scan(deviceName, logPath);
                writeToLog(logPath, "line 90 - images = WIAScanner.Scan(deviceName, logPath)");
                var outputFile = Path.Combine(Path.GetTempPath(), @"convertedimage" + DateTime.Now.Ticks + ".tiff");
                writeToLog(logPath, "line 92 - outputFile: " + outputFile);
                var imagebase64 = ImageHelper.MergeTiff(images, outputFile);
                writeToLog(logPath, "line 94 - imagebase64: " + imagebase64);

                //test: save file
                //var imgBase64 = ImageToBase64(Image.FromFile(outputFile), ImageFormat.Tiff);
                //    var t1 = Path.Combine(Path.GetTempPath(), @"convertedimage_2_" + DateTime.Now.Ticks + ".tiff");
                //    Image image;
                //    byte[] bytes = Convert.FromBase64String(imagebase64);
                //    using (MemoryStream ms = new MemoryStream(bytes))
                //    {
                //        image = Image.FromStream(ms);
                //    }
                //    File.WriteAllBytes(t1, bytes);

                return "ok" + imagebase64;
            }
            catch (Exception exc)
            {
                if (exc.StackTrace != null)
                {
                    writeToLog(logPath, "line 113 - exc.StackTrace: " + exc.StackTrace.ToString());
                }
                writeToLog(logPath, "line 115 - exc.Message: " + exc.Message);
                return "ko" + exc.Message;
            }
        }

        //public string GetScan()
        //{
        //	string error = "no error";

        //	//_ = (CommonDialog)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("850D1D11-70F3-4BE5-9A11-77AA6B2BB201")));
        //	try
        //	{
        //		//ImageFile imageFile = checkScannerList();
        //		//string filename = Path.Combine(Path.GetTempPath(), "scan" + DateTime.Now.Ticks + ".tiff");

        //		var outputFile = Path.Combine(Path.GetTempPath(), @"convertedimage" + DateTime.Now.Ticks + ".tiff");

        //		if (imageFile != null)
        //		{
        //			List<System.Drawing.Image> images = WIAScanner.Scan((string)lbDevices.SelectedItem);
        //			TiffHelper.MergeTiff(images);

        //			imageFile.SaveFile(filename);
        //			return ImageToBase64(Image.FromFile(filename), ImageFormat.Tiff);
        //		}
        //		return "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";
        //	}
        //	catch (Exception ex)
        //	{
        //		error = ex.Message + ex.InnerException == null ? "" : ex.InnerException.Message;

        //		switch (ex.Message)
        //		{
        //			case "Exception from HRESULT: 0x80210015":
        //				Console.WriteLine("no scanner found");
        //				break;
        //			case "Exception from HRESULT: 0x80210016":
        //				Console.WriteLine("scanner is open plz close it to start");
        //				break;
        //			case "Exception from HRESULT: 0x80210006":
        //				Console.WriteLine("scanner is open plz close it to start");
        //				break;
        //			default:
        //				Console.WriteLine("no match error found check log");
        //				Console.WriteLine(ex.Message);
        //				break;
        //		}
        //		return error; // ex.Message;
        //	}
        //}

        public static ImageFile checkScannerList()
        {
            IDeviceInfo x = ((DeviceManager)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("E1C5D730-7E97-4D8A-9E42-BBAE87C2059F")))).DeviceInfos.OfType<IDeviceInfo>().FirstOrDefault((IDeviceInfo g) => g.Type == WiaDeviceType.ScannerDeviceType);
            if (x != null)
            {
                Item scannerItem = x.Connect().Items[1];
                return (ImageFile)(dynamic)scannerItem.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
            }
            return null;
        }

        public static string ImageToBase64(Image image, ImageFormat format)
        {
            if (image == null)
            {
                return string.Empty;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}

//reflector  -------------------------------------------------------------------------------
// Decompiled with JetBrains decompiler
// Type: Arian.Core.ScannerService
// Assembly: Arian.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 106191F3-F4B3-4D8E-93B0-D23CE29719F1
// Assembly location: D:\MahmooDev\Scanner\arianscan-new\arianTH\arianTH\WebSite\Arian.Core.dll

//using Microsoft.CSharp.RuntimeBinder;
//using SelfHost;
//using System;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using WIA;

//namespace Arian.Core
//{
//    public class ScannerService : IScannerService
//    {
//        public string GetScan()
//        {
//            // ISSUE: variable of a compiler-generated type
//            //CommonDialog instance = (CommonDialog)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("850D1D11-70F3-4BE5-9A11-77AA6B2BB201")));


//            try
//            {
//                // ISSUE: variable of a compiler-generated type
//                ImageFile mageFile = ScannerService.checkScannerList();
//                string str = Path.Combine(Path.GetTempPath(), "scan" + DateTime.Now.Ticks.ToString() + ".jpg");
//                if (mageFile == null)
//                    return "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";
//                // ISSUE: reference to a compiler-generated method
//                mageFile.SaveFile(str);
//                return ScannerService.ImageToBase64(Image.FromFile(str), ImageFormat.Jpeg);
//            }
//            catch (Exception ex)
//            {
//                switch (ex.Message)
//                {
//                    case "Exception from HRESULT: 0x80210015":
//                        Console.WriteLine("no scanner found");
//                        break;
//                    case "Exception from HRESULT: 0x80210016":
//                        Console.WriteLine("scanner is open plz close it to start");
//                        break;
//                    case "Exception from HRESULT: 0x80210006":
//                        Console.WriteLine("scanner is open plz close it to start");
//                        break;
//                    default:
//                        Console.WriteLine("no match error found check log");
//                        Console.WriteLine(ex.Message);
//                        break;
//                }
//                return ex.Message;
//            }
//        }

//        public static ImageFile checkScannerList()
//        {
//            // ISSUE: variable of a compiler-generated type
//            IDeviceInfo deviceInfo = ((IDeviceManager)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("E1C5D730-7E97-4D8A-9E42-BBAE87C2059F")))).DeviceInfos.OfType<IDeviceInfo>().FirstOrDefault<IDeviceInfo>((Func<IDeviceInfo, bool>)(g => g.Type == WiaDeviceType.ScannerDeviceType));

//            if (deviceInfo == null)
//                return (ImageFile)null;
//            // ISSUE: reference to a compiler-generated method
//            // ISSUE: variable of a compiler-generated type
//            Item tem = deviceInfo.Connect().Items[1];
//            // ISSUE: reference to a compiler-generated field
//            if (ScannerService.\u003C\u003Eo__1.\u003C\u003Ep__0 == null)
//      {
//                // ISSUE: reference to a compiler-generated field
//                ScannerService.\u003C\u003Eo__1.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, ImageFile>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(ImageFile), typeof(ScannerService)));
//            }
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated field
//            // ISSUE: reference to a compiler-generated method
//            return ScannerService.\u003C\u003Eo__1.\u003C\u003Ep__0.Target((CallSite)ScannerService.\u003C\u003Eo__1.\u003C\u003Ep__0, tem.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}"));
//        }

//        public static string ImageToBase64(Image image, ImageFormat format)
//        {
//            if (image == null)
//                return string.Empty;
//            using (MemoryStream memoryStream = new MemoryStream())
//            {
//                image.Save((Stream)memoryStream, format);
//                return Convert.ToBase64String(memoryStream.ToArray());
//            }
//        }
//    }
//}





//using System;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using System.ServiceModel.Activation;
//using WIA;

//namespace SelfHost
//{
//    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
//    public class ScannerService : IScannerService
//    {
//        public string GetScan()
//        {
//            string imgResult;
//            var dialog = new CommonDialogClass();
//            try
//            {
//                // نمایش فرم پیشفرض اسکنر
//                var image = dialog.ShowAcquireImage(WiaDeviceType.ScannerDeviceType);

//                // ذخیره تصویر در یک فایل موقت
//                //var filename = Path.GetTempFileName();
//                //var filename = Path.GetTempPath();
//                var filename = Path.Combine(Path.GetTempPath(), "scan" + DateTime.Now.Ticks + ".jpg");
//                image.SaveFile(filename);

//                var img = Image.FromFile(filename);

//                // img جهت ارسال سمت کاربر و نمایش در تگ Base64 تبدیل تصویر به 
//                imgResult = ImageHelper.ImageToBase64(img, ImageFormat.Jpeg);
//            }
//            catch (Exception ex)
//            {
//                //switch (ex)
//                //{
//                //    case "Exception from HRESULT: 0x80210015":
//                //}

//                switch (ex.Message)
//                {
//                    case "Exception from HRESULT: 0x80210015":
//                        Console.WriteLine("no scanner found");
//                        break;
//                    case "Exception from HRESULT: 0x80210016":
//                        Console.WriteLine("scanner is open plz close it to start");
//                        break;                    
//                    case "Exception from HRESULT: 0x80210006":
//                        Console.WriteLine("scanner is open plz close it to start");
//                        break;
//                    default:
//                        Console.WriteLine("no match error found check log");
//                        Console.WriteLine(ex.Message);
//                        break;
//                }
//                // از آنجاییه که امکان نمایش خطا وجود ندارد در صورت بروز خطا رشته خالی 
//                // بازگردانده می‌شود که به معنای نبود تصویر می‌باشد

//                // a base-64 image to test
//                return @"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";
//            }
//            return imgResult;
//        }
//    }
//}