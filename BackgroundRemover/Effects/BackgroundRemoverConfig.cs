using PaintDotNet.Effects;

namespace BackgroundRemover.Effects;

public class BackgroundRemoverConfig(bool finalize, bool useFP16, bool useGPU) : EffectConfigToken
{
    public bool Finalize { get; set; } = finalize;
    public bool UseFP16 { get; set; } = useFP16;
    public bool UseGPU { get; set; } = useGPU;

    public override object Clone() => new BackgroundRemoverConfig(Finalize, UseFP16, UseGPU);

    public override string ToString() => $"FP16: {UseFP16}; GPU: {UseGPU}; Finalize: {Finalize}";
}