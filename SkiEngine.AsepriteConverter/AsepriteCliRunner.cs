using System;
using System.Diagnostics;
using System.IO;

namespace SkiEngine.AsepriteConverter
{
    public class AsepriteCliRunner
    {
        private readonly IAsepriteCliRunnerOptions _options;

        public AsepriteCliRunner(IAsepriteCliRunnerOptions options)
        {
            _options = options;
        }

        public void Run()
        {
            Directory.CreateDirectory(_options.OutputPngDirectory);

            var exportAnimationProcess = new Process
            {
                StartInfo =
                {
                    FileName = _options.AsepriteExePath,
                    Arguments =
                        $" -b --split-layers --list-tags --list-layers --format json-array " +
                        $"--inner-padding {_options.InnerPadding} " +
                        $"\"{_options.AnimationFilePath}\" " +
                        (_options.PalettePath == null ? "" : $"--palette \"{_options.PalettePath}\" ") +
                        $"--sheet \"{Path.Combine(_options.OutputPngDirectory, _options.OutputName + ".png")}\" " +
                        $"--data \"{_options.TempJsonOutputPath}\" ",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            exportAnimationProcess.Start();

            var err = exportAnimationProcess.StandardError.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(err))
            {
                Debug.WriteLine(err);
            }

            exportAnimationProcess.WaitForExit();
            if (exportAnimationProcess.ExitCode != 0)
            {
                throw new Exception(".ase exporting failed");
            }
        }
    }
}
