using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using WIA;
using System.Threading;
using System.Text;

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
        public static List<Image> Scan(string logPath)
        {
            WIA.ICommonDialog dialog = new WIA.CommonDialog();
            WIA.Device device = dialog.ShowSelectDevice(WIA.WiaDeviceType.UnspecifiedDeviceType, true, false);
            if (device != null)
            {
                writeToLog(logPath, "line 88 - device.DeviceID: " + device.DeviceID);
                return Scan(device.DeviceID, logPath);
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


        /// <summary>
        /// Use scanner to scan an image (scanner is selected by its unique id).
        /// </summary>
        /// <param name="scannerName"></param>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan(string scannerId, string logPath)
        {
            List<Image> images = new List<Image>();
            bool hasMorePages = true;
            while (hasMorePages)
            {
                // select the correct scanner using the provided scannerId parameter
                WIA.DeviceManager manager = new WIA.DeviceManager();
                WIA.Device device = null;
                foreach (WIA.DeviceInfo info in manager.DeviceInfos)
                {
                    if (info.DeviceID == scannerId)
                    {
                        writeToLog(logPath, "line 88 - scannerId: " + scannerId);
                        // connect to scanner
                        device = info.Connect();
                        break;
                    }
                }
                // device was not found
                if (device == null)
                {
                    writeToLog(logPath, "line 97: device is null");
                    // enumerate available devices
                    string availableDevices = "";
                    foreach (WIA.DeviceInfo info in manager.DeviceInfos)
                    {
                        writeToLog(logPath, "line 102 - info.DeviceID: " + info.DeviceID);
                        availableDevices += info.DeviceID + "\n";
                    }

                    // show error with available devices
                    throw new Exception("The device with provided ID could not be found. Available Devices:\n" + availableDevices);
                }

                try
                {
                    foreach (WIA.Item item in device.Items)
                    {

                        // scan image
                        //WIA.ICommonDialog wiaCommonDialog = new WIA.CommonDialog();
                        //WIA.ImageFile image = (WIA.ImageFile)wiaCommonDialog.ShowTransfer(item, wiaFormatBMP, false);

                        //SetDeviceIntProperty(ref device, 1048, 1);
                        WIA.ICommonDialog wiaCommonDialog = new WIA.CommonDialog();
                        //var tempRes = wiaCommonDialog.ShowItemProperties();
                        Thread.Sleep(2000);
                        var tempRes = wiaCommonDialog.ShowTransfer(item, wiaFormatBMP, true);
                        Thread.Sleep(2000);
                        WIA.ImageFile image = null;
                        if (tempRes != null)
                        {
                            image = (WIA.ImageFile)tempRes;
                        }


                        if (image != null)
                        {
                            // save to temp file
                            string fileName = Path.GetTempFileName();
                            writeToLog(logPath, "line 136 - fileName: " + fileName);
                            File.Delete(fileName);
                            image.SaveFile(fileName);
                            image = null;
                            // add file to output list
                            images.Add(Image.FromFile(fileName));

                            //item = null;
                            //determine if there are any more pages waiting
                            WIA.Property documentHandlingSelect = null;
                            WIA.Property documentHandlingStatus = null;
                            foreach (WIA.Property prop in device.Properties)
                            {
                                if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_SELECT)
                                    documentHandlingSelect = prop;
                                if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_STATUS)
                                    documentHandlingStatus = prop;
                            }
                            // assume there are no more pages
                            hasMorePages = false;
                            // may not exist on flatbed scanner but required for feeder
                            if (documentHandlingSelect != null)
                            {
                                // check for document feeder
                                if ((Convert.ToUInt32(documentHandlingSelect.get_Value()) & WIA_DPS_DOCUMENT_HANDLING_SELECT.FEEDER) != 0)
                                {
                                    hasMorePages = ((Convert.ToUInt32(documentHandlingStatus.get_Value()) & WIA_DPS_DOCUMENT_HANDLING_STATUS.FEED_READY) != 0);
                                }
                            }
                        }

                    }
                    Thread.Sleep(2000);
                }
                catch (Exception exc)
                {
                    if (images.Count > 0)
                    {
                        return images;
                    }
                    //throw exc;
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
