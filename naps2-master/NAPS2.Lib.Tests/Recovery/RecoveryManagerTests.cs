using System.Threading;
using Moq;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Lib.Tests.Recovery;

public class RecoveryManagerTests : ContextualTests
{
    private readonly string _recoveryBasePath;
    private readonly RecoveryManager _recoveryManager;

    public RecoveryManagerTests()
    {
        _recoveryBasePath = Path.Combine(FolderPath, "recovery");
        ScanningContext.FileStorageManager = FileStorageManager.CreateFolder(_recoveryBasePath);
        ScanningContext.RecoveryPath = _recoveryBasePath;
        _recoveryManager = new RecoveryManager(ScanningContext);
    }

    [Fact]
    public void NoFoldersAvailable()
    {
        var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder);
    }

    [Fact]
    public void FolderWithNoImages()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 0);

        var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder);
    }

    [Fact]
    public void FolderLocking()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 1);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);

        using var folder2 = _recoveryManager.GetLatestRecoverableFolder();
        Assert.Null(folder2);

        folder.Dispose();
        using var folder3 = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder3);
    }

    [Fact]
    public void FindSingleFolder()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 1);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.Equal(1, folder.ImageCount);
        DateAsserts.Recent(TimeSpan.FromMilliseconds(100), folder.ScannedDateTime);
    }

    [Fact]
    public void DeleteFolder()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 1);

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        Assert.True(Directory.Exists(recovery1));
        folder.TryDelete();
        Assert.False(Directory.Exists(recovery1));
    }

    [Fact]
    public void Recover()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 2);

        var images = new List<ProcessedImage>();
        void ImageCallback(ProcessedImage img) => images.Add(img);
        var mockProgressCallback = new Mock<ProgressCallback>();

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var result = folder.TryRecover(ImageCallback, new RecoveryParams(), mockProgressCallback.Object);
        Assert.True(result);

        Assert.Equal(2, images.Count);
        ImageAsserts.Similar(ImageResources.dog, images[0]);

        mockProgressCallback.Verify(callback => callback(0, 2));
        mockProgressCallback.Verify(callback => callback(1, 2));
        mockProgressCallback.Verify(callback => callback(2, 2));
        mockProgressCallback.VerifyNoOtherCalls();
    }

    [Fact]
    public void CancelRecover()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        CreateFolderToRecoverFrom(recovery1, 2);

        var mockImageCallback = new Mock<Action<ProcessedImage>>();
        CancellationTokenSource cts = new CancellationTokenSource();

        void ProgressCallback(int current, int total)
        {
            // Cancel after the first image is recovered
            if (current == 1) cts.Cancel();
        }

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);

        var result = folder.TryRecover(mockImageCallback.Object, new RecoveryParams(),
            new ProgressHandler(ProgressCallback, cts.Token));
        Assert.False(result);
        Assert.True(Directory.Exists(recovery1));
        mockImageCallback.Verify(callback => callback(It.IsAny<ProcessedImage>()));
        mockImageCallback.VerifyNoOtherCalls();

        // After a cancelled recovery, we should be able to recover from the same folder again
        folder.Dispose();
        using var folder2 = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder2);
    }

    [Fact]
    public void RecoverWithMissingFile()
    {
        string recovery1 = Path.Combine(_recoveryBasePath, Path.GetRandomFileName());
        var uiImages = CreateFolderToRecoverFrom(recovery1, 2);
        File.Delete(((ImageFileStorage) uiImages[0].GetImageWeakReference().ProcessedImage.Storage).FullPath);

        var images = new List<ProcessedImage>();
        void ImageCallback(ProcessedImage img) => images.Add(img);
        var mockProgressCallback = new Mock<ProgressCallback>();

        using var folder = _recoveryManager.GetLatestRecoverableFolder();
        Assert.NotNull(folder);
        var result = folder.TryRecover(ImageCallback, new RecoveryParams(), mockProgressCallback.Object);
        Assert.True(result);

        Assert.Single(images);
        ImageAsserts.Similar(ImageResources.dog, images[0]);

        mockProgressCallback.Verify(callback => callback(0, 2));
        mockProgressCallback.Verify(callback => callback(1, 2));
        mockProgressCallback.Verify(callback => callback(2, 2));
        mockProgressCallback.VerifyNoOtherCalls();
    }

    private List<UiImage> CreateFolderToRecoverFrom(string folderPath, int imageCount)
    {
        var imageList = new UiImageList();
        var rsm1 = RecoveryStorageManager.CreateFolderWithoutThrottle(folderPath, imageList);
        var recoveryContext = new ScanningContext(TestImageContextFactory.Get())
        {
            FileStorageManager = new FileStorageManager(folderPath)
        };
        var images = Enumerable.Range(0, imageCount).Select(x => new UiImage(CreateRecoveryImage(recoveryContext)))
            .ToList();
        imageList.Mutate(new ListMutation<UiImage>.Append(images));
        rsm1.ReleaseLockForTesting();
        return images;
    }

    // TODO: Add tests for recovery params (i.e. thumbnail)

    private ProcessedImage CreateRecoveryImage(ScanningContext recoveryContext)
    {
        return recoveryContext.CreateProcessedImage(ImageContext.Load(ImageResources.dog));
    }
}