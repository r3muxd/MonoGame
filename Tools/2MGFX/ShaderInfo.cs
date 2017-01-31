// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TwoMGFX.EffectParsing;

namespace TwoMGFX
{
    public class ShaderInfo
	{
		public string FilePath { get; private set; }

		public string FileContent { get; private set; }

		public ShaderProfile Profile { get; private set; }

		public string OutputFilePath { get; private set; }

		public bool Debug { get; private set; }

		public List<TechniqueInfo> Techniques = new List<TechniqueInfo>();

        public Dictionary<string, SamplerStateInfo> SamplerStates = new Dictionary<string, SamplerStateInfo>();

        public List<string> Dependencies { get; private set; }

        public List<string> AdditionalOutputFiles { get; private set; }

        static public ShaderInfo FromFile(string path, Options options, IEffectCompilerOutput output)
		{
			var effectSource = File.ReadAllText(path);
			return FromString(effectSource, path, options, output);
		}

		static public ShaderInfo FromString(string effectSource, string filePath, Options options, IEffectCompilerOutput output)
		{
			var macros = new Dictionary<string, string>();
			macros.Add("MGFX", "1");

			options.Profile.AddMacros(macros);

			// If we're building shaders for debug set that flag too.
			if (options.Debug)
				macros.Add("DEBUG", "1");

		    if (!string.IsNullOrEmpty(options.Defines))
		    {
		        var defines = options.Defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var define in defines)
                    macros.Add(define, "1");
		    }

		    // Use the D3DCompiler to pre-process the file resolving 
			// all #includes and macros.... this even works for GLSL.
		    var fullPath = Path.GetFullPath(filePath);
		    var dependencies = new List<string>();
		    var newFile = Preprocessor.Preprocess(effectSource, fullPath, macros, dependencies, output);

			// Parse the resulting file for techniques and passes.
		    ParseResult result;

		    if (!EffectParser.Parse(options.Profile, newFile, out result))
		    {
                var errors = new StringBuilder();
                errors.AppendLine(string.Format("Errors parsing {0}:", fullPath));
		        foreach (var error in result.Errors)
                    errors.AppendLine(string.Format("- ({0}, {1}) -> {2}", error.Line, error.Column, error.Message));

				throw new Exception(errors.ToString());
		    }

		    var shaderInfo = result.ShaderInfo;

            // Remove the samplers and techniques so that the shader compiler
            // gets a clean file without any FX file syntax in it.
		    var cleanFile = EffectParser.GetCleanedFiled(result);

            // Setup the rest of the shader info.
            shaderInfo.Dependencies = dependencies;
            shaderInfo.FilePath = fullPath;
            shaderInfo.FileContent = cleanFile;
            if (!string.IsNullOrEmpty(options.OutputFile))
                shaderInfo.OutputFilePath = Path.GetFullPath(options.OutputFile);
            shaderInfo.AdditionalOutputFiles = new List<string>();

            // Remove empty techniques.
            for (var i=0; i < shaderInfo.Techniques.Count; i++)
            {
                var tech = shaderInfo.Techniques[i];
                if (tech.Passes.Count <= 0)
                {
                    shaderInfo.Techniques.RemoveAt(i);
                    i--;
                }
            }

            // We must have at least one technique.
            if (shaderInfo.Techniques.Count <= 0)
                throw new Exception("The effect must contain at least one technique and pass!");

			shaderInfo.Profile = options.Profile;
			shaderInfo.Debug = options.Debug;

			return shaderInfo;
		}

	}
}
