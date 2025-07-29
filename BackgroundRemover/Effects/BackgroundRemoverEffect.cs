using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.Imaging;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackgroundRemover.Effects;

[PluginSupportInfo(typeof(PluginSupportInfo))]
public class BackgroundRemoverEffect : PropertyBasedBitmapEffect
{
    private const string IconResourcePath = "BackgroundRemover.Assets.ic_fluent_video_background_effect_24_filled.ico";
    private const string ONNX16FileName = "RMBG-2.0_FP16.onnx";
    private const string ONNX32FileName = "RMBG-2.0_FP32.onnx";

    private const string ErrorDialogTitle = "Background Remover Error";

    private readonly string _onnxPathFp16;
    private readonly string _onnxPathFp32;

    private static Bitmap Icon
    {
        get
        {
            if (null == field)
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var iconStream = assembly.GetManifestResourceStream(IconResourcePath)!;
                field = new Bitmap(iconStream);
            }

            return field;
        }
    }

    public BackgroundRemoverEffect() : base("Background Removal", Icon, SubmenuNames.Photo, BitmapEffectOptionsFactory.Create() with { IsConfigurable = true })
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        _onnxPathFp16 = Path.Combine(baseDir, "onnx", ONNX16FileName);
        _onnxPathFp32 = Path.Combine(baseDir, "onnx", ONNX32FileName);

        if (!File.Exists(_onnxPathFp16) || !File.Exists(_onnxPathFp32))
            ShowErrorDialog("Required ONNX models missing. Please ensure both FP16 and FP32 models are in the 'onnx' folder.");
    }

    // properties
    private enum PropertyName
    {
        FP16,
        GPU,
    }

    protected override PropertyCollection OnCreatePropertyCollection() => new([
        new BooleanProperty(PropertyName.FP16, false),
        new BooleanProperty(PropertyName.GPU,  false),
    ]);

    protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
    {
        var config = CreateDefaultConfigUI(props);

        config.SetPropertyControlValue(PropertyName.FP16, ControlInfoPropertyNames.DisplayName, "Use FP16");
        config.SetPropertyControlValue(PropertyName.FP16, ControlInfoPropertyNames.Description, "Enable FP16 computation to reduce memory usage and potentially improve performance.");
        config.SetPropertyControlValue(PropertyName.FP16, ControlInfoPropertyNames.ShowHeaderLine, false);

        config.SetPropertyControlValue(PropertyName.GPU, ControlInfoPropertyNames.DisplayName, "Use GPU");
        config.SetPropertyControlValue(PropertyName.GPU, ControlInfoPropertyNames.Description, "Enable GPU acceleration to accelerate background removal.");
        config.SetPropertyControlValue(PropertyName.GPU, ControlInfoPropertyNames.ShowHeaderLine, false);

        return config;
    }

    // rendering
    private IEffectInputBitmap<ColorBgra32>? _source;
    private IBitmap<ColorBgra32>? _bitmap;

    private readonly Lock _lock = new();

    protected override void OnSetToken(PropertyBasedEffectConfigToken? token)
    {
        var useFp16 = (bool)token!.GetProperty(PropertyName.FP16)!.Value!;
        var useGpu = (bool)token.GetProperty(PropertyName.GPU)!.Value!;

        lock (_lock) // ensure thread safety when accessing bitmaps
            RemoveBackground(useFp16, useGpu);
    }

    private void RemoveBackground(bool useFp16, bool useGpu)
    {
        using var sessionOptions = new SessionOptions { LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_FATAL };
        using var runOptions = new RunOptions();
        var onnx = useFp16 ? _onnxPathFp16 : _onnxPathFp32;

        if (useGpu)
            sessionOptions.AppendExecutionProvider_DML(0);

        _source ??= Environment.GetSourceBitmapBgra32();
        using var data = _source.CreateScaler(new(1024, 1024), BitmapInterpolationMode.HighQualityCubic).ToBitmap();
        using var datL = data.Lock(new(0, 0, data.Size), BitmapLockOptions.ReadWrite);
        var dat = datL.AsRegionPtr();

        var color = new DenseTensor<float>([1, 3, dat.Height, dat.Width]);
        var alpha = new DenseTensor<float>([1, 1, dat.Height, dat.Width]);
        using var vcolor = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, color.Buffer, [1, 3, dat.Height, dat.Width]);
        using var valpha = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, alpha.Buffer, [1, 1, dat.Height, dat.Width]);

        for (var y = 0; y < dat.Height; y++)
        {
            for (var x = 0; x < dat.Width; x++)
            {
                color[0, 0, y, x] = (dat[x, y].R / 255f - 0.485f) / 0.229f;
                color[0, 1, y, x] = (dat[x, y].G / 255f - 0.456f) / 0.224f;
                color[0, 2, y, x] = (dat[x, y].B / 255f - 0.406f) / 0.225f;
            }
        }

        try
        {
            using var session = new InferenceSession(onnx, sessionOptions);

            var task = Task.Run(() => session.Run(runOptions, session.InputNames, [vcolor], session.OutputNames, [valpha]));
            while (!task.IsCompleted)
            {
                runOptions.Terminate = IsCancelRequested;
                Thread.Sleep(100);
            }

            for (var y = 0; y < dat.Height; y++)
            {
                for (var x = 0; x < dat.Width; x++)
                    dat[x, y].A = (byte)MathF.Round(alpha[0, 0, y, x] * 255f, MidpointRounding.AwayFromZero);
            }
        }
        catch (Exception ex)
        {
            ShowErrorDialog($"Background removal failed:\n{ex.Message}\nTry disabling GPU usage and try again.");
        }

        DisposableUtil.Free(ref _bitmap);
        _bitmap = data.CreateScaler(_source.Size, BitmapInterpolationMode.HighQualityCubic).ToBitmap();
    }

    protected override void OnRender(IBitmapEffectOutput output)
    {
        using var dstL = output.LockBgra32(); var dst = dstL.AsRegionPtr();
        using var srcL = _source!.Lock(output.Bounds); var src = srcL.AsRegionPtr();
        using var auxL = _bitmap!.Lock(output.Bounds, BitmapLockOptions.Read); var aux = auxL.AsRegionPtr();

        for (var y = 0; y < dst.Height; y++)
        {
            for (var x = 0; x < dst.Width; x++)
                dst[x, y] = new(src[x, y].Bgr, aux[x, y].A);
        }
    }

    private static void ShowErrorDialog(string message) => MessageBox.Show(message, ErrorDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);

    protected override void OnDispose(bool disposing)
    {
        DisposableUtil.Free(ref _bitmap, disposing);

        base.OnDispose(disposing);
    }
}