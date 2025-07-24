using BackgroundRemover.Effects;
using PaintDotNet.Effects;

namespace Dialogs;

public partial class BackgroundRemoverConfigDialog : EffectConfigForm2
{
    private bool _finalize = false;

    public BackgroundRemoverConfigDialog()
    {
        InitializeComponent();

        fp16CheckBox.CheckedChanged += (_, _) => UpdateTokenFromDialog();
        gpuCheckBox.CheckedChanged += (_, _) => UpdateTokenFromDialog();
        button1.Click += (_, _) =>
        {
            _finalize = true;
            UpdateTokenFromDialog();
        };
    }

    protected override EffectConfigToken OnCreateInitialToken() => new BackgroundRemoverConfig(false, false, true);

    protected override void OnUpdateDialogFromToken(EffectConfigToken token)
    {
        if (token is not BackgroundRemoverConfig config)
            return;

        fp16CheckBox.Checked = config.UseFP16;
        gpuCheckBox.Checked = config.UseGPU;
    }

    protected override void OnUpdateTokenFromDialog(EffectConfigToken dstToken)
    {
        var config = (BackgroundRemoverConfig)dstToken;
        config.Finalize = _finalize;
        config.UseFP16 = fp16CheckBox.Checked;
        config.UseGPU = gpuCheckBox.Checked;
    }
}