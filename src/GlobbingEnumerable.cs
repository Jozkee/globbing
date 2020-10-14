using DotNet.Globbing;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;

namespace Globbing
{
    public class GlobbingEnumerable : FileSystemEnumerable<string>
    {
        private readonly Glob _glob;
        private bool _patternIsRooted;

        // RecurseSubdirectories must be turned on to properly 
        // support patterns with globstar and/or with subdirectories.
        private static readonly EnumerationOptions s_options 
            = new EnumerationOptions { RecurseSubdirectories = true };

        public GlobbingEnumerable(string directory, string pattern)
        : base(directory, Transform, s_options)
        {
            _patternIsRooted = Path.IsPathRooted(pattern);
            _glob = Glob.Parse(pattern);

            ShouldIncludePredicate = IsMatchV2;
        }

        private static string Transform(ref FileSystemEntry entry) => entry.ToFullPath();

        private bool IsMatch(ref FileSystemEntry entry)
            {
            // To get the relative path that we will try to match with the glob pattern, 
            // we will use (Directory - RootDirectory) + Filename.
            ReadOnlySpan<char> pathMinusRoot = entry.Directory.Slice(entry.RootDirectory.Length);
            int pathMinusRootLength = pathMinusRoot.Length;

            // TODO: Use ArrayPool instead after a certain threshold
            // e.g: after 1024 chanracters.
            Span<char> fullPath = stackalloc char[pathMinusRootLength + entry.FileName.Length];

            if (pathMinusRootLength > 0)
            {
                // Skip '\\'.
                pathMinusRoot.Slice(1).CopyTo(fullPath);

                // Append trailing '\\' before filename.
                fullPath[pathMinusRootLength - 1] = '\\';
            }

            entry.FileName.CopyTo(fullPath.Slice(pathMinusRootLength));

            return _glob.IsMatch(fullPath);
        }

        private bool IsMatchV2(ref FileSystemEntry entry)
        {
            // if the pattern is rooted (e.g: C:/**/bar.cs), we have to match against entry's absolute path.
            if (_patternIsRooted)
            {
                ReadOnlySpan<char> directory = entry.Directory;
                ReadOnlySpan<char> filename = entry.FileName;

                int directoryLength = directory.Length;

                // Unlike non-rooted phats, 'C:\' already contains a trailing '\'.
                // Do not append '\' for that case.
                if (directory[directory.Length - 1] != '\\')
                {
                    directoryLength++;
                }

                int absolutePathLength = directoryLength + filename.Length;

                // TODO: Use ArrayPool instead after a certain threshold
                // e.g: after 1024 chanracters.
                Span<char> fullPathAbsolute = stackalloc char[absolutePathLength];

                directory.CopyTo(fullPathAbsolute);
                fullPathAbsolute[directory.Length] = '\\';
                filename.CopyTo(fullPathAbsolute.Slice(directoryLength));

                return _glob.IsMatch(fullPathAbsolute);
            }
            // if the pattern is NOT rooted (e.g: **/foo.cs), we have to match against entry's relative path.
            else
            {
                ReadOnlySpan<char> rootDirectory = entry.RootDirectory;
                int rootDirectoryLength = rootDirectory.Length;

                // Normalization:
                // If the root ends with '\', we won't count for that trailing '\' 
                // since we need one slot to append '\' between directory and filename.
                if (rootDirectory[rootDirectory.Length - 1] == '\\')
                {
                    rootDirectoryLength--;
                }

                // Remove the root directory to keep the relative path from the specified directory in the ctor.
                ReadOnlySpan<char> directory = entry.Directory.Slice(rootDirectoryLength);

                if (directory.Length > 1)
                {
                    Debug.Assert(directory[0] == '\\');

                    // TODO: Use ArrayPool instead after a certain threshold
                    // e.g: after 1024 chanracters.
                    Span<char> fullPathRelative = stackalloc char[directory.Length + entry.FileName.Length];

                    // Skip directory's leading '\\'.
                    directory.Slice(1).CopyTo(fullPathRelative);

                    // Append trailing '\\' before filename.
                    fullPathRelative[directory.Length - 1] = '\\';

                    entry.FileName.CopyTo(fullPathRelative.Slice(directory.Length));

                    return _glob.IsMatch(fullPathRelative);
                }
                // if directory.Length == 0, we are in a non-rooted root directory.
                // if directory.Length == 1, we ara in a rooted root directory and the '\' in directory[0] is just a side-effect of normalizing the way we get the relative path.
                // in either case, the entry is in the root directory and no path needs to be appended.
                // Note: the "root directory" is the same as the directory specified in the ctor.
                else
                {
                    return _glob.IsMatch(entry.FileName);
                }
            }
        } 
    }
}
