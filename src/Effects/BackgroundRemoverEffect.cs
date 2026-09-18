using BackgroundRemover.Assets.Localization;
using BackgroundRemover.Models;
using BackgroundRemover.Services;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using PaintDotNet;
using PaintDotNet.AppModel;
using PaintDotNet.Direct2D1;
using PaintDotNet.Direct2D1.Effects;
using PaintDotNet.Effects;
using PaintDotNet.Effects.Gpu;
using PaintDotNet.Imaging;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using PaintDotNet.Rendering;
using System.Numerics;
using System.Reflection;
using IDeviceContext = PaintDotNet.Direct2D1.IDeviceContext;

namespace BackgroundRemover.Effects;

[PluginSupportInfo(typeof(PluginSupportInfo))]
public class BackgroundRemoverEffect : PropertyBasedGpuImageEffect
{
    private ONNXModelService? _modelService;
    private ONNXModelService ModelService
    {
        get
        {
            if (null == _modelService)
                _modelService = new ONNXModelService(Services.GetService<IDxgiAdapterService2>()!);

            return _modelService;
        }
    }

    private bool _cpuFallbackNotified;

    private const string IconResourcePath = @"BackgroundRemover.Assets.ic_fluent_video_background_effect_24_filled.ico";

    enum PropertyName
    {
        Model,
        UseDirectML
    }

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

    public BackgroundRemoverEffect() : base(Strings.EffectName, Icon, SubmenuNames.Photo, GpuImageEffectOptionsFactory.Create() with { IsConfigurable = true }) { }

    protected override PropertyCollection OnCreatePropertyCollection()
    {
        var properties = new List<Property>() { new BooleanProperty(PropertyName.UseDirectML, true) };

        var onnxDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "onnx");
        try
        {
            ModelService.LoadModels(onnxDirectory);
        }
        catch (Exception ex)
        {
            ShowErrorDialog($"{Strings.LoadModelsFailed}\n\n{ex.Message}");
            return new PropertyCollection(properties);
        }

        if (ModelService.Models.Length == 0)
        {
            ShowErrorDialog(Strings.NoModelsFound);
        }
        else
        {
            properties.Add(new StaticListChoiceProperty(PropertyName.Model, ModelService.Models));
        }

        return new PropertyCollection(properties);
    }

    protected override ControlInfo OnCreateConfigUI(PropertyCollection properties)
    {
        var config = CreateDefaultConfigUI(properties);

        config.SetPropertyControlValue(PropertyName.UseDirectML, ControlInfoPropertyNames.DisplayName, Strings.UseDirectML);
        config.SetPropertyControlValue(PropertyName.UseDirectML, ControlInfoPropertyNames.Description, Strings.UseDirectMLDescription);
        config.SetPropertyControlValue(PropertyName.UseDirectML, ControlInfoPropertyNames.ShowHeaderLine, false);

        config.SetPropertyControlValue(PropertyName.Model, ControlInfoPropertyNames.DisplayName, Strings.ModelDisplayName);
        config.SetPropertyControlValue(PropertyName.Model, ControlInfoPropertyNames.Description, Strings.ModelDescription);
        config.SetPropertyControlValue(PropertyName.Model, ControlInfoPropertyNames.ShowHeaderLine, false);

        return config;
    }

    protected override void OnInitializeRenderInfo(IGpuImageEffectRenderInfo renderInfo)
    {
        renderInfo.ColorContext = GpuEffectColorContext.WorkingSpace;
        renderInfo.InputAlphaMode = GpuEffectAlphaMode.Straight;
        renderInfo.OutputAlphaMode = GpuEffectAlphaMode.Straight;

        base.OnInitializeRenderInfo(renderInfo);
    }

    // rendering
    protected override IDeviceImage OnCreateOutput(IDeviceContext deviceContext)
    {
        var property = Token.GetProperty(PropertyName.Model);

        if (null == property)
            return Environment.SourceImage;

        var model = (ONNXModel)property.Value!;
        var useDirectML = Token.GetProperty<BooleanProperty>(PropertyName.UseDirectML)!.Value;

        InferenceSession session;
        try
        {
            session = ModelService.GetOrCreateSession(model, useDirectML);

            if (ModelService.LastSessionUsedCpuFallback && !_cpuFallbackNotified)
            {
                _cpuFallbackNotified = true;
                ShowErrorDialog(Strings.FallbackToCpu);
            }
        }
        catch (Exception ex)
        {
            ShowErrorDialog($"{Strings.InitializationFailed}\n\n{ex.Message}");
            return Environment.SourceImage;
        }

        using var source = Environment.SourceImage.CreateRef();
        var sourceSize = (Vector2)Environment.Document.Size;

        using var scaleEffect = new ScaleEffect(deviceContext, source, model.InputSize / sourceSize, default, ScaleInterpolationMode.HighQualityCubic, BorderMode.Hard);
        using var datS = deviceContext.CreateBitmapSourceFromImage<ColorPrgba128Float>((SizeInt32)model.InputSize, scaleEffect);
        using var data = datS.ToBitmap();
        using var datL = data.Lock(BitmapLockOptions.ReadWrite);
        var dat = datL.AsRegionPtr().Cast<Vector4>();

        var width = dat.Width;
        var height = dat.Height;
        var planeSize = width * height;

        var color = new DenseTensor<float>([1, 3, height, width]);
        var alpha = new DenseTensor<float>([1, 1, height, width]);
        using var vcolor = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, color.Buffer, [1, 3, height, width]);
        using var valpha = OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, alpha.Buffer, [1, 1, height, width]);

        var colorMemory = color.Buffer;
        Parallel.For(0, height, y =>
        {
            var colorSpan = colorMemory.Span;
            var rowOffset = y * width;
            for (var x = 0; x < width; x++)
            {
                var px = dat[x, y];
                var i = rowOffset + x;
                colorSpan[i] = (px.X - 0.485f) / 0.229f;
                colorSpan[planeSize + i] = (px.Y - 0.456f) / 0.224f;
                colorSpan[2 * planeSize + i] = (px.Z - 0.406f) / 0.225f;
            }
        });

        try
        {
            try
            {
                RunInference(session);
            }
            catch (OnnxRuntimeException ex) when (ex.Message.Contains("Exiting due to terminate flag being set to true")) { } // cancelled
            catch (OnnxRuntimeException) when (useDirectML && !ModelService.LastSessionUsedCpuFallback)
            {
                session = ModelService.FallBackToCpu(model);

                if (!_cpuFallbackNotified)
                {
                    _cpuFallbackNotified = true;
                    ShowErrorDialog(Strings.FallbackToCpu);
                }

                RunInference(session);
            }

            var alphaMemory = alpha.Buffer;
            var alphaSpan = alphaMemory.Span;
            var (min, max) = (+1 / 0f, -1 / 0f);
            for (var i = 0; i < alphaSpan.Length; i++)
                (min, max) = (Math.Min(min, alphaSpan[i]), Math.Max(max, alphaSpan[i]));

            var range = max > min ? max - min : 1f; // guard against degenerate alpha output
            Parallel.For(0, height, y =>
            {
                var localAlphaSpan = alphaMemory.Span;
                var rowOffset = y * width;
                for (var x = 0; x < width; x++)
                    dat[x, y] = new(new(1), (localAlphaSpan[rowOffset + x] - min) / range);
            });

            using var render = deviceContext.CreateImageFromBitmap(data, (BitmapImageOptions)5);
            using var oscale = new ScaleEffect(deviceContext, render, sourceSize / model.InputSize, default, ScaleInterpolationMode.HighQualityCubic, BorderMode.Hard);
            using var output = new AlphaMaskEffect2(deviceContext, source, oscale, AlphaMaskAlphaMode2.Straight);

            return output.CreateRef();
        }
        catch (Exception ex)
        {
            ShowErrorDialog($"{Strings.BackgroundRemovalFailed}\n\n{ex.Message}");
        }

        return Environment.SourceImage; // don't modify image on error

        void RunInference(InferenceSession s)
        {
            using var runOptions = new RunOptions();
            var task = Task.Run(() => s.Run(runOptions, s.InputNames, [vcolor], s.OutputNames, [valpha]));
            while (!task.IsCompleted)
            {
                runOptions.Terminate = IsCancelRequested;
                Thread.Sleep(100);
            }

            task.GetAwaiter().GetResult(); // unwrap to original exception
        }
    }

    private static void ShowErrorDialog(string message)
        => MessageBox.Show(message, Strings.ErrorDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

    protected override void OnDispose(bool disposing)
    {
        _modelService?.Dispose();

        base.OnDispose(disposing);
    }
}
