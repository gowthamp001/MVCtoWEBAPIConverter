using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeConverter
{
     class BaseControllerConverter
    {
        public static string ConvertBaseController(string mmvcControllerCode)
        {
            try
            {
                Constants constants = new Constants();
                SyntaxTree tree = CSharpSyntaxTree.ParseText(mmvcControllerCode);
                var root = (CompilationUnitSyntax)tree.GetRoot();
                //Added this code for adding using -- Using Web.http
                var hasSystemWebHttpUsing = root.Usings.Any(u => u.Name.ToString() == constants.SystemWebHttp);
                if (!hasSystemWebHttpUsing)
                {
                    var systemWebHttpUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(constants.SystemWebHttp));
                    root = root.AddUsings(systemWebHttpUsing);
                }

                string? newroot = string.Empty;
                // Find class declarations
                var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (classDeclaration != null)
                {
                    // Modify the base class to ApiController
                    classDeclaration = classDeclaration.WithBaseList(
                        SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(constants.ApiController))
                        ))
                    );
                    // Replace the original class declaration with the modified one
                    root = root.ReplaceNode(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First(), classDeclaration);
                    newroot = (root).NormalizeWhitespace().ToFullString();
                }
                return newroot;

            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
