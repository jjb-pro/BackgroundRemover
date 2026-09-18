using System.Numerics;

namespace BackgroundRemover.Models;

internal record ONNXModel(string Name, Vector2 InputSize, string FilePath)
{
    public override string ToString() => Name;
}
