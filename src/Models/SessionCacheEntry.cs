using Microsoft.ML.OnnxRuntime;

namespace BackgroundRemover.Models;

internal sealed class SessionCacheEntry(ONNXModel Model, bool UseDirectML, InferenceSession Session) : IDisposable
{
    public ONNXModel Model { get; } = Model;
    public bool UseDirectML { get; } = UseDirectML;
    public InferenceSession Session { get; } = Session;

    public void Dispose() => Session.Dispose();
}
