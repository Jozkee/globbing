using System.IO;
using System;
using Xunit;
using GlobbingTests.Helpers;

namespace Globbing.Tests
{
    /// <summary>
    /// Tests GlobbingEnumerable class by creating temporary directories and verifying that they are correctly fetched.
    /// </summary>
    /// <remarks>
    /// All absolute paths shall be written without root. An empty string may refer to the root directory.
    /// </remarks>
    public class GlobbingTests
    {
        private static string s_root => Path.GetPathRoot(Environment.CurrentDirectory);

        [TheoryWindowsOnly]
        [InlineData("globtest", "", "globtest")]
        [InlineData("globtest", "", "globte*t")]
        [InlineData("globtest/foo", "", "globtest/*")]
        [InlineData("globtest/foo.txt", "", "globtest/*.*")]
        public void TestMatch_AbsoluteDirectory_AbsolutePattern(string searchFor, string directory, string pattern)
        {
            searchFor = s_root + searchFor;
            directory = s_root + directory;

            using var file = new TempDirectory(searchFor);
            var entries = new GlobbingEnumerable(directory, pattern);
            Assert.Single(entries);
        }

        [TheoryWindowsOnly]
        // Absolute rooted directory and relative pattern.
        [InlineData("globtest", "", "glob*est")]
        [InlineData("globtest/foo.txt", "", "glob*est/*.*")]
        // Absolute non-rooted directory and relative pattern.
        [InlineData("globtest/foo", "globtest", "*")]
        [InlineData("globtest/foo", "globtest", "foo")]
        [InlineData("globtest/foo/", "globtest/", "foo")]
        [InlineData("globtest/foo/bar", "globtest", "f*o/b*r")]
        [InlineData("globtest/foo/bar", "globtest", "*/bar")]
        [InlineData("globtest/foo/bar", "globtest", "foo/bar")]
        [InlineData("globtest/foo/bar/", "globtest/", "foo/bar")]
        public void TestMatch_AbsoluteDirectory_RelativePattern(string searchFor, string directory, string pattern)
        {
            searchFor = s_root + searchFor;
            directory = s_root + directory;

            using var file = new TempDirectory(searchFor);
            var entries = new GlobbingEnumerable(directory, pattern);
            Assert.Single(entries);
        }

        [Theory]
        [InlineData("./globtest", "./", "globt*")]
        [InlineData("./globtest/foo", "/", "*/foo")]
        public void TestMatch_RelativeDirectory_AbsolutePattern(string searchFor, string directory, string pattern)
        {
            using var file = new TempDirectory(searchFor);

            string absolutePattern = Environment.CurrentDirectory + Path.DirectorySeparatorChar + pattern;
            var entries = new GlobbingEnumerable(directory, absolutePattern);
            Assert.Single(entries);
        }

        [Theory]
        [InlineData("./globtest", "./", "globtes*")]
        [InlineData("./globtest/foo", "./", "*/foo")]
        [InlineData("./globtest/foo", "./globtest", "f*o")]
        [InlineData("./globtest/foo/bar", "./globtest", "f*o/b*r")]
        [InlineData("./globtest/foo/bar", "./globtest/", "f*o/b*r")]
        [InlineData("./globtest/foo", "C:", "*/f*o")]
        public void TestMatch_RelativeDirectory_RelativePattern(string searchFor, string directory, string pattern)
        {
            using var file = new TempDirectory(searchFor);
            var entries = new GlobbingEnumerable(directory, pattern);
            Assert.Single(entries);
        }

        [FactWindowsOnly]
        public void TestMatch_UNCPath()
        {
            using var file = new TempDirectory("//wsl$/Ubuntu/home/jozky/globtest/foo");
            var entries = new GlobbingEnumerable("//wsl$/Ubuntu/home/jozky/globtest", "f*o");
            Assert.Single(entries);
        }

        [FactWindowsOnly(Skip = "Insufficient system resources exist to complete the requested service.")]
        public void TestMatch_UNCPath_FailingTest()
        {
            using var file = new TempDirectory("//wsl$/Ubuntu/home/jozky/globtest");
            var entries = new GlobbingEnumerable("//wsl$/Ubuntu/home/jozky", "globt*");
            Assert.Single(entries);
        }

        [FactWindowsOnly]
        public void TestMatch_FailingTest_Windows()
        {
            using var file = new TempDirectory("C:/repos/globtest");
            var entries = new GlobbingEnumerable("C:/repos", "globt*");
            Assert.Single(entries);
        }
    }
}
