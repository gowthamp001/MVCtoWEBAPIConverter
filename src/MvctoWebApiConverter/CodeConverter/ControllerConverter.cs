using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeConverter
{
    public class ControllerConverter
    {
        public static Constants constants = new Constants();
       
       public static void ConvertControllers(string sourceDirectory, string destinationDirectory)
        {           
            string ControllerKey = constants.Controllerkey;
            string[] directories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);
            foreach (string directory in directories)
            {
                if (Path.GetFileName(directory).Contains(ControllerKey, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string relativePath = directory.Substring(sourceDirectory.Length + 1);
                        string destinationPath = Path.Combine(destinationDirectory, relativePath);
                        Directory.CreateDirectory($"{destinationPath}");
                        ConvertController(directory, destinationPath);
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine($"Error moving folder: {directory}");
                        Console.WriteLine($"Error message: {ex.Message}");
                    }
                }
            }
        }

        static void ConvertController(string directory, string destinationPath)
        {
            string[] files = Directory.GetFiles(directory);
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string mvcControllerCode = File.ReadAllText(filePath);
                var apiControllerCode = ConvertFolder(filePath, mvcControllerCode, fileName);
                string combinedPath = Path.Combine(destinationPath, fileName);

                File.WriteAllText(combinedPath, apiControllerCode);
            }
        }

        public static string ConvertFolder(string filePath, string mvcControllerCode, string filename)
        {
            try
            {
                Constants constants = new Constants();
                //Remove  Usings
                string RemoveHttpAction = mvcControllerCode.Replace(constants.HttpAction, string.Empty);
                string RemoveUsingWeb = RemoveHttpAction.Replace(constants.WebMvcUsing, string.Empty);
                string RemoveUsinganti = RemoveUsingWeb.Replace(constants.ValidateAntiForgeryToken, string.Empty);
                string apiControllerCode = string.Empty;
                if (filename.Contains(constants.BaseController))
                {
                    apiControllerCode = ConvertBaseController(RemoveUsinganti);

                }
                else
                {

                    apiControllerCode = ConvertToApiController(RemoveUsinganti);
                }
                var ControllerCode = SessionHandler.SessionConverter(apiControllerCode);

                return ControllerCode;
            }
            catch (Exception)
            {

                return "";
            }


        }


        static string ConvertToApiController(string mvcControllerCode)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(mvcControllerCode);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            Constants constants = new Constants();
            string? newRoot = string.Empty;
            var rewriter = new ApiControllerConverter();
            newRoot = rewriter.Visit(root).NormalizeWhitespace().ToFullString();
            return newRoot;
        }

        static string ConvertBaseController(string mmvcControllerCode) 
        {
            var newroot=   BaseControllerConverter.ConvertBaseController(mmvcControllerCode);
            return newroot;
        }


    }
}