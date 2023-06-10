﻿using System.Reflection;

// ReSharper disable once CheckNamespace
namespace NAPS2.Util;

public static class ExceptionExtensions
{
    private static MethodInfo? _internalPreserveStackTrace;

    static ExceptionExtensions()
    {
        _internalPreserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    /// <summary>
    /// Maintains the stack trace of an exception even after it is rethrown.
    /// This can be helpful when marshalling exceptions across process boundaries.
    /// </summary>
    /// <param name="e"></param>
    public static void PreserveStackTrace(this Exception e)
    {
        if (_internalPreserveStackTrace != null)
        {
            _internalPreserveStackTrace.Invoke(e, new object[] { });
        }
    }
}