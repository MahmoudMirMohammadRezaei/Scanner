﻿using NAPS2.ImportExport;
using NAPS2.ImportExport.Images;
using NAPS2.Pdf.Pdfium;
using NAPS2.Scan;

namespace NAPS2.Pdf;

public class PdfImporter : IPdfImporter
{
    private const int MAX_PASSWORD_ATTEMPTS = 5;

    private readonly ScanningContext _scanningContext;
    private readonly IPdfPasswordProvider? _pdfPasswordProvider;
    private readonly ImportPostProcessor _importPostProcessor;

    public PdfImporter(ScanningContext scanningContext)
        : this(scanningContext, null)
    {
    }

    public PdfImporter(ScanningContext scanningContext, IPdfPasswordProvider? pdfPasswordProvider)
        : this(scanningContext, pdfPasswordProvider, new ImportPostProcessor())
    {
    }

    internal PdfImporter(ScanningContext scanningContext, IPdfPasswordProvider? pdfPasswordProvider,
        ImportPostProcessor importPostProcessor)
    {
        _scanningContext = scanningContext;
        _pdfPasswordProvider = pdfPasswordProvider;
        _importPostProcessor = importPostProcessor;
    }

    public IAsyncEnumerable<ProcessedImage> Import(string filePath, ImportParams? importParams = null,
        ProgressHandler progress = default)
    {
        importParams ??= new ImportParams();
        return AsyncProducers.RunProducer<ProcessedImage>(produceImage =>
        {
            if (progress.IsCancellationRequested) return;

            lock (PdfiumNativeLibrary.Instance)
            {
                using var document = LoadDocument(filePath, importParams);
                if (document == null) return;
                progress.Report(0, document.PageCount);

                // TODO: Maybe do a permissions check

                // TODO: Make sure to test slices (both unit and command line)
                using var pages = importParams.Slice
                    .Indices(document.PageCount)
                    .Select(index => document.GetPage(index))
                    .ToDisposableList();

                int i = 0;
                foreach (var page in pages.InnerList)
                {
                    if (progress.IsCancellationRequested) return;
                    var image = GetImageFromPage(page, importParams);
                    progress.Report(++i, document.PageCount);
                    produceImage(image);
                }
            }
        });
    }

    private PdfDocument? LoadDocument(string filePath, ImportParams importParams)
    {
        PdfDocument? doc = null;
        try
        {
            var password = importParams.Password;
            var passwordAttempts = 0;
            while (passwordAttempts < MAX_PASSWORD_ATTEMPTS)
            {
                try
                {
                    doc = PdfDocument.Load(filePath, password);
                    break;
                }
                catch (PdfiumException ex) when (ex.ErrorCode == PdfiumErrorCode.PasswordNeeded &&
                                                 _pdfPasswordProvider != null)
                {
                    if (!_pdfPasswordProvider.ProvidePassword(Path.GetFileName(filePath), passwordAttempts++,
                            out password))
                    {
                        return null;
                    }
                }
                catch (PdfiumException ex) when (ex.ErrorCode == PdfiumErrorCode.FileNotFoundOrUnavailable)
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException($"Could not find pdf file: '{filePath}'");
                    }
                    throw new IOException($"Could not open pdf file for reading: '{filePath}'");
                }
            }
            return doc;
        }
        catch (Exception)
        {
            doc?.Dispose();
            throw;
        }
    }

    private ProcessedImage GetImageFromPage(PdfPage page, ImportParams importParams)
    {
        using var storage = PdfiumImageExtractor.GetSingleImage(_scanningContext.ImageContext, page);
        if (storage != null)
        {
            var image = _scanningContext.CreateProcessedImage(storage, BitDepth.Color, false, -1);
            return _importPostProcessor.AddPostProcessingData(
                image,
                storage,
                importParams.ThumbnailSize,
                importParams.BarcodeDetectionOptions,
                true);
        }

        return ExportRawPdfPage(page, importParams);
    }

    private ProcessedImage ExportRawPdfPage(PdfPage page, ImportParams importParams)
    {
        IImageStorage storage;
        using var document = PdfDocument.CreateNew();
        document.ImportPage(page);
        if (_scanningContext.FileStorageManager != null)
        {
            string pdfPath = _scanningContext.FileStorageManager.NextFilePath() + ".pdf";
            document.Save(pdfPath);
            storage = new ImageFileStorage(pdfPath);
        }
        else
        {
            var stream = new MemoryStream();
            document.Save(stream);
            storage = new ImageMemoryStorage(stream, ".pdf");
        }

        var image = _scanningContext.CreateProcessedImage(storage);
        return _importPostProcessor.AddPostProcessingData(
            image,
            null,
            importParams.ThumbnailSize,
            importParams.BarcodeDetectionOptions,
            true);
    }
}