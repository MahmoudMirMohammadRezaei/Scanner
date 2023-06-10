﻿using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.ImportExport.Images;

public class ImageImporter : IImageImporter
{
    private readonly ScanningContext _scanningContext;
    private readonly ImageContext _imageContext;
    private readonly ImportPostProcessor _importPostProcessor;

    public ImageImporter(ScanningContext scanningContext, ImageContext imageContext,
        ImportPostProcessor importPostProcessor)
    {
        _scanningContext = scanningContext;
        _imageContext = imageContext;
        _importPostProcessor = importPostProcessor;
    }

    public IAsyncEnumerable<ProcessedImage> Import(string filePath, ImportParams importParams,
        ProgressHandler progress = default)
    {
        return AsyncProducers.RunProducer<ProcessedImage>(async produceImage =>
        {
            if (progress.IsCancellationRequested) return;

            int frameCount = 0;
            try
            {
                var toImport =
                    _imageContext.LoadFrames(filePath, new ProgressCallback((current, max) =>
                    {
                        frameCount = max;
                        if (current == 0)
                        {
                            progress.Report(0, frameCount);
                        }
                    }));

                int i = 0;
                await foreach (var frame in toImport)
                {
                    using (frame)
                    {
                        if (progress.IsCancellationRequested) return;

                        bool lossless = frame.OriginalFileFormat is ImageFileFormat.Bmp or ImageFileFormat.Png;
                        var image = _scanningContext.CreateProcessedImage(
                            frame,
                            BitDepth.Color,
                            lossless,
                            -1);
                        image = _importPostProcessor.AddPostProcessingData(
                            image,
                            frame,
                            importParams.ThumbnailSize,
                            importParams.BarcodeDetectionOptions,
                            true);

                        progress.Report(++i, frameCount);
                        produceImage(image);
                    }
                }
            }
            catch (Exception e)
            {
                _scanningContext.Logger.LogError(e, "Error importing image: {FilePath}", filePath);
                // Handle and notify the user outside the method so that errors importing multiple files can be aggregated
                throw;
            }
        });
    }
}