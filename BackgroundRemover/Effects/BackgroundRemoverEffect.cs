using BackgroundRemover.Utils;
using Dialogs;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.Imaging;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace BackgroundRemover.Effects;

[PluginSupportInfo(typeof(PluginSupportInfo))]
public class BackgroundRemoverEffect : BitmapEffect
{
    private const string IconResourcePath = "BackgroundRemover.Assets.ic_fluent_video_background_effect_24_filled.ico";
    private const string ONNX16FileName = "RMBG-2.0_FP16.onnx";
    private const string ONNX32FileName = "RMBG-2.0_FP32.onnx";

    private const string ErrorDialogTitle = "Background Remover Error";

    private bool _isFinalCall = false;
    private BackgroundRemoverConfig? _config;
    private IEffectInputBitmap<ColorBgra32>? _sourceBitmap;

    public BackgroundRemoverEffect() : base("Background Remover", LoadIcon(), "AI Tools", BitmapEffectOptionsFactory.Create() with { IsConfigurable = true })
    { }

    private static Bitmap LoadIcon()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var iconStream = assembly.GetManifestResourceStream(IconResourcePath)!;
        return new Bitmap(iconStream);
    }

    protected override IEffectConfigForm OnCreateConfigForm() => new BackgroundRemoverConfigDialog();

    protected override void OnSetToken(EffectConfigToken? newToken) => _config = newToken as BackgroundRemoverConfig;

    protected override void OnInitializeRenderInfo(IBitmapEffectRenderInfo renderInfo)
    {
        renderInfo.OutputPixelFormat = PixelFormats.Bgra32;
        renderInfo.Schedule = BitmapEffectRenderingSchedule.None;
        renderInfo.Flags = BitmapEffectRenderingFlags.DisableSelectionClipping;
        _sourceBitmap = Environment.GetSourceBitmapBgra32();
    }

    protected override void OnRender(IBitmapEffectOutput output)
    {
        // don't render if the effect configuration was not finalized
        if (_config == null || !_config.Finalize)
        {
            CopySourceToOutput(output);
        }
        else if (!_isFinalCall)
        {
            CopySourceToOutput(output);
            _isFinalCall = true;
        }
        else
        {
            var modelPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                    "onnx",
                    _config.UseFP16 ? ONNX16FileName : ONNX32FileName
                );

            try
            {
                using var remover = new RMBGBackgroundRemover(modelPath, _config.UseFP16, _config.UseGPU);
                remover.RemoveBackground(_sourceBitmap!, output, CancellationToken);
            }
            catch (OperationCanceledException)
            {
                // catch silently
            }
            catch (Exception ex)
            {
                ShowErrorDialog($"Background removal failed: \n{ex.Message}\nTry disabling GPU usage and try again.");
            }
        }
    }

    private void CopySourceToOutput(IBitmapEffectOutput output)
    {
        using var outputLock = output.Lock<ColorBgra32>();
        _sourceBitmap!.CopyPixels(outputLock, output.Bounds.Location);
    }

    private static void ShowErrorDialog(string message) => MessageBox.Show(message, ErrorDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
}