using BackgroundRemover.Models;
using Microsoft.ML.OnnxRuntime;
using PaintDotNet.AppModel;

namespace BackgroundRemover.Services;

internal class ONNXModelService(IDxgiAdapterService2 adapterService) : IDisposable
{
    private SessionCacheEntry? _lastSession;

    public ONNXModel[] Models { get; private set; } = [];

    public bool LastSessionUsedCpuFallback { get; private set; }

    public void LoadModels(string directory)
        => Models = [.. Directory.EnumerateFiles(directory, "*.onnx").Select(f => new ONNXModel(Path.GetFileNameWithoutExtension(f), new(1024), f))];

    public InferenceSession GetOrCreateSession(ONNXModel model, bool useDirectML)
    {
        if (_lastSession != null && _lastSession.Model == model && _lastSession.UseDirectML == useDirectML)
            return _lastSession.Session;

        _lastSession?.Dispose();
        _lastSession = null;

        var session = CreateSession(model, useDirectML);
        _lastSession = new SessionCacheEntry(model, useDirectML, session);

        return session;
    }

    private InferenceSession CreateSession(ONNXModel model, bool useDirectML)
    {
        LastSessionUsedCpuFallback = false;

        if (useDirectML)
        {
            try
            {
                using var dmlOptions = CreateSessionOptions(GraphOptimizationLevel.ORT_ENABLE_ALL);
                using var device = adapterService.GetRenderingAdapter();
                dmlOptions.AppendExecutionProvider_DML(device.GetEnumAdapterIndex());

                return new InferenceSession(model.FilePath, dmlOptions);
            }
            catch
            {
                LastSessionUsedCpuFallback = true;
            }
        }

        using var cpuOptions = CreateSessionOptions(GraphOptimizationLevel.ORT_ENABLE_BASIC);
        return new InferenceSession(model.FilePath, cpuOptions);
    }

    private static SessionOptions CreateSessionOptions(GraphOptimizationLevel optimizationLevel) => new()
    {
        LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_FATAL,
        GraphOptimizationLevel = optimizationLevel
    };

    public InferenceSession FallBackToCpu(ONNXModel model)
    {
        _lastSession?.Dispose();
        _lastSession = null;

        using var cpuOptions = CreateSessionOptions(GraphOptimizationLevel.ORT_ENABLE_BASIC);
        var session = new InferenceSession(model.FilePath, cpuOptions);

        LastSessionUsedCpuFallback = true;
        _lastSession = new SessionCacheEntry(model, UseDirectML: true, session);

        return session;
    }

    public void Dispose() => _lastSession?.Dispose();
}
