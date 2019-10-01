﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.DotNet.Tools.Test.Utilities;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Microsoft.DotNet.Tests.EndToEnd
{
    public class GivenDotNetUsesMSBuild : TestBase
    {
        [Fact]
        public void ItCanNewRestoreBuildRunCleanMSBuildProject()
        {
            var directory = TestAssets.CreateTestDirectory();
            string projectDirectory = directory.FullName;

            string newArgs = "console --debug:ephemeral-hive --no-restore";
            var newResult = new NewCommandShim()
                .WithWorkingDirectory(projectDirectory)
                .ExecuteWithCapturedOutput(newArgs);

            newResult.Should().Pass();

            string projectPath = Directory.GetFiles(projectDirectory, "*.csproj").Single();

            //  Override TargetFramework since there aren't .NET Core 3 templates yet
            //  https://github.com/dotnet/core-sdk/issues/24 tracks removing this workaround
            XDocument project = XDocument.Load(projectPath);
            var ns = project.Root.Name.Namespace;
            project.Root.Element(ns + "PropertyGroup")
                .Element(ns + "TargetFramework")
                .Value = "netcoreapp3.1";
            project.Save(projectPath);


            new RestoreCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute("/p:SkipInvalidConfigurations=true")
                .Should().Pass();

            new BuildCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should().Pass();

            var runCommand = new RunCommand()
                .WithWorkingDirectory(projectDirectory);

            //  Set DOTNET_ROOT as workaround for https://github.com/dotnet/cli/issues/10196
            runCommand = runCommand.WithEnvironmentVariable(Environment.Is64BitProcess ? "DOTNET_ROOT": "DOTNET_ROOT(x86)",
                Path.GetDirectoryName(DotnetUnderTest.FullName));

            runCommand.ExecuteWithCapturedOutput()
                .Should().Pass()
                     .And.HaveStdOutContaining("Hello World!");

            var binDirectory = new DirectoryInfo(projectDirectory).Sub("bin");
            binDirectory.Should().HaveFilesMatching("*.dll", SearchOption.AllDirectories);

            new CleanCommand()
                .WithWorkingDirectory(projectDirectory)
                .Execute()
                .Should().Pass();

            binDirectory.Should().NotHaveFilesMatching("*.dll", SearchOption.AllDirectories);
        }
    }
}
