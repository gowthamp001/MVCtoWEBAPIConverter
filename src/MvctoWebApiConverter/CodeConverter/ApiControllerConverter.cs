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
    class ApiControllerConverter : CSharpSyntaxRewriter
    {
        Constants constants = new Constants();
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {

            bool inheritsFromBaseController = node.BaseList?.Types
              .Any(t => t.Type is IdentifierNameSyntax identifier &&
                        (identifier.Identifier.ValueText == "ControllerBase" ||
                         identifier.Identifier.ValueText == "Controller")) ?? false;

            // Change base class to ApiController  
            var baseListAPIController = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(constants.ApiController))));
            var baseListBaseController = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(constants.BaseController))));



            // Add using directive for System.Web.Http if not already present  
            var root = (CompilationUnitSyntax)node.SyntaxTree.GetRoot();
            if (!root.Usings.Any(u => u.Name.ToString() == constants.SystemWebHttp))
            {
                var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(constants.SystemWebHttp));
                root = root.AddUsings(newUsing);
            }
            ClassDeclarationSyntax? newClass;




            // Modify the class declaration  
            if (!inheritsFromBaseController)
            {
                newClass = node.WithIdentifier(SyntaxFactory.Identifier(node.Identifier.ValueText.Replace(constants.Controller, constants.Controller)))
                             .WithBaseList(baseListBaseController)
                             .WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>());
            }
            else
            {
                newClass = node.WithIdentifier(SyntaxFactory.Identifier(node.Identifier.ValueText.Replace(constants.Controller, constants.Controller)))
                              .WithBaseList(baseListAPIController)
                              .WithAttributeLists(SyntaxFactory.List<AttributeListSyntax>());
            }


            // Add RoutePrefix attribute  
            var routePrefix = SyntaxFactory.Attribute(SyntaxFactory.ParseName(constants.RoutePrefix),
                SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("api/" + node.Identifier.ValueText.Replace(constants.Controller, "").ToLower())))
                )));
            var routePrefixList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(routePrefix));
            newClass = newClass.AddAttributeLists(routePrefixList);

            return base.VisitClassDeclaration(newClass.WithAdditionalAnnotations(new SyntaxAnnotation(constants.ControllerRoot, root.ToFullString())));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // Replace ActionResult with IHttpActionResult  
            var newReturnType = SyntaxFactory.ParseTypeName(constants.IHttpActionResult);
            var newMethod = node.WithReturnType(newReturnType);



            // Add Route attribute based on the method name  
            var routeAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName(constants.Route),
                SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(node.Identifier.Text.ToLower())))
                )));
            var routeAttributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(routeAttribute));



            #region Http Attribrutes
            // Determine if the method HTTP Attribrute and Add the Route
            var isHttpPost = node.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains(constants.HttpPost));
            var isHttpPut = node.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains(constants.HttpPut));
            var HTTPDELETE = node.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains(constants.HttpDelete));
            var isHttpGet = node.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains(constants.HttpGet));
            var HttpAction = node.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains(constants.httpAction));
            if (isHttpPost || isHttpGet || isHttpPut || HTTPDELETE)
            {
                newMethod = newMethod.AddAttributeLists(routeAttributeList);
            }
            else
            {
                var httpGetAttribute = SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.ParseName(constants.HttpGet)
                        )
                    )
                );
                newMethod = newMethod.AddAttributeLists(routeAttributeList, httpGetAttribute);
            }
            #endregion

            #region Retun Statements
            // Determine and modify return statements of the methods
            newMethod = newMethod.ReplaceNodes(
            newMethod.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(invocation => invocation.Expression.ToString().StartsWith(constants.View)),
            (node, _) =>
            {
                var invocation = (InvocationExpressionSyntax)node;
                if (invocation.ArgumentList.Arguments.Count > 1)
                {
                    var argument = invocation.ArgumentList.Arguments.ElementAt(1);
                    return SyntaxFactory.ParseExpression($"Ok({argument})");

                }
                if (invocation.ArgumentList.Arguments.Count == 1)
                {

                    var argument = invocation.ArgumentList.Arguments.First().ToString().ToString();
                    return SyntaxFactory.ParseExpression($"Ok({argument})");
                }
                else
                {

                    return SyntaxFactory.ParseExpression(constants.MethodOk);
                }
            });

            newMethod = newMethod.ReplaceNodes(
          newMethod.DescendantNodes().OfType<InvocationExpressionSyntax>()
          .Where(invocation => invocation.Expression.ToString().StartsWith(constants.Json)),
          (node, _) =>
          {
              var invocation = (InvocationExpressionSyntax)node;

              if (invocation.ArgumentList.Arguments.Count > 1)
              {
                  var argument = invocation.ArgumentList.Arguments.First().ToString().ToString();
                  return SyntaxFactory.ParseExpression($"Ok({argument})");
              }
              else
              {
                  return SyntaxFactory.ParseExpression(constants.MethodOk);
              }
          });


            newMethod = newMethod.ReplaceNodes(
            newMethod.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(invocation => invocation.Expression.ToString().Contains(constants.Redirect)),
             (node, _) =>
             {
                 var invocation = (InvocationExpressionSyntax)node;

                 if (invocation.ArgumentList.Arguments.Count == 1)
                 {
                     var argument = invocation.ArgumentList.Arguments.First().ToString();
                     return SyntaxFactory.ParseExpression($"RedirectToRoute({argument})");
                 }
                 else if (invocation.ArgumentList.Arguments.Count > 1)
                 {
                     var argument1 = invocation.ArgumentList.Arguments.First().ToString();
                     var argument2 = invocation.ArgumentList.Arguments.ElementAt(1).ToString;
                     return SyntaxFactory.ParseExpression($"RedirectToRoute({argument1})");
                 }
                 else
                 {
                     return SyntaxFactory.ParseExpression(constants.RedirectToRoute);
                 }
             });


            newMethod = newMethod.ReplaceNodes(
              newMethod.DescendantNodes().OfType<InvocationExpressionSyntax>()
                  .Where(invocation => invocation.Expression.ToString() == constants.HttpNotFound),
              (_, __) => SyntaxFactory.ParseExpression(constants.NotFound));


            return newMethod;
            #endregion
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {

            var hasSystemWebHttpUsing = node.Usings.Any(u => u.Name.ToString() == constants.SystemWebHttp);
            if (!hasSystemWebHttpUsing)
            {
                var systemWebHttpUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(constants.SystemWebHttp));
                node = node.AddUsings(systemWebHttpUsing);
            }
            return base.VisitCompilationUnit(node);
        }



    }
}
