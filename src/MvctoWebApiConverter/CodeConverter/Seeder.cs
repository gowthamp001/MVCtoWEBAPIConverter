using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter
{
   public class Seeder
    {
        public static void BeginSeeding(string inputPath, string outputPath) 
        {
            Constants constants = new Constants();
            if (constants.MoveAllorCopySelected.Contains(constants.MoveAll))
            {
                CopyAllExceptSelected(inputPath, outputPath);

            }
            else
            {

                MoveSelectedFolders(inputPath, outputPath);
            }
        }

        public static void CopyAllExceptSelected(string sourceDirectory, string destinationDirectory)
        {
            Constants constants = new Constants();
            string foldersToMove = constants.filestoexclude;
            string[] foldersToExclude = foldersToMove.Split(',');
            CopyDirectory(sourceDirectory, destinationDirectory, foldersToExclude);
            Console.WriteLine("All files and folders copied, excluding specified ones.");
        }
        public static void MoveSelectedFolders(string sourceDirectory, string destinationDirectory)
        {
            Constants constants = new Constants();
            string[] directories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);
            string Folderstomove = constants.Folderstomove;
            string[] Listoffolders = Folderstomove.Split(',');
            foreach (string directory in directories)
            {
                foreach (string folderName in Listoffolders)
                {
                    if (Path.GetFileName(directory).Contains(folderName.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            string relativePath = directory.Substring(sourceDirectory.Length + 1);
                            string destinationPath = Path.Combine(destinationDirectory, relativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                            CopyDirectory(directory, destinationPath);
                            Console.WriteLine($"Moved folder containing '{folderName}': {directory}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error moving folder: {directory}");
                            Console.WriteLine($"Error message: {ex.Message}");
                        }
                    }
                }
            }

        }
     

        static void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
                File.Copy(file, destFile);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string destDir = Path.Combine(targetDir, dirName);
                CopyDirectory(directory, destDir);
            }
        }


        static void CopyDirectory(string sourceDir, string targetDir, string[] foldersToExclude)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
            
                if (!ShouldExclude(fileName, foldersToExclude))
                {
                    File.Copy(file, destFile);
                   
                }
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string destDir = Path.Combine(targetDir, dirName);
             
                if (!ShouldExclude(dirName, foldersToExclude))
                {
                    CopyDirectory(directory, destDir, foldersToExclude);
                  
                }
            }
        }

        static bool ShouldExclude(string itemName, string[] foldersToExclude)
        {
            foreach (string folderName in foldersToExclude)
            {
                if (itemName.Contains(folderName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
