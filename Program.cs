using PCDWC4Converter;

Console.WriteLine("PCDWC4Converter v1.0 by Sabresite");

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

// All PCD data
Span<byte> fileData = stackalloc byte[856];

for (var i = 0; i < files.Length; i++)
{
    var file = files[i];
    using (var fs = file.OpenRead())
    {
        var bytesRead = fs.Read(fileData);
        if (bytesRead != 856)
        {
            Console.WriteLine("Failed to read file {0}", file.FullName);
            continue;
        }
    }

    var newExtension = "";

    switch (file.Extension)
    {
        case ".pcd":
            {
                PokemonData.DecryptData(fileData[8..]);
                newExtension = ".wc4";
                break;
            }
        case ".wc4":
            {
                PokemonData.EncryptData(fileData[8..]);
                newExtension = ".pcd";
                break;
            }
    }

    var newFilePath = Path.Join(file.DirectoryName, $"{file.Name[..^file.Extension.Length]}{newExtension}");
    Console.WriteLine("New File {0}", newFilePath);
    try
    {
        File.Delete(newFilePath);
        using var writer = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        writer.Write(fileData);
    }
    catch (Exception e)
    {
        Console.WriteLine("Failed to write file {0}", newFilePath);
        Console.WriteLine(e);
    }
}
