using DotNet.Globbing;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;

namespace Globbing
{
    public class GlobbingEnumerable : FileSystemEnumerable<string>
    {
        private static readonly char s_directorySeparator = Path.DirectorySeparatorChar;
        private readonly Glob _glob;
        private bool _patternIsRooted;

        // RecurseSubdirectories must be turned on to properly 
        // support patterns with globstar and/or with subdirectories.
        private static readonly EnumerationOptions s_recurseSubdirectoriesOptions 
            = new EnumerationOptions { RecurseSubdirectories = true };

        public GlobbingEnumerable(string directory, string pattern)
        : base(directory, Transform, GetOptions())
        {
            _patternIsRooted = Path.IsPathRooted(pattern);
            _glob = Glob.Parse(pattern);

            ShouldIncludePredicate = IsMatch;
        }

        private static string Transform(ref FileSystemEntry entry) => entry.ToFullPath();

        private static EnumerationOptions GetOptions()
        {
            // TODO: avoid using s_recurseSubdirectoriesOptions if the pattern is limited to one level i.e: doesn't require recursing subdirectories.
            //e.g: "foo", "./foo".
            return s_recurseSubdirectoriesOptions;
        }

        private bool IsMatch(ref FileSystemEntry entry)
        {
            return _patternIsRooted ?
                // if the pattern is rooted (e.g: C:/**/bar.cs), we have to match against entry's absolute path.
                IsMatchAbsolutePattern(ref entry) :
                // if the pattern is NOT rooted (e.g: **/foo.cs), we have to match against entry's relative path.
                IsMatchRelativePattern(ref entry);
        }

        private bool IsMatchAbsolutePattern(ref FileSystemEntry entry)
        {
            ReadOnlySpan<char> directory = entry.Directory;
            ReadOnlySpan<char> filename = entry.FileName;

            int directoryPathLength = directory.Length;

            // Unlike non-rooted phats, a rooted path may contain a trailing separator e.g: C:\
            // do not append a separator in that case.
            if (!directory.EndsWith(s_directorySeparator))
            {
                directoryPathLength++;
            }

            int absolutePathLength = directoryPathLength + filename.Length;

            // TODO: Consider using ArrayPool after the 1024 threshold.
            Span<char> entryFullPath = absolutePathLength > 1024 ? new char[absolutePathLength] : stackalloc char[absolutePathLength];

            directory.CopyTo(entryFullPath);
            entryFullPath[directory.Length] = s_directorySeparator;
            filename.CopyTo(entryFullPath.Slice(directoryPathLength));

            return _glob.IsMatch(entryFullPath);

        }

        private bool IsMatchRelativePattern(ref FileSystemEntry entry)
        {
            ReadOnlySpan<char> rootDirectory = entry.RootDirectory;

            // Sustract the root directory to get the directory's relative path from the specified directory in the ctor.
            int rootDirectoryLength = rootDirectory.Length;

            // For consistency:
            // If RootDirectory ends with a separator e.g: C:\, we decrease the lenght by one
            // since we want to keep a leading separator in directoryPathRelative.
            if (rootDirectory.EndsWith(s_directorySeparator))
            {
                rootDirectoryLength--;
            }

            ReadOnlySpan<char> directoryPathRelative = entry.Directory.Slice(rootDirectoryLength);

            // If length is 0, we are in a non-rooted root directory.
            // If length is 1, we ara in a rooted root directory and the separator in directory[0] is just a side-effect of normalizing the relative path in above step.
            // In either case, entry is in the root directory and no path needs to be appended.
            // Note: the "root directory" is the same as the directory specified in the ctor.
            if (directoryPathRelative.Length > 1)
            {
                Debug.Assert(directoryPathRelative[0] == s_directorySeparator);

                int relativePathLength = directoryPathRelative.Length + entry.FileName.Length;
                // TODO: Consider using ArrayPool after the 1024 threshold.
                Span<char> entryFullPathRelative = relativePathLength > 1024 ? new char[relativePathLength] : stackalloc char[relativePathLength];

                // Skip directory's leading separator.
                directoryPathRelative.Slice(1).CopyTo(entryFullPathRelative);

                // Append trailing separator before filename.
                entryFullPathRelative[directoryPathRelative.Length - 1] = s_directorySeparator;

                entry.FileName.CopyTo(entryFullPathRelative.Slice(directoryPathRelative.Length));

                return _glob.IsMatch(entryFullPathRelative);
            }
            else
            {
                return _glob.IsMatch(entry.FileName);
            }
        }
    }
}
