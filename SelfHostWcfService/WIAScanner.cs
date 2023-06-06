using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using WIA;
using System.Threading;
using System.Text;
using ADFScanner;
using System.Drawing.Imaging;
using System.Runtime.ConstrainedExecution;

namespace ADFScanner
{
    public enum ScanColor : int
    {
        Color = 1,
        Gray = 2,
        BlackWhite = 4
    }

    public class WiaImageEventArgs : EventArgs
    {
        public WiaImageEventArgs(Image img)
        {
            ScannedImage = img;
        }
        public Image ScannedImage { get; private set; }
    }

    public class ADFScan
    {
        public void BeginScan(ScanColor color, int dotsperinch)
        {
            Scan(color, dotsperinch);
        }
        public event EventHandler<WiaImageEventArgs> Scanning;
        public event EventHandler ScanComplete;

        void Scan(ScanColor clr, int dpi)
        {
            string deviceid;
            //Choose Scanner
            CommonDialogClass class1 = new CommonDialogClass();
            Device d = class1.ShowSelectDevice(WiaDeviceType.UnspecifiedDeviceType, true, false);
            if (d != null)
            {
                deviceid = d.DeviceID;
            }
            else
            {
                //no scanner chosen
                return;
            }

            EventHandler tempCom = ScanComplete;
            if (tempCom != null)
            {
                tempCom(this, EventArgs.Empty);
            }
        }
        //internal classes
        #region InternalClasses
        class WIA_ERRORS
        {
            public const uint BASE_VAL_WIA_ERROR = 0x80210000;
            public const uint WIA_ERROR_PAPER_EMPTY = BASE_VAL_WIA_ERROR + 3;
        }
        #endregion
    }
}

namespace SelfHost
{
    class WIAScanner
    {
        const string wiaFormatBMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";
        class WIA_DPS_DOCUMENT_HANDLING_SELECT
        {
            public const uint FEEDER = 0x00000001;
            public const uint FLATBED = 0x00000002;
        }
        class WIA_DPS_DOCUMENT_HANDLING_STATUS
        {
            public const uint FEED_READY = 0x00000001;
        }
        class WIA_PROPERTIES
        {
            public const uint WIA_RESERVED_FOR_NEW_PROPS = 1024;
            public const uint WIA_DIP_FIRST = 2;
            public const uint WIA_DPA_FIRST = WIA_DIP_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            public const uint WIA_DPC_FIRST = WIA_DPA_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            //
            // Scanner only device properties (DPS)
            //
            public const uint WIA_DPS_FIRST = WIA_DPC_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            public const uint WIA_DPS_DOCUMENT_HANDLING_STATUS = WIA_DPS_FIRST + 13;
            public const uint WIA_DPS_DOCUMENT_HANDLING_SELECT = WIA_DPS_FIRST + 14;
        }

        /// <summary>
        /// Use scanner to scan an image (with user selecting the scanner from a dialog).
        /// </summary>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan()
        {
            WIA.ICommonDialog dialog = new WIA.CommonDialog();
            WIA.Device device = dialog.ShowSelectDevice(WIA.WiaDeviceType.UnspecifiedDeviceType, true, false);
            if (device != null)
            {
                //writeToLog(logPath, "line 88 - device.DeviceID: " + device.DeviceID);
                return Scan(device.DeviceID, "");
            }
            else
            {
                throw new Exception("You must select a device for scanning.");
            }
        }


        public static void writeToLog(string logPath, string s)
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


        private static void SetDeviceIntProperty(ref Device device, uint propertyID, uint propertyValue)
        {
            foreach (Property p in device.Properties)
            {
                if (p.PropertyID == propertyID)
                {
                    object value = propertyValue;
                    p.set_Value(ref value);
                    break;
                }
            }
        }

        /// <summary>
        /// Use scanner to scan an image (scanner is selected by its unique id).
        /// </summary>
        /// <param name="scannerName"></param>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan(string scannerId, string logPath)
        {
            List<Image> images = new List<Image>();
            bool hasMorePages = true;

            WIA.CommonDialog WiaCommonDialog = new CommonDialogClass();
            int x = 0;
            int numPages = 0;
            while (hasMorePages)
            {
                //Create DeviceManager
                DeviceManager manager = new DeviceManagerClass();
                Device WiaDev = null;
                foreach (DeviceInfo info in manager.DeviceInfos)
                {
                    if (info.DeviceID == scannerId)
                    {
                        WIA.Properties infoprop = null;
                        infoprop = info.Properties;
                        //connect to scanner
                        WiaDev = info.Connect();
                        break;
                    }
                }
                //Start Scan
                WIA.ImageFile img = null;
                WIA.Item Item = WiaDev.Items[1] as WIA.Item;
                ////set properties //BIG SNAG!! if you call WiaDev.Items[1] apprently it erases the item from memory so you cant call it again
                //Item.Properties["6146"].set_Value((int)clr);//Item MUST be stored in a variable THEN the properties must be set.
                //Item.Properties["6147"].set_Value(dpi);
                //Item.Properties["6148"].set_Value(dpi);
                try
                {//WATCH OUT THE FORMAT HERE DOES NOT MAKE A DIFFERENCE... .net will load it as a BITMAP!
                    img = (ImageFile)WiaCommonDialog.ShowTransfer(Item, wiaFormatBMP, false);
                    //process image:
                    //Save to file and open as .net IMAGE
                    string varImageFileName = Path.GetTempFileName();
                    if (File.Exists(varImageFileName))
                    {
                        //file exists, delete it
                        File.Delete(varImageFileName);
                    }
                    img.SaveFile(varImageFileName);
                    images.Add(Image.FromFile(varImageFileName));
                    numPages++;
                    img = null;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Item = null;
                    //determine if there are any more pages waiting
                    Property documentHandlingSelect = null;
                    Property documentHandlingStatus = null;

                    foreach (Property prop in WiaDev.Properties)
                    {
                        if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_SELECT)
                            documentHandlingSelect = prop;
                        if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_STATUS)
                            documentHandlingStatus = prop;
                    }
                    hasMorePages = false; //assume there are no more pages
                    if (documentHandlingSelect != null)
                    //may not exist on flatbed scanner but required for feeder
                    {
                        //check for document feeder
                        if ((Convert.ToUInt32(documentHandlingSelect.get_Value()) & WIA_DPS_DOCUMENT_HANDLING_SELECT.FEEDER) != 0)
                        {
                            hasMorePages = ((Convert.ToUInt32(documentHandlingStatus.get_Value()) & WIA_DPS_DOCUMENT_HANDLING_STATUS.FEED_READY) != 0);
                        }
                    }
                    x++;
                }
            }

            return images;
        }

        /// <summary>
        /// Gets the list of available WIA devices.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDevices()
        {
            List<string> devices = new List<string>();
            WIA.DeviceManager manager = new WIA.DeviceManager();
            foreach (WIA.DeviceInfo info in manager.DeviceInfos)
            {
                devices.Add(info.DeviceID);
            }
            return devices;
        }

        private static void SetDeviceIntProperty(ref Device device, int propertyID, int propertyValue)
        {
            foreach (Property p in device.Properties)
            {
                if (p.PropertyID == propertyID)
                {
                    object value = propertyValue;
                    p.set_Value(ref value);
                    break;
                }
            }
        }

    }
}
