// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace Microsoft.DotNet.GenAPI.Tool
{
    /// <summary>
    /// CLI frontend for the Roslyn-based GenAPI.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            // Global options
            Option<string[]> assembliesOption = new("--assembly",
                description: "The path to one or more assemblies or directories with assemblies.",
                parseArgument: ParseAssemblyArgument)
            {
                Arity = ArgumentArity.OneOrMore,
                IsRequired = true
            };

            Option<string[]?> assemblyReferencesOption = new("--assembly-reference",
                description: "Paths to assembly references or their underlying directories for a specific target framework in the package.",
                parseArgument: ParseAssemblyArgument)
            {
                Arity = ArgumentArity.ZeroOrMore
            };

            Option<string[]?> excludeAttributesFilesOption = new("--exclude-attributes-file",
                description: "The path to one or more attribute exclusion files with types in DocId format.",
                parseArgument: ParseAssemblyArgument)
            {
                Arity = ArgumentArity.ZeroOrMore
            };

            Option<string?> outputPathOption = new("--output-path",
                @"Output path. Default is the console. Can specify an existing directory as well
            and then a file will be created for each assembly with the matching name of the assembly.");

            Option<string?> headerFileOption = new("--header-file",
                "Specify a file with an alternate header content to prepend to output.");

            Option<string?> exceptionMessageOption = new("--exception-message",
                "If specified - method bodies should throw PlatformNotSupportedException, else `throw null`.");

            Option<bool> includeVisibleOutsideOfAssemblyOption = new("--include-visible-outside",
                "Include internal API's. Default is false.");

            RootCommand rootCommand = new("Microsoft.DotNet.GenAPI")
            {
                TreatUnmatchedTokensAsErrors = true
            };
            rootCommand.AddGlobalOption(assembliesOption);
            rootCommand.AddGlobalOption(assemblyReferencesOption);
            rootCommand.AddGlobalOption(excludeAttributesFilesOption);
            rootCommand.AddGlobalOption(outputPathOption);
            rootCommand.AddGlobalOption(headerFileOption);
            rootCommand.AddGlobalOption(exceptionMessageOption);
            rootCommand.AddGlobalOption(includeVisibleOutsideOfAssemblyOption);

            rootCommand.SetHandler((InvocationContext context) =>
            {
                GenAPIApp.Run(new GenAPIApp.Context(
                    context.ParseResult.GetValue(assembliesOption)!,
                    context.ParseResult.GetValue(assemblyReferencesOption),
                    context.ParseResult.GetValue(outputPathOption),
                    context.ParseResult.GetValue(headerFileOption),
                    context.ParseResult.GetValue(exceptionMessageOption),
                    context.ParseResult.GetValue(excludeAttributesFilesOption),
                    context.ParseResult.GetValue(includeVisibleOutsideOfAssemblyOption)
                ));
            });

            return rootCommand.Invoke(args);
        }

        /// Splits delimiter separated list of pathes represented as a string to a List of paths.
        /// </summary>
        /// <param name="pathSet">Delimiter separated list of paths.</param>
        /// <returns></returns>
        private static string[] ParseAssemblyArgument(ArgumentResult argumentResult)
        {
            List<string> args = new();
            foreach (Token token in argumentResult.Tokens)
            {
                args.AddRange(token.Value.Split(','));
            }

            return args.ToArray();
        }
    }
}

