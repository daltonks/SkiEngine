using Engine.Aseprite.Models;

namespace SkiEngine.AsepriteConverter
{
    public interface IAsepriteCodeGeneratorOptions
    {
        string OutputClassNamespace { get; }
        string OutputClassDirectory { get; }
        string OutputName { get; }
        AsepriteCliDataModel AsepriteCliDataModel { get; }
        string OutputPngContentDirectory { get; }
        string OutputGAnimContentDirectory { get; }
    }
}
