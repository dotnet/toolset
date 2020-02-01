// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.DotNet.NugetSearch;
using Microsoft.DotNet.Tools.Test.Utilities;
using Xunit;

namespace dotnet.Tests.ToolSearchTests
{
    public class NugetSearchApiRequestTests : TestBase
    {
        [Fact]
        public void WhenPassedInRequestParametersItCanConstructTheUrl()
        {
            NugetSearchApiRequest.ConstructUrl("mytool", 3, 4, true, "1.0.0")
                .AbsoluteUri
                .Should().Be(
                    "https://azuresearch-usnc.nuget.org/query?q=mytool&packageType=dotnettool&skip=3&take=4&prerelease=true&semVerLevel=1.0.0");
        }

        [Fact]
        public void WhenPassedWithoutParameterItCanConstructTheUrl()
        {
            NugetSearchApiRequest.ConstructUrl()
                .AbsoluteUri
                .Should().Be(
                    "https://azuresearch-usnc.nuget.org/query?packageType=dotnettool");
        }
    }
}
