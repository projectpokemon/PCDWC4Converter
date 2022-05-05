using PCDWC4Converter;

Console.WriteLine("PCDWC4Converter v1.1 by Sabresite");

if (args.Length == 0)
{
    Console.WriteLine("Usage: ./pcdwc4converter <path-to-folder-or-file>");
    return;
}

var path = args[0];
var fileAttr = File.GetAttributes(path);

var extensions = new [] { ".pcd", ".wc4" };
var files = Array.Empty<FileInfo>();

if ((fileAttr & FileAttributes.Directory) != 0)
{
    files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
        .Select(file => new FileInfo(file))
        .Where(file => extensions.Contains(file.Extension) && file.Length == 856)
        .ToArray();
}
else if (File.Exists(path))
{
    var file = new FileInfo(path);
    if (extensions.Contains(file.Extension) && file.Length == 856)
    {
        files = new[] { file };
    }
}

if (files.Length == 0)
{
    Console.WriteLine("No files found to convert.");
    return;
}

Console.WriteLine($"Found {files.Length} files to convert...");

Parallel.ForEach(files, WC4Data.ProcessWC4File);

Console.WriteLine("Conversion complete.");
