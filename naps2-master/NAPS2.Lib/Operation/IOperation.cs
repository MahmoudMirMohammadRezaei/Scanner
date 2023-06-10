﻿using System.Threading;

namespace NAPS2.Operation;

/// <summary>
/// A representation of a long-running operation with progress reporting and cancellation.
/// </summary>
public interface IOperation
{
    string? ProgressTitle { get; }

    bool AllowCancel { get; }

    bool AllowBackground { get; }

    bool SkipExitPrompt { get; }

    OperationStatus? Status { get; }

    Task<bool>? Success { get; }

    bool IsFinished { get; }

    void Wait(CancellationToken cancelToken = default);

    void Cancel();

    event EventHandler StatusChanged;

    event EventHandler Finished;

    event EventHandler<OperationErrorEventArgs> Error;
}