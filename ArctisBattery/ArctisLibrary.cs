using HidLibrary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Arctis;

public class ArctisLibrary
{
    public int CheckBattery()
    {
        ushort vendorId = 0x1038;
        List<ushort> productIds = new List<ushort>()
            {
                0x12AD,
                0x1260,
                0x1252,
                0x12B3,
                0x12C2
            };

        List<HidDevice> devices = new List<HidDevice>();

        productIds.ForEach(productId =>
        {
            devices.AddRange(HidDevices.Enumerate(vendorId, productId));
        });

        if (devices.Any())
        {
            using var device = devices.First(x => x.Capabilities.OutputReportByteLength > 0);
            var outData = new byte[device.Capabilities.OutputReportByteLength - 1];


            outData[0] = 0x06;
            outData[1] = 0x18;

            device.Write(outData);

            // Blocking read of report
            var text = device.Read().Data[2];
            _ = int.TryParse(text.ToString(), out var percentageValue);
            Debug.WriteLine($"Percentage: {percentageValue}");
            return percentageValue > 100 ? 100 : percentageValue;
        }
        return -1;
    }
}
