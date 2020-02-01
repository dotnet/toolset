// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.NugetSearch;

namespace Microsoft.DotNet.Tools.Tool.Search
{
    internal class ToolSearchCommand : CommandBase
    {
        private readonly INugetSearchApiRequest _nugetSearchApiRequest;
        private readonly AppliedOption _options;
        private readonly SearchResultPrinter _searchResultPrinter;

        public ToolSearchCommand(
            AppliedOption options,
            ParseResult result,
            INugetSearchApiRequest nugetSearchApiRequest = null
        )
            : base(result)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _nugetSearchApiRequest = nugetSearchApiRequest ?? new NugetSearchApiRequest();
            _searchResultPrinter = new SearchResultPrinter(Reporter.Output);
        }

        public override int Execute()
        {
            var isDetailed = _options.ValueOrDefault<bool>("detail");
            NugetSearchApiParameter nugetSearchApiParameter = new NugetSearchApiParameter(_options);
            IReadOnlyCollection<SearchResultPackage> searchResultPackages =
                NugetSearchApiResultDeserializer.Deserialize(
                    _nugetSearchApiRequest.GetResult(nugetSearchApiParameter));

            _searchResultPrinter.Print(isDetailed, searchResultPackages);

            return 0;
        }
    }
}
