using System.Collections.Immutable;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class UndoStackTests : ContextualTests
{
    [Fact]
    public void MementoComparison()
    {
        var emptyList = ImmutableList<ProcessedImage>.Empty;
        
        var emptyMemento = Memento.Empty;
        Assert.Equal(new Memento(emptyList), emptyMemento);

        var image = CreateScannedImage();
        var snapshot1 = new Memento(emptyList.Add(image));
        Assert.NotEqual(emptyMemento, snapshot1);
        Assert.Equal(new Memento(emptyList.Add(image)), snapshot1);

        var image2 = image.WithTransform(new BrightnessTransform(100));
        var snapshot2 = new Memento(emptyList.Add(image2));
        Assert.NotEqual(snapshot1, snapshot2);
        Assert.Equal(new Memento(emptyList.Add(image2)), snapshot2);
    }

    [Fact]
    public void Initial_IsEmpty()
    {
        var stack = new UndoStack(10);
        Assert.Equal(Memento.Empty, stack.Current);
    }

    [Fact]
    public void Initial_NoUndoOrRedo()
    {
        var stack = new UndoStack(10);
        Assert.False(stack.Undo());
        Assert.False(stack.Redo());
    }

    [Fact]
    public void PushUndoRedoUndo()
    {
        var emptyList = ImmutableList<ProcessedImage>.Empty;

        var stack = new UndoStack(10);
        var image = CreateScannedImage();
        Assert.True(stack.Push(new[] { image }));
        Assert.Equal(new Memento(emptyList.Add(image)), stack.Current);
        Assert.True(stack.Undo());
        Assert.Equal(Memento.Empty, stack.Current);
        Assert.True(stack.Redo());
        Assert.Equal(new Memento(emptyList.Add(image)), stack.Current);
        Assert.True(stack.Undo());
        Assert.Equal(Memento.Empty, stack.Current);
    }

    [Fact]
    public void PushSame()
    {
        var stack = new UndoStack(10);
        Assert.False(stack.Push(Memento.Empty));
        Assert.False(stack.Undo());
        var imageArray = new[] { CreateScannedImage() };
        Assert.True(stack.Push(imageArray));
        Assert.False(stack.Push(imageArray));
        Assert.True(stack.Undo());
        Assert.Equal(Memento.Empty, stack.Current);
    }

    [Fact]
    public void StackLimit()
    {
        var stack = new UndoStack(4);
        var images = Enumerable.Repeat(0, 10).Select(i => CreateScannedImage()).ToList();
        for (int i = 1; i <= 10; i++)
        {
            stack.Push(images.Take(i));
        }
        Assert.Equal(new Memento(images.Take(10).ToImmutableList()), stack.Current);
        for (int i = 9; i >= 7; i--)
        {
            Assert.True(stack.Undo());
            Assert.Equal(new Memento(images.Take(i).ToImmutableList()), stack.Current);
        }
        Assert.False(stack.Undo());
        for (int i = 8; i <= 10; i++)
        {
            Assert.True(stack.Redo());
            Assert.Equal(new Memento(images.Take(i).ToImmutableList()), stack.Current);
        }
        Assert.False(stack.Redo());
    }

    [Fact]
    public void ClearUndo()
    {
        var stack = new UndoStack(10);
        var images = Enumerable.Repeat(0, 5).Select(i => CreateScannedImage()).ToList();
        for (int i = 1; i <= 5; i++)
        {
            stack.Push(images.Take(i));
        }
        stack.ClearUndo();
        Assert.False(stack.Undo());
        Assert.Equal(new Memento(images.Take(5).ToImmutableList()), stack.Current);
    }

    [Fact]
    public void ClearRedo()
    {
        var stack = new UndoStack(10);
        var images = Enumerable.Repeat(0, 5).Select(i => CreateScannedImage()).ToList();
        for (int i = 1; i <= 5; i++)
        {
            stack.Push(images.Take(i));
        }
        for (int i = 1; i <= 5; i++)
        {
            stack.Undo();
        }
        stack.ClearRedo();
        Assert.False(stack.Redo());
        Assert.Equal(Memento.Empty, stack.Current);
    }

    [Fact]
    public void ClearBoth()
    {
        var stack = new UndoStack(10);
        var images = Enumerable.Repeat(0, 5).Select(i => CreateScannedImage()).ToList();
        for (int i = 1; i <= 5; i++)
        {
            stack.Push(images.Take(i));
        }
        for (int i = 1; i <= 2; i++)
        {
            stack.Undo();
        }
        stack.ClearBoth();
        Assert.False(stack.Undo());
        Assert.False(stack.Redo());
        Assert.Equal(new Memento(images.Take(3).ToImmutableList()), stack.Current);
    }
}