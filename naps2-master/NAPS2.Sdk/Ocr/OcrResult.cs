﻿using System.Collections.Immutable;

namespace NAPS2.Ocr;

/// <summary>
/// The result of an OCR request. Contains a set of elements that represent text segments. 
/// </summary>
public class OcrResult
{
    public OcrResult((int x, int y, int w, int h) pageBounds, ImmutableList<OcrResultElement> elements)
    {
        PageBounds = pageBounds;
        Elements = elements;
    }

    public (int x, int y, int w, int h) PageBounds { get; }

    public ImmutableList<OcrResultElement> Elements { get; }
}