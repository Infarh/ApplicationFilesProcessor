
using System.Globalization;
using System.IO.Compression;

const string data_path = "data";
const string result_path = "result";

if(Directory.Exists(result_path))
    Directory.Delete(result_path, true);
Directory.CreateDirectory(result_path);

var data_directory = new DirectoryInfo(data_path);

// using var zip_stream = File.Create("data.zip");
// using var zip = new ZipArchive(zip_stream, ZipArchiveMode.Create, true);

using var zip = new ZipArchive(File.Create("data.zip"), ZipArchiveMode.Create, false);

foreach(var src_file in data_directory.EnumerateFiles("*parameters.txt"))
{
    var prefix = src_file.Name[..src_file.Name.IndexOf('_')];

    Console.WriteLine(prefix);

    var options = src_file.EnumerateLines()
        .Select(line => line.Split('='))
        .Where(arr => arr.Length == 2)
        .ToDictionary(arr => arr[0], arr => arr[1]);

    // var f0_str = options["f0"];
    // var f0 = double.Parse(f0_str, CultureInfo.InvariantCulture);

    var x0_str = options["x0"];
    var x0 = double.Parse(x0_str, CultureInfo.InvariantCulture);

    foreach(var data_file in data_directory.EnumerateFiles($"{prefix}*Pattern.txt"))
    {
        var f0_str = data_file.Name[(data_file.Name.IndexOf('=') + 1)..data_file.Name.IndexOf(')')];
        var f0 = double.Parse(f0_str, CultureInfo.InvariantCulture);

        var file_name = FormattableString.Invariant($"Beam({f0 * 1000:f0}M)[x{x0:f2}y0z{x0 * 0.1:f3}].txt");

        data_file.CopyTo(Path.Combine(result_path, file_name), true);

        
        var entry = zip.CreateEntry(Path.Combine("data", file_name), CompressionLevel.SmallestSize);

        using var entry_stream = entry.Open();
        using var src_stream =  data_file.OpenRead();

        src_stream.CopyTo(entry_stream);

        Console.WriteLine($"\t{file_name}");
    }
}

File.Delete("result.zip");
ZipFile.CreateFromDirectory(result_path, "result.zip", CompressionLevel.SmallestSize, true);

Console.WriteLine("End.");


static class Ext
{
    public static IEnumerable<string> EnumerateLines(this FileInfo file)
    {
        using var reader = file.OpenText();
        while(!reader.EndOfStream)
            yield return reader.ReadLine()!;
    }
}