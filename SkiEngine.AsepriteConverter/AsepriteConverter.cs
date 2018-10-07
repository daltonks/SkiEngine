using System;
using System.IO;
using Engine.Aseprite.Models;

namespace SkiEngine.AsepriteConverter
{
    public class AsepriteConverter : IAsepriteCodeGeneratorOptions, IAsepriteCliRunnerOptions
    {
        public string AsepriteExePath { get; set; }
        public string AnimationFilePath { get; set; }
        public string PalettePath { get; set; }
        public int InnerPadding { get; set; }
        public string ContentDirectory { get; set; }
        public string OutputPngContentDirectory { get; set; }
        public string OutputGAnimContentDirectory { get; set; }
        public string OutputClassContentDirectory { get; set; }
        public string OutputClassNamespace { get; set; }
        public string OutputName { get; set; }

        public string OutputPngDirectory => Path.Combine(ContentDirectory, OutputPngContentDirectory);
        public string OutputClassDirectory => Path.Combine(ContentDirectory, OutputClassContentDirectory);
        public string OutputGAnimDirectory => Path.Combine(ContentDirectory, OutputGAnimContentDirectory);

        public string TempJsonOutputPath { get; } = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        public AsepriteCliDataModel AsepriteCliDataModel { get; private set; }

        public void Run()
        {
            // Run Aseprite CLI
            var cliRunner = new AsepriteCliRunner(this);
            cliRunner.Run();

            // Get CLI model
            AsepriteCliDataModel = AsepriteCliDataModel.FromPath(TempJsonOutputPath);

            // Generate code
            using (var codeGenerator = new AsepriteCodeGenerator(this))
            {
                codeGenerator.Generate();
            }

            // Create .ganim file
            using(var fileStream = File.Open(Path.Combine(OutputGAnimDirectory, OutputName + ".ganim"), FileMode.Create))
            {
                var animationData = AsepriteCliDataModel.ToGSpriteSheetAnimationData();
                animationData.WriteTo(fileStream);
            }
        }
    }
}
