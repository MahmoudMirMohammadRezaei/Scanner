using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Sdk.ScannerTests;

public class ScannerTests
{
    // TODO: Make real tests for WIA/TWAIN, Flatbed/Feeder, Color/Gray/BW, DPIs, etc.

    [ScannerFact]
    public async Task ScanWithWia()
    {
        using var scanningContext = new ScanningContext(TestImageContextFactory.Get());

        var scanController = new ScanController(scanningContext);
        var devices = await scanController.GetDeviceList(Driver.Wia);
        var device = GetUserDevice(devices);

        var options = new ScanOptions
        {
            Device = device,
            Driver = Driver.Wia,
            PaperSource = PaperSource.Flatbed,
            Dpi = 100
        };

        var image = await scanController.Scan(options).FirstAsync();
        Assert.NotNull(image);
        using var rendered = image.Render();

        // TODO: Aside from generating the relevant files/resources, we also need to consider how to compare images when ImageAsserts assumes perfect pixel alignment.
        // TODO: One possibility is having a section of the test page with gradual gradients and only compare that subsection of the images. 
        // ImageAsserts.Similar(ScannerTestResources.naps2_test_page, rendered);
    }

    [ScannerFact]
    public async Task ScanWithTwain()
    {
        var imageContext = TestImageContextFactory.Get();
        using var scanningContext = new ScanningContext(imageContext);

        var scanController = new ScanController(scanningContext);
        var devices = await scanController.GetDeviceList(Driver.Twain);
        var device = GetUserDevice(devices);

        var options = new ScanOptions
        {
            Device = device,
            Driver = Driver.Twain,
            PaperSource = PaperSource.Flatbed,
            Dpi = 100
        };

        var image = await scanController.Scan(options).FirstAsync();
        Assert.NotNull(image);
    }

    [ScannerFact]
    public async Task ScanWithSane()
    {
        var imageContext = TestImageContextFactory.Get();
        using var scanningContext = new ScanningContext(imageContext);

        var scanController = new ScanController(scanningContext);
        var devices = await scanController.GetDeviceList(Driver.Sane);
        var device = GetUserDevice(devices);

        var options = new ScanOptions
        {
            Device = device,
            Driver = Driver.Sane,
            PaperSource = PaperSource.Flatbed,
            Dpi = 100
        };

        var image = await scanController.Scan(options).FirstAsync();
        Assert.NotNull(image);
    }

    // TODO: Generalize the common infrastructure into helper classes (ScannerTests as a base class, FlatbedTests, FeederTests, etc.?)
    private static ScanDevice GetUserDevice(List<ScanDevice> devices)
    {
        if (devices.Count == 0)
        {
            throw new InvalidOperationException("No scanner available");
        }
        foreach (var device in devices)
        {
            if (device.Name!.IndexOf(HowToRunScannerTests.SCANNER_NAME, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return device;
            }
        }
        throw new InvalidOperationException("Set SCANNER_NAME to one of: " +
                                            string.Join(",", devices.Select(x => x.Name)));
    }
}