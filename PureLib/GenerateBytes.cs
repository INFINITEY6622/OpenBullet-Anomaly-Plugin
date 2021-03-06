﻿using PluginFramework;
using PluginFramework.Attributes;
using RuriLib;
using RuriLib.LS;
using System;
using System.Security.Cryptography;

namespace OpenBulletPlugin
{
    public class GenerateBytes : BlockBase, IBlockPlugin
    {
        public string Name => "GenerateBytes";
        public string Color => "BlueViolet";
        public bool LightForeground => false;

        [Text("Variable Name", "The output variable name")]
        public string VariableName { get; set; } = "";

        [Text("Byte Count(int)", "The Result from Verifier Gen")]
        public string VerInput { get; set; } = "";

        [Checkbox("Is Capture", "Should the output variable be marked as capture?")]
        public bool IsCapture { get; set; } = false;

        public GenerateBytes()
        {
            Label = Name;
        }

        public override BlockBase FromLS(string line)
        {
            var input = line.Trim();
            if (input.StartsWith("#")) // If the input actually has a label
                Label = LineParser.ParseLabel(ref input); // Parse the label and remove it from the original string
            VerInput = LineParser.ParseLiteral(ref input, "First Number");

            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == "")
                return this;
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }
            return this;
        }

        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled)
                .Label(Label) // Write the label. If the label is the default one, nothing is written.
                .Token(Name) // Write the block name. This cannot be changed.
                .Literal(VerInput);
            if (!writer.CheckDefault(VariableName, nameof(VariableName)))
            {
                writer
                     .Arrow() // Write the -> arrow.
                     .Token(IsCapture ? "CAP" : "VAR") // Write CAP or VAR depending on IsCapture.
                     .Literal(VariableName); // Write the Variable Name as a literal.
            }
            return writer.ToString();
        }

        public override void Process(BotData data)
        {
            var Input = (ReplaceValues(VerInput, data));
            int z = 0;
            try
            {
                z = System.Convert.ToInt32(Input);
                byte[] genbyte = new byte[z];
                RandomNumberGenerator rando = RandomNumberGenerator.Create();
                rando.GetBytes(genbyte);
                var end = (BitConverter.ToString(genbyte));
                var result = end.ToString();
                InsertVariable(data, IsCapture, result, VariableName, "", "", false, false);
                data.Log($"Generated Bytes with result {result}");
            }
            catch (Exception ex)
            {
                data.Log($"Error Generating Bytes. Make sure input is an integer{ex}");
                throw new ArgumentException("Error Generating Bytes. Make sure input is an integer");
            }
        }
    }
}