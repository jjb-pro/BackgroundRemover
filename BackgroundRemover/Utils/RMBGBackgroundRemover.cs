using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.Imaging;
using PaintDotNet.Rendering;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BackgroundRemover.Utils;

public class RMBGBackgroundRemover : IDisposable
{
    private const int ModelSize = 1024;

    private readonly InferenceSession _inferenceSession;

    public RMBGBackgroundRemover(string modelPath, bool useFP16, bool useGPU)
    {
        var sessionOptions = new SessionOptions
        {
            LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_FATAL
        };

        if (useFP16)
            sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_EXTENDED;

        if (useGPU)
            sessionOptions.AppendExecutionProvider_DML();

        _inferenceSession = new InferenceSession(modelPath, sessionOptions);
    }

    public void RemoveBackground(IBitmapSource<ColorBgra32> source, IBitmapEffectOutput output, CancellationToken cts = default)
    {
        var fullRect = new RectInt32(0, 0, ModelSize, ModelSize);

        // create a scaled view of the source bitmap at 1024x1024 using Linear interpolation
        using var scaledSource = source.CreateScaler(new SizeInt32(ModelSize, ModelSize), BitmapInterpolationMode.Linear);
        using var resizedSource = scaledSource.ToBitmap();
        using var resizedLock = resizedSource.Lock(fullRect, BitmapLockOptions.Read);

        // preprocess image for model
        var image = LoadAndPreprocessImage(resizedLock, ModelSize, ModelSize);

        cts.ThrowIfCancellationRequested();

        // prepare input tensor
        var inputTensor = new DenseTensor<float>([1, 3, ModelSize, ModelSize]);
        for (int c = 0; c < 3; c++)
            for (int y = 0; y < ModelSize; y++)
                for (int x = 0; x < ModelSize; x++)
                    inputTensor[0, c, y, x] = image[c, y, x];

        cts.ThrowIfCancellationRequested();

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("pixel_values", inputTensor)
        };

        // run inference
        using var results = _inferenceSession.Run(inputs);

        var resultTensor = results[0].AsTensor<float>();
        var mask = new float[ModelSize, ModelSize];
        for (int y = 0; y < ModelSize; y++)
            for (int x = 0; x < ModelSize; x++)
                mask[y, x] = resultTensor[0, 0, y, x];

        cts.ThrowIfCancellationRequested();

        using var outputLock = output.Lock<ColorBgra32>();
        var outputRegion = outputLock.AsRegionPtr();
        using var sourceLock = source.ToBitmap().Lock(BitmapLockOptions.Read);
        var sourceRegion = sourceLock.AsRegionPtr();

        var resizedMask = ResizeMask(mask, source.Size.Width, source.Size.Height);

        // apply mask to output
        for (int y = 0; y < source.Size.Height; y++)
        {
            for (int x = 0; x < source.Size.Width; x++)
            {
                var srcPixel = sourceRegion[x, y];
                var alpha = (byte)(resizedMask[y, x] * 255);
                outputRegion[x, y] = ColorBgra.FromBgra(srcPixel.B, srcPixel.G, srcPixel.R, alpha);
            }
        }
    }

    private static float[,,] LoadAndPreprocessImage(IBitmapLock<ColorBgra32> lockObj, int width, int height)
    {
        var data = new float[3, height, width];
        var region = lockObj.AsRegionPtr();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = region[x, y];
                data[0, y, x] = (pixel.R / 255f - 0.485f) / 0.229f; // r
                data[1, y, x] = (pixel.G / 255f - 0.456f) / 0.224f; // g
                data[2, y, x] = (pixel.B / 255f - 0.406f) / 0.225f; // b
            }
        }

        return data;
    }

    private static float[,] ResizeMask(float[,] mask, int targetWidth, int targetHeight)
    {
        int maskHeight = mask.GetLength(0);
        int maskWidth = mask.GetLength(1);
        float[,] resizedMask = new float[targetHeight, targetWidth];

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float origY = (float)y / targetHeight * maskHeight;
                float origX = (float)x / targetWidth * maskWidth;
                int y0 = (int)origY;
                int x0 = (int)origX;
                int y1 = Math.Min(y0 + 1, maskHeight - 1);
                int x1 = Math.Min(x0 + 1, maskWidth - 1);

                float v00 = mask[y0, x0];
                float v01 = mask[y0, x1];
                float v10 = mask[y1, x0];
                float v11 = mask[y1, x1];

                float xFrac = origX - x0;
                float yFrac = origY - y0;

                float interpolatedValue = (1 - xFrac) * (1 - yFrac) * v00 +
                                         xFrac * (1 - yFrac) * v01 +
                                         (1 - xFrac) * yFrac * v10 +
                                         xFrac * yFrac * v11;

                resizedMask[y, x] = interpolatedValue;
            }
        }

        return resizedMask;
    }

    public void Dispose()
    {
        _inferenceSession.Dispose();
        GC.SuppressFinalize(this);
    }
}