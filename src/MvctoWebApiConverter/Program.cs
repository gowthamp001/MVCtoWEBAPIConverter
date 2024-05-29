using CodeConverter;
using System.Configuration;

try
{
    Constants constants = new Constants();
    string inputPath = constants.SourcePath;
    string outputPath = constants.Destinationpath;
    var ExistingNamespace = string.Empty;

    if (Directory.Exists(inputPath))
    {
        string[] csproj = Directory.GetFiles(inputPath, "*.csproj");

        if (csproj.Length > 0)
        {
            foreach (string slnFile in csproj)
            {
                ExistingNamespace = Path.GetFileName(slnFile);
            }
            CodeConverter(inputPath, outputPath, ExistingNamespace);
        }
        else
        {
            Console.WriteLine("Selected folder does not contain any .csproj file.");
        }
    }
    else
    {
        Console.WriteLine("Invalid folder path or the path does not exist.");
    }
    Console.WriteLine("Conversion Completed..............!");
}
catch (Exception)
{

    throw;
}

void CodeConverter(string inputPath, string outputPath, string ExistingNamespace)
{
    Seeder.BeginSeeding(inputPath, outputPath);
    SessionHandler.ExistingNamespace = ExistingNamespace;
    ControllerConverter.ConvertControllers(inputPath, outputPath);

}
