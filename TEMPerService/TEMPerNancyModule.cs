using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TEMPerService
{
    public class TEMPerNancyModule : Nancy.NancyModule
    {
        private static HidDevice device;
        private static Double Calibration_Offset = 0;
        private static Double Calibration_Scale = 1;

        public TEMPerNancyModule()
        {
            byte[] temp = { 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 };
            HidDevice[] _deviceList = HidDevices.Enumerate().ToArray();
            List<HidDevice> _TemperInterfaces = _deviceList.Where(x => x.Attributes.ProductHexId == "0x7401" & x.Attributes.VendorHexId == "0x0C45").ToList();
            device = _TemperInterfaces.Find(x => x.DevicePath.Contains("mi_01"));
            var outData = device.CreateReport();
            outData.ReportId = 0x00;
            outData.Data = temp;
            device.WriteReport(outData);
            while (outData.ReadStatus == HidDeviceData.ReadStatus.NoDataRead) ;
            HidDeviceData data = device.Read();
            byte[] value = data.Data;
            int RawReading = (value[4] & 0xFF) + (value[3] << 8);
            double temperatureCelsius = (Calibration_Scale * (RawReading * (125.0 / 32000.0))) + Calibration_Offset;
            Get["/"] = _ => temperatureCelsius.ToString("0.00");
        }
    }
}
