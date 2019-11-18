// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Tools
{
    internal class FilePermissionSetter : IFilePermissionSetter
    {
        public void SetUserExecutionPermission(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            CommandResult result = new CommandFactory.CommandFactory()
                .Create("chmod", new[] { "g=r,o=r,u=r", path })   // -r--r--r--
                .CaptureStdOut()
                .CaptureStdErr()
                .Execute();

            if (result.ExitCode != 0)
            {
                throw new FilePermissionSettingException(result.StdErr);
            }
        }
    }
}
