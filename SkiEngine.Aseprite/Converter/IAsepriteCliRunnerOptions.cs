namespace SkiEngine.Aseprite.Converter
{
    public interface IAsepriteCliRunnerOptions
    {
        string AsepriteExePath { get; }
        string AnimationFilePath { get; }
        string PalettePath { get; }
        int InnerPadding { get; }
        string TempJsonOutputPath { get; }
        string OutputName { get; }
        string OutputPngDirectory { get; }
    }
}
