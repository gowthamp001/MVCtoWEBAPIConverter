using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeConverter
{
    public class SessionHandler
    {
        public static string? ExistingNamespace;
        public static string SessionConverter(string controllercode)
        {
            if (controllercode.Contains("Session"))
            {
                string pattern = @"Session\[""([^""]+)""\]\s*=\s*""([^""]+)"";";
                string replacement = @"_redisCacheService.Set(""$1""+ "","" + ""RequestToken"", $2, null);";
                controllercode =
                    Regex.Replace(controllercode, pattern, replacement);
                string getterpattern = @"(?<=\=|\b\S+\().*?\b(?:Http\.Current\.)?Session\[""(.*?)""\]";
                string getterreplacement = @"_redisCacheService.Get(""$1""+ "","" + ""RequestToken"")";
                controllercode = Regex.Replace(controllercode, getterpattern, getterreplacement);
                var Feildaddedcontrollercode = AddSessionMemeber(controllercode);
                var Sessionaddedcontrollercode = AddConstrcutor(Feildaddedcontrollercode);
                return Sessionaddedcontrollercode;
            }

            return controllercode;
        }


        public static string AddSessionMemeber(string controllercode)
        {
            Constants constants = new Constants();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(controllercode);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var fieldDeclaration = SyntaxFactory.FieldDeclaration(
               SyntaxFactory.VariableDeclaration(
                   SyntaxFactory.ParseTypeName(constants.RedisCacheService),
                   SyntaxFactory.SingletonSeparatedList(
                       SyntaxFactory.VariableDeclarator(
                           SyntaxFactory.Identifier(constants._redisCacheService)
                       )
                   )
               )
           )
           .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));

            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration != null)
            {
                classDeclaration = classDeclaration.WithBaseList(
                    SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(constants.ApiController))
                    ))
                );
                var firstMember = classDeclaration.Members.FirstOrDefault();
                if (firstMember != null)
                {
                    classDeclaration = classDeclaration.InsertNodesBefore(firstMember, new[] { fieldDeclaration });
                }
                else
                {

                    classDeclaration = classDeclaration.AddMembers(fieldDeclaration);
                }
                root = root.ReplaceNode(root.DescendantNodes().OfType<ClassDeclarationSyntax>().First(), classDeclaration);
            }
            string newRoot = root.NormalizeWhitespace().ToFullString();
            return newRoot;

        }

        public static string AddConstrcutor(string controllercode)
        {
            Constants constants = new Constants();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(controllercode);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var constructor = root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (constructor != null)
            {

                var assignmentStatement = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(constants._redisCacheService),
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.ParseTypeName(constants.RedisCacheService)
                        )
                        .WithArgumentList(SyntaxFactory.ArgumentList())
                    )
                );
                constructor = constructor.AddBodyStatements(assignmentStatement);
                root = root.ReplaceNode(root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault(), constructor);
            }
            string newRoot = root.NormalizeWhitespace().ToFullString();
            return newRoot;
        }




        public static void AddServiceClass()
        {
            Constants constants = new Constants();
            var ServiceClassString = constants.ServiceClassString;
            string destinationDirectory = constants.Destinationpath;
            string destinationServiceClassPath = Path.Combine(destinationDirectory, constants.RedisCacheServices, constants.RedisCacheServiceClass);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationServiceClassPath));
            File.WriteAllText(destinationServiceClassPath, ServiceClassString);

        }


      
        public static void UpdateNameSpaces(string projectDirectory, string projectName)
        {
            string[] csFiles = Directory.GetFiles(projectDirectory, "*.cs", SearchOption.AllDirectories);

            foreach (string file in csFiles)
            {
                string content = File.ReadAllText(file);
                content = Regex.Replace(content, @"namespace MvcSolution", $"namespace {projectName}");
                File.WriteAllText(file, content);
            }

        }


    }
}
