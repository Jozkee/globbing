using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Globbing.Tests
{
    public class GlobbingTests
    {
        [Theory]
        // Absolute directory and absolute pattern.
        [InlineData("C:/globtest", "C:/", "C:/globtest")]
        [InlineData("C:/globtest", "C:/", "C:/globte*t")]
        [InlineData("C:/globtest/foo", "C:/", "C:/globtest/*")]
        [InlineData("C:/globtest/foo.txt", "C:/", "C:/globtest/*.*")]
        // Absolute directory and relative pattern.
        [InlineData("C:/globtest", "C:/", "glob*est")]
        [InlineData("C:/globtest/foo.txt", "C:/", "glob*est/*.*")]
        // Absolute non-rooted directory and relative pattern.
        [InlineData("C:/globtest/foo", "C:/globtest", "*")]
        [InlineData("C:/globtest/foo", "C:/globtest", "foo")]
        [InlineData("C:/globtest/foo/", "C:/globtest/", "foo")]
        [InlineData("C:/globtest/foo/bar", "C:/globtest", "f*o/b*r")]
        [InlineData("C:/globtest/foo/bar", "C:/globtest", "*/bar")]
        [InlineData("C:/globtest/foo/bar", "C:/globtest", "foo/bar")]
        [InlineData("C:/globtest/foo/bar/", "C:/globtest/", "foo/bar")]
        // Relative directory and relative pattern.
        [InlineData("./globtest", "./", "globtes*")]
        [InlineData("./globtest/foo", "./", "*/foo")]
        [InlineData("./globtest/foo", "./globtest", "f*o")]
        [InlineData("./globtest/foo/bar", "./globtest", "f*o/b*r")]
        [InlineData("./globtest/foo/bar", "./globtest/", "f*o/b*r")]
        [InlineData("./globtest/foo", "C:", "*/f*o")]
        public void TestMatch(string searchFor, string directory, string pattern)
        {
            // Create a folder.
            using var file = new TempDirectory(searchFor);
            // Create the collection expecting it to only contain the created folder.
            var entries = new GlobbingEnumerable(directory, pattern);
            Assert.Single(entries);
        }

        [Theory]
        [InlineData("./globtest", "./", "*/foo")]
        [InlineData("./globtest/foo", "/", "globt*")]
        public void TestMatchRelativeDirectoryAndAbsolutePattern(string searchFor, string directory, string pattern)
        {

        }
    }
}
