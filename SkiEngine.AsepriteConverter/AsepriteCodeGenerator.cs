using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkiEngine.AsepriteConverter
{
    public class AsepriteCodeGenerator : IDisposable
    {
        private readonly IAsepriteCodeGeneratorOptions _options;
        private IndentedTextWriter _indentWriter;

        public AsepriteCodeGenerator(IAsepriteCodeGeneratorOptions options)
        {
            _options = options;
        }

        public void Generate()
        {
            Directory.CreateDirectory(_options.OutputClassDirectory);

            var path = Path.Combine(_options.OutputClassDirectory, $"{_options.OutputName}AnimationComponent.cs");
            var fileStream = File.Open(path, FileMode.Create);
            var streamWriter = new StreamWriter(fileStream);
            _indentWriter = new IndentedTextWriter(streamWriter, "    ") { Indent = 0 };

            WriteLine(@"using System.IO;
using Engine.ECS.Component;
using Engine.DataModel;
using Engine.Util;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;

//  _______  __   __  _______  _______                                                    
// |   _   ||  | |  ||       ||       |                                                   
// |  |_|  ||  | |  ||_     _||   _   |                                                   
// |       ||  |_|  |  |   |  |  | |  |                                                   
// |       ||       |  |   |  |  |_|  |                                                   
// |   _   ||       |  |   |  |       |                                                   
// |__| |__||_______|  |___|  |_______|                                                   
//  _______  _______  __    _  _______  ______    _______  _______  _______  ______   __  
// |       ||       ||  |  | ||       ||    _ |  |   _   ||       ||       ||      | |  | 
// |    ___||    ___||   |_| ||    ___||   | ||  |  |_|  ||_     _||    ___||  _    ||  | 
// |   | __ |   |___ |       ||   |___ |   |_||_ |       |  |   |  |   |___ | | |   ||  | 
// |   ||  ||    ___||  _    ||    ___||    __  ||       |  |   |  |    ___|| |_|   ||__| 
// |   |_| ||   |___ | | |   ||   |___ |   |  | ||   _   |  |   |  |   |___ |       | __  
// |_______||_______||_|  |__||_______||___|  |_||__| |__|  |___|  |_______||______| |__| ");

            WriteLine();
            WriteLine("// ReSharper disable InconsistentNaming");

            WriteLine();
            WriteLine($"namespace {_options.OutputClassNamespace}");
            WriteLineBlock(WriteModel);
        }

        private void WriteModel()
        {
            var fileName = ToFriendlyName(_options.OutputName);
            var statesEnumName = ToFriendlyName($"{fileName}AnimationStates");
            var layersEnumName = ToFriendlyName($"{fileName}AnimationLayers");

            WriteRendererScriptClass($"{fileName}AnimationComponent", statesEnumName, layersEnumName);
            WriteLine();
            WriteEnum(statesEnumName, _options.AsepriteCliDataModel.Metadata.Tags.Select(t => t.Name));
            WriteLine();
            WriteEnum(layersEnumName, _options.AsepriteCliDataModel.Metadata.Layers.Select(l => l.Name));
            WriteLine();
        }

        private void WriteRendererScriptClass(string className, string statesEnumName, string layersEnumName)
        {
            WriteLine($"public partial class {className} : SpriteSheetAnimationComponent<{statesEnumName}, {layersEnumName}>");
            WriteLineBlock(
                () =>
                {
                    WriteLine($@"private static readonly object _lock = new object();
        private static Texture2D _texture;
        private static byte[] _gAnimBytes;

        private static Texture2D InitializeTextureAndGAnimBytes(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {{
            if (_texture == null)
            {{
                lock (_lock)
                {{
                    if (_texture == null)
                    {{
                        using (var textureStream = contentManager.OpenStream(""{Path.Combine( _options.OutputPngContentDirectory, _options.OutputName)}.png""))
                        {{
                            _texture = Texture2D.FromStream(graphicsDevice, textureStream);
                        }}

                        using (var ganimStream = contentManager.OpenStream(""{Path.Combine(_options.OutputGAnimContentDirectory, _options.OutputName)}.ganim""))
                        using (var memoryStream = new MemoryStream())
                        {{
                            ganimStream.CopyTo(memoryStream);
                            _gAnimBytes = memoryStream.ToArray();
                        }}
                    }}
                }}
            }}

            return _texture;
        }}

        private static GSpriteSheetAnimationData CreateData()
        {{
            using(var memoryStream = new MemoryStream(_gAnimBytes))
            {{
                return GSpriteSheetAnimationData.ReadFrom(memoryStream);
            }}
        }}

        public {className}(ContentManager contentManager, GraphicsDevice graphicsDevice, OneOrMore<(CameraComponent, int)> cameraAndDrawOrders)
            : base(InitializeTextureAndGAnimBytes(contentManager, graphicsDevice), CreateData(), cameraAndDrawOrders)
        {{

        }}"
                    );
                }
            );
        }

        private void WriteEnum(string name, IEnumerable<string> valueNames)
        {
            WriteLine($"public enum {name}");
            WriteLineBlock(() => {
                foreach (var valueName in valueNames)
                {
                    WriteLine($"{ToFriendlyName(valueName)},");
                }
            });
        }

        private void Write(object o)
        {
            _indentWriter.Write(o);
        }

        private void WriteLine()
        {
            _indentWriter.WriteLine();
        }

        private void WriteLine(object o)
        {
            _indentWriter.WriteLine(o);
        }

        private void WriteLineBlock(Action action)
        {
            WriteBlock(action);
            WriteLine();
        }

        private void WriteBlock(Action action)
        {
            WriteLine("{");
            Indent(action);
            Write("}");
        }

        private void Indent(Action action)
        {
            _indentWriter.Indent++;
            action.Invoke();
            _indentWriter.Indent--;
        }

        private string ToFriendlyName(string str)
        {
            var friendlyName = "";
            if (char.IsDigit(str[0]))
            {
                friendlyName = "_";
            }
            friendlyName += string.Concat(str.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
            return friendlyName;
        }

        public void Dispose()
        {
            _indentWriter.Flush();
            _indentWriter.Dispose();
        }
    }
}
