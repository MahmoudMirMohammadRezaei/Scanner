﻿using Autofac;
using Microsoft.Extensions.Logging;
using NAPS2.EtoForms;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Email.Mapi;
using NAPS2.ImportExport.Images;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Platform.Windows;
using NAPS2.Recovery;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;
using NAPS2.Scan.Internal;
using NAPS2.Unmanaged;

namespace NAPS2.Modules;

public class CommonModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Import
        builder.RegisterType<ScannedImageImporter>().As<IScannedImageImporter>();
        builder.RegisterType<PdfImporter>().As<IPdfImporter>();
        builder.RegisterType<ImageImporter>().As<IImageImporter>();
        builder.RegisterType<RecoveryManager>().AsSelf();

        // Export
        builder.RegisterType<PdfExporter>().As<IPdfExporter>();
        builder.RegisterType<AutofacEmailProviderFactory>().As<IEmailProviderFactory>();
        builder.RegisterType<StubMapiWrapper>().As<IMapiWrapper>();
        builder.RegisterType<OcrRequestQueue>().AsSelf().SingleInstance();

        // Scan
        builder.RegisterType<ScanPerformer>().As<IScanPerformer>();
        builder.RegisterType<LocalPostProcessor>().As<ILocalPostProcessor>();
        builder.RegisterType<RemotePostProcessor>().As<IRemotePostProcessor>();
        builder.RegisterType<ScanBridgeFactory>().As<IScanBridgeFactory>();
        builder.RegisterType<ScanDriverFactory>().As<IScanDriverFactory>();
        builder.RegisterType<RemoteScanController>().As<IRemoteScanController>();
        builder.RegisterType<InProcScanBridge>().AsSelf();
        builder.RegisterType<WorkerScanBridge>().AsSelf();

        // Config
        // TODO: Make this a usable path on Mac/Linux
        builder.Register(_ => new Naps2Config(Path.Combine(Paths.Executable, "appsettings.xml"),
            Path.Combine(Paths.AppData, "config.xml"))).SingleInstance();

        // Host
        builder.Register<IWorkerFactory>(_ => WorkerFactory.CreateDefault()).SingleInstance();

        // Logging
        builder.Register<ILogger>(ctx =>
        {
            var config = ctx.Resolve<Naps2Config>();
            return NLogConfig.CreateLogger(() => config.Get(c => c.EnableDebugLogging));
        }).SingleInstance();

        // Misc
        builder.RegisterType<AutofacFormFactory>().As<IFormFactory>();
        builder.RegisterType<AutofacOperationFactory>().As<IOperationFactory>();
        builder.RegisterInstance(new UiImageList());
        builder.RegisterType<StillImage>().AsSelf().SingleInstance();
        builder.RegisterType<AutoSaver>().AsSelf();
        // TODO: Use PdfiumWorkerCoordinator?
        builder.RegisterType<PdfiumPdfRenderer>().As<IPdfRenderer>();
        builder.RegisterType<OcrOperationManager>().AsSelf().SingleInstance();
        builder.RegisterType<ThumbnailController>().AsSelf().SingleInstance();
        builder.RegisterType<ThumbnailRenderQueue>().AsSelf().SingleInstance();
        builder.RegisterType<DefaultIconProvider>().As<IIconProvider>();

        // ScanningContext has several properties that need to be populated. We do some here, and also some in
        // GuiModule/ConsoleModule/WorkerModule as they each have their own needs.
        builder.RegisterType<ScanningContext>().AsSelf().SingleInstance();
        builder.RegisterBuildCallback(ctx =>
        {
            var scanningContext = ctx.Resolve<ScanningContext>();
            scanningContext.WorkerFactory = ctx.Resolve<IWorkerFactory>();
            scanningContext.Logger = ctx.Resolve<ILogger>();
            scanningContext.TempFolderPath = Paths.Temp;
            scanningContext.RecoveryPath = Paths.Recovery;
        });

        //container.Resolve<ImageContext>().PdfRenderer = container.Resolve<PdfiumWorkerCoordinator>();

        builder.Register<IProfileManager>(ctx =>
        {
            var config = ctx.Resolve<Naps2Config>();
            return new ProfileManager(
                Path.Combine(Paths.AppData, "profiles.xml"),
                // TODO: Make this a usable path on Mac/Linux
                Path.Combine(AssemblyHelper.EntryFolder, "profiles.xml"),
                config.Get(c => c.LockSystemProfiles),
                config.Get(c => c.LockUnspecifiedDevices),
                config.Get(c => c.NoUserProfiles));
        }).SingleInstance();

        builder.Register(ctx =>
        {
            var config = ctx.Resolve<Naps2Config>();
            var customComponentsPath = config.Get(c => c.ComponentsPath);
            var componentsPath = string.IsNullOrWhiteSpace(customComponentsPath)
                ? Paths.Components
                : Environment.ExpandEnvironmentVariables(customComponentsPath);
            return new TesseractLanguageManager(componentsPath);
        }).SingleInstance();
        builder.Register<IOcrEngine>(ctx =>
        {
            var tesseractPath = PlatformCompat.System.UseSystemTesseract
                ? "tesseract"
                : NativeLibrary.FindExePath(PlatformCompat.System.TesseractExecutableName);
            var engine = new TesseractOcrEngine(
                tesseractPath,
                ctx.Resolve<TesseractLanguageManager>().TessdataBasePath,
                Paths.Temp);
            var errorOutput = ctx.Resolve<ErrorOutput>();
            engine.OcrError += (_, args) => errorOutput.DisplayError(SdkResources.OcrError, args.Exception);
            engine.OcrTimeout += (_, _) => errorOutput.DisplayError(SdkResources.OcrTimeout);
            return engine;
        }).SingleInstance();
    }
}