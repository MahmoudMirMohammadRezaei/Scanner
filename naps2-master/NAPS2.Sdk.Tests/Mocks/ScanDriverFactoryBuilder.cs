using System.Threading;
using Moq;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.Sdk.Tests.Mocks;

public class ScanDriverFactoryBuilder
{
    private readonly StubScanDriver _scanDriver;
    private readonly Mock<IScanDriverFactory> _scanDriverFactory;

    public ScanDriverFactoryBuilder()
    {
        _scanDriver = new StubScanDriver();
        _scanDriverFactory = new Mock<IScanDriverFactory>();
        _scanDriverFactory.Setup(x => x.Create(It.IsAny<ScanOptions>())).Returns(_scanDriver);
    }

    public ScanDriverFactoryBuilder WithDeviceList(params ScanDevice[] devices)
    {
        _scanDriver.DeviceList = devices.ToList();
        return this;
    }

    public ScanDriverFactoryBuilder WithScannedImages(params byte[][] images)
    {
        _scanDriver.AddScanResult(images.Select(image => TestImageContextFactory.Get().Load(image)).ToList());
        return this;
    }

    internal IScanDriverFactory Build()
    {
        return _scanDriverFactory.Object;
    }

    private class StubScanDriver : IScanDriver
    {
        private readonly Queue<List<IMemoryImage>> _scans = new();
            
        public List<ScanDevice> DeviceList { get; set; }

        public void AddScanResult(List<IMemoryImage> images)
        {
            _scans.Enqueue(images);
        }
            
        public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
        {
            foreach (var device in DeviceList)
            {
                callback(device);
            }
            return Task.CompletedTask;
        }

        public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
        {
            foreach (var image in _scans.Dequeue())
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return Task.CompletedTask;
                }
                scanEvents.PageStart();
                callback(image);
            }
            return Task.CompletedTask;
        }
    }
}