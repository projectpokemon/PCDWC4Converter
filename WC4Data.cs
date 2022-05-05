namespace PCDWC4Converter;

public static class WC4Data
{
    public static void ProcessWC4File(FileInfo file)
    {
        Span<byte> fileData = stackalloc byte[856];
        
        using (var fs = file.OpenRead())
        {
            var bytesRead = fs.Read(fileData);
            if (bytesRead != 856)
            {
                Console.WriteLine("Failed to read file {0}", file.FullName);
                return;
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
        try
        {
            File.Delete(newFilePath);
            using var writer = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            writer.Write(fileData);
            
            Console.WriteLine("- Created file {0}", newFilePath);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to write file {0}", newFilePath);
            Console.WriteLine(e);
        }
    }
}
