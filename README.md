## GlobbingEnumerable 
Simple class prototype that extends FileSystemEnumerable and uses [DotNet.Glob](https://github.com/dazinator/DotNet.Glob) to match files with glob patterns.

https://github.com/Jozkee/globbing/blob/master/src/GlobbingEnumerable.cs

## Sample scenario

Given the following directory tree.

```
.
├── bar
│   ├── Program.cs
│   └── Project.csproj
├── foo
│   ├── Class1.cs
│   ├── a.txt
│   └── b.txt
└── qux
    └── Class2.cs
```

```cs
var entries = new GlobbingEnumerable(directory: ".", pattern: "**/*.cs");

Console.WriteLine("Files found:");

foreach (string entry in entries)
    Console.WriteLine(entry);
```

Result:
```
Files found:
/bar/Program.cs
/foo/Class1.cs
/qux/Class2.cs
```
