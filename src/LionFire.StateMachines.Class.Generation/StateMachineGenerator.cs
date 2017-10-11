using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Validation;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Runtime.Loader;
using System.Diagnostics;
using LionFire.ExtensionMethods.CodeAnalysis;

namespace LionFire.StateMachines.Class.Generation
{
    public class StateMachineGenerator : ICodeGenerator
    {
        AttributeData attributeData;

        public StateMachineGenerator(AttributeData attributeData)
        {
            Requires.NotNull(attributeData, nameof(attributeData));
            this.attributeData = attributeData;


            AssemblyLoadContext.Default.Resolving += Default_Resolving;
        }

        private Assembly Default_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            Log("Resolving - " + arg2);
            return null;
        }


        public static HashSet<string> DefaultAfterPrefixes = new HashSet<string>()
            {
                "After"
            };

        public static HashSet<string> DefaultEnteringPrefixes = new HashSet<string>()
            {
                "On",
                "Enter",
                "OnEnter",
                "OnEntering",
            };
        public static HashSet<string> DefaultLeavingPrefixes = new HashSet<string>()
            {
                "Leave",
                "OnLeave",
                "OnLeaving",
            };

        public HashSet<string> AfterPrefixes
        {
            get => afterPrefixes ?? DefaultAfterPrefixes;
            set => afterPrefixes = value;
        }
        private HashSet<string> afterPrefixes;

        public HashSet<string> EnteringPrefixes
        {
            get => enteringPrefixes ?? DefaultEnteringPrefixes;
            set => enteringPrefixes = value;
        }
        private HashSet<string> enteringPrefixes;

        public HashSet<string> LeavingPrefixes
        {
            get => leavingPrefixes ?? DefaultLeavingPrefixes;
            set => leavingPrefixes = value;
        }
        private HashSet<string> leavingPrefixes;

    
        private List<string> logEntries = new List<string>();

        public const BindingFlags bf = BindingFlags.Static | BindingFlags.Public;
        const string unusedIndicator = " - ";
        const string usedIndicator = " * ";

        #region Log

        //[Conditional("DEBUG")]
        public void Log(string msg = null)
        {
            logEntries.Add(msg);
        }

        #endregion

        public async Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            if (context.ProcessingMember == null) throw new ArgumentNullException("context.ProcessingMember");
            var dClass = (ClassDeclarationSyntax)context.ProcessingMember;

            var typeInfo = context.SemanticModel.GetTypeInfo(dClass);
            //try
            //{
            //    foreach (var loc in
            //    context.Compilation.ScriptClass.Locations)
            //    {
            //        Log("Script class location: " + loc);
            //    }
            //}
            //catch { }

            ////Log("Location: " + Assembly.GetEntryAssembly().Location);
            Log("BaseDir: " + AppContext.BaseDirectory);
            Log("Compilation: ");
            Log(" - Source module name " + (context.Compilation.SourceModule.Name));
            Log(" - source module locations: " + (context.Compilation.SourceModule.Locations.Select(l => l.ToString()).Aggregate((x, y) => x + ", " + y)));

            foreach (var exref in context.Compilation.ExternalReferences)
            {
                //Log(" - External ref: " + exref.Display);
            }
            foreach (var r in context.Compilation.References)
            {
                if (r.Display.Contains(".nuget")) continue;
                Log("#r \"" + r.Display + "\"");

                assemblies.Add(AssemblyLoadContext.Default.LoadFromAssemblyPath(r.Display));
            }
            Log();

            return await Part2(context, progress, cancellationToken);
        }

        List<Assembly> assemblies = new System.Collections.Generic.List<Assembly>();

        int i = 0;

        //public string GetFullName(INamedTypeSymbol nts)
        //{
        //    return nts.GetFullMetadataName();
        //    //List<string> list = new System.Collections.Generic.List<string>();

        //    //list.Add(nts.Name);
        //    //ISymbol s = nts;
        //    //while (s.ContainingNamespace != null && !string.IsNullOrWhiteSpace(s.Name))
        //    //{
        //    //    s = s.ContainingNamespace;
        //    //    list.Add(s.Name);
        //    //}
        //    //list.Reverse();
        //    //return list.Aggregate((x, y) => x + "." + y).TrimStart('.');
        //}
        private Type ResolveType(TransformationContext context, object o)
        {
            var result = TryResolveType(context, o);
            if (result != null) return result;

            var combined = ((INamedTypeSymbol)o).GetFullMetadataName();
            foreach (var a in assemblies)
            {
                Log(" - looking in " + a.FullName);
            }
            throw new Exception("Failed to resolve " + combined);
        }
        private Type TryResolveType(TransformationContext context, object o)
        {
            var combined = ((INamedTypeSymbol)o).GetFullMetadataName();

            var type = Type.GetType(combined);

            if (type == null)
            {
                foreach (var a in assemblies)
                {
                    type = a.GetType(combined);

                    if (type != null) break;
                    else
                    {
                        if (a.FullName.StartsWith("LionFire.Execution.Abstractions"))
                        {
                            foreach (var t in a.GetTypes())
                            {
                                Log(".. " + t.FullName);
                            }
                        }
                    }
                }
            }

            if (type != null) Log("Resolved " + type.FullName);
            else
            {
                Log("Failed to resolve " + combined);

                //type = context.Compilation.References
                //    .Select(context.Compilation.GetAssemblyOrModuleSymbol)
                //    .OfType<IAssemblySymbol>()
                //    .Select(assemblySymbol => assemblySymbol.GetTypeByMetadataName(combined));

                //if (type != null)
                //{
                //    Log("resolved using 2nd approach" + combined);
                //}
            }


            return type;
            //if (i++ == 0) return stateMachineAttribute.StateType;
            //else return stateMachineAttribute.TransitionType;

            /*  REVIEW -- See this for other options  https://github.com/dotnet/roslyn/issues/3864
              
              @robintheilade The GetTypeByMetadataName API exists on both the Compilation and the IAssemblySymbol types. Therefore, if you have a Compilation, you can get the interesting IAssemblySymbols from that, and then call IAssemblySymbol.GetTypeByMetadataName.

            compilation.References
            .Select(compilation.GetAssemblyOrModuleSymbol)
            .OfType<IAssemblySymbol>()
            .Select(assemblySymbol => assemblySymbol.GetTypeByMetadataName(typeMetadataName))
            Depending on your use case, you might also want to include Compilation.Assembly in the list of searched IAssemblySymbols.


                */
        }
        private Task<SyntaxList<MemberDeclarationSyntax>> Part2(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var dClass = (ClassDeclarationSyntax)context.ProcessingMember;

            StateMachineAttribute stateMachineAttribute;

            if (attributeData.ConstructorArguments.Any())
            {
                if (attributeData.ConstructorArguments[0].Value is int flagsInt)
                {
                    stateMachineAttribute = (StateMachineAttribute)Activator.CreateInstance(typeof(StateMachineAttribute), (GenerateStateMachineFlags)flagsInt);
                }
                else
                {
                    stateMachineAttribute = (StateMachineAttribute)Activator.CreateInstance(typeof(StateMachineAttribute),
                        ResolveType(context, attributeData.ConstructorArguments[0].Value),
                        ResolveType(context, attributeData.ConstructorArguments[1].Value),
                        (GenerateStateMachineFlags)attributeData.ConstructorArguments[2].Value
                        );
                }
            }
            else
            {
                stateMachineAttribute = (StateMachineAttribute)Activator.CreateInstance(typeof(StateMachineAttribute));
            }

            Log();
            Log("StateMachine: ");
            Log(" - StateType: " + stateMachineAttribute.StateType.FullName);
            Log(" - TransitionType: " + stateMachineAttribute.TransitionType.FullName);
            Log(" - Options: " + stateMachineAttribute.Options.ToString());
            Log();
            foreach (var na in attributeData.NamedArguments)
            {
                var pi = typeof(StateMachineAttribute).GetProperty(na.Key);
                pi.SetValue(stateMachineAttribute, na.Value.Value);
            }

            if (stateMachineAttribute.Options.HasFlag(GenerateStateMachineFlags.DisableGeneration)) { return Task.FromResult(new SyntaxList<MemberDeclarationSyntax>()); }

            Type stateType = stateMachineAttribute.StateType;
            Type transitionType = stateMachineAttribute.TransitionType;

            var allStates = new HashSet<string>(stateType.GetFields(bf).Select(fi => fi.Name));
            var allTransitions = new HashSet<string>(transitionType.GetFields(bf).Select(fi => fi.Name));
            HashSet<string> usedStates;
            HashSet<string> usedTransitions;

            if (stateMachineAttribute.Options.HasFlag(GenerateStateMachineFlags.DisablePruneUnusedTransitions))
            {
                usedTransitions = allTransitions;
            }
            else
            {
                usedTransitions = new HashSet<string>();
                foreach (var md in dClass.Members.OfType<MethodDeclarationSyntax>())
                {
                    foreach (var transition in allTransitions)
                    {
                        if (md.Identifier.Text.EndsWith(transition))
                        {
                            var prefix = md.Identifier.Text.Substring(0, md.Identifier.Text.Length - transition.Length);

                            if (!usedTransitions.Contains(transition))
                            {
                                //Log(" - method " + md.Identifier.Text + $"() for {transition}");
                                usedTransitions.Add(transition);
                            }
                        }
                    }
                }
            }

            if (stateMachineAttribute.Options.HasFlag(GenerateStateMachineFlags.DisablePruneUnusedStates))
            {
                usedStates = allStates;
            }
            else
            {
                usedStates = new HashSet<string>();
                foreach (var md in dClass.Members.OfType<MethodDeclarationSyntax>())
                {
                    foreach (var state in allStates)
                    {
                        if (md.Identifier.Text.EndsWith(state))
                        {
                            var prefix = md.Identifier.Text.Substring(0, md.Identifier.Text.Length - state.Length);

                            if (!usedStates.Contains(state))
                            {
                                //Log(" - method " + md.Identifier.Text + $"() for state '{state}'");
                                usedStates.Add(state);
                            }
                        }
                    }
                }
                foreach (var transition in usedTransitions)
                {
                    var info = StateMachine.GetTransitionInfo(stateType, transitionType, Enum.Parse(stateMachineAttribute.TransitionType, transition));

                    if (info != null)
                    {
                        if (!usedStates.Contains(info.From.ToString())) usedStates.Add(info.From.ToString());
                        if (!usedStates.Contains(info.To.ToString())) usedStates.Add(info.To.ToString());
                    }
                }
            }

            Log("States:");

            foreach (var n in stateMachineAttribute.StateType.GetFields(bf).Select(fi => fi.Name))
            {
                Log((usedStates.Contains(n) ? usedIndicator : unusedIndicator) + n);

            }
            Log();

            Log("Transitions:");
            foreach (var n in stateMachineAttribute.TransitionType.GetFields(bf).Select(fi => fi.Name))
            {
                Log((usedTransitions.Contains(n) ? usedIndicator : unusedIndicator) + n);
            }
            Log();

            //CompilationUnitSyntax cu = ClassDeclaration()
            //       .AddUsings(UsingDirective(IdentifierName("System")))
            //       .AddUsings(UsingDirective(IdentifierName("LionFire.StateMachines.Class")))
            //    ;

            ClassDeclarationSyntax c = ClassDeclaration(dClass.Identifier).AddModifiers(Token(SyntaxKind.PartialKeyword));

            //foreach (var used in usedStates)
            //{
            //    var m = MethodDeclaration(SyntaxFactory.ParseTypeName("void"), used);
            //    //m = m.WithBody((BlockSyntax)BlockSyntax.DeserializeFrom(new MemoryStream(UTF8Encoding.UTF8.GetBytes("{}"))));
            //    var block = Block();
            //    m = m.WithBody(block);
            //    c = c.AddMembers(m);
            //}
            foreach (var used in usedTransitions)
            {
                var md = MethodDeclaration(
                            PredefinedType(
                                Token(SyntaxKind.VoidKeyword)),
                            Identifier(used))
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword)))
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                        InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName(stateMachineAttribute.StateMachineStatePropertyName),
                                                IdentifierName(nameof(StateMachineState<object, object, object>.ChangeState))))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList<ArgumentSyntax>(
                                                    Argument(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName(transitionType.Name),
                                                            IdentifierName(used))))))))
                                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                ;

                c = c.AddMembers(md);
            }
            c = AddStateMachineProperty(c, stateMachineAttribute);

            //foreach (var used in usedStates)
            //{
            //    var md = MethodDeclaration(
            //                PredefinedType(
            //                    Token(SyntaxKind.VoidKeyword)),
            //                Identifier(used))
            //            .WithModifiers(
            //                TokenList(
            //                    Token(SyntaxKind.PublicKeyword)))
            //            .WithExpressionBody(
            //                ArrowExpressionClause(
            //                            //SingletonList<StatementSyntax>(
            //                            //    ExpressionStatement(
            //                            InvocationExpression(
            //                                MemberAccessExpression(
            //                                    SyntaxKind.SimpleMemberAccessExpression,
            //                                    IdentifierName("stateMachine"),
            //                                    IdentifierName("ChangeState")))
            //                            .WithArgumentList(
            //                                ArgumentList(
            //                                    SingletonSeparatedList<ArgumentSyntax>(
            //                                        Argument(
            //                                            MemberAccessExpression(
            //                                                SyntaxKind.SimpleMemberAccessExpression,
            //                                                IdentifierName(transitionType.Name),
            //                                                IdentifierName(used))))))))
            //                                                //))
            //                                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            //    ;
            //    c = c.AddMembers(md);

            //}
            //NamespaceDeclarationSyntax ns = NamespaceDeclaration(IdentifierName(typeInfo.Type.ContainingNamespace.Name));
            //cu = cu.AddMembers(ns);

            //ClassDeclarationSyntax c = ClassDeclaration(typeInfo.Type.Name)
            //    //.AddModifiers(Token(SyntaxKind.PrivateKeyword))
            //    .AddModifiers(Token(SyntaxKind.PartialKeyword))
            //    ;
            //ns = ns.AddMembers(c);

            c = AddLog(c);

            var results = SyntaxFactory.List<MemberDeclarationSyntax>();
            results = results.Add(c);
            return Task.FromResult(results);
        }
        private ClassDeclarationSyntax AddStateMachineProperty(ClassDeclarationSyntax c, StateMachineAttribute attr)
        {
            string StateMachineStatePropertyName = "StateMachine";
            string StateMachineStateFieldName = "stateMachine";

            string typeName = typeof(StateMachineState<,,>).Name;
            typeName = typeName.Substring(0, typeName.LastIndexOf('`'));

            return c.AddMembers(
               new MemberDeclarationSyntax[]{
                    PropertyDeclaration(
                        GenericName(Identifier(typeName))

                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SeparatedList<TypeSyntax>(
                                    new SyntaxNodeOrToken[]{
                                        IdentifierName(attr.StateType.FullName),
                                        Token(SyntaxKind.CommaToken),
                                        IdentifierName(attr.TransitionType.FullName),
                                        Token(SyntaxKind.CommaToken),
                                        IdentifierName(c.Identifier)})))
                                ,

                        Identifier(StateMachineStatePropertyName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(
                        AccessorList(
                            SingletonList<AccessorDeclarationSyntax>(
                                AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration)
                                .WithBody(
                                    Block(
                                        IfStatement(
                                            BinaryExpression(
                                                SyntaxKind.EqualsExpression,
                                                IdentifierName("stateMachine"),
                                                LiteralExpression(
                                                    SyntaxKind.NullLiteralExpression)),
                                            Block(
                                                SingletonList<StatementSyntax>(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            IdentifierName("stateMachine"),
                                                            InvocationExpression(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    GenericName(
                                                                        Identifier("StateMachine"))
                                                                    .WithTypeArgumentList(
                                                                        TypeArgumentList(
                                                                            SeparatedList<TypeSyntax>(
                                                                                new SyntaxNodeOrToken[]{
                                                                                    IdentifierName("TS"), // TODO
                                                                                    Token(SyntaxKind.CommaToken),
                                                                                    IdentifierName("ExecutionTransition")}))),// TODO
                                                                    IdentifierName("CreateState")))
                                                            .WithArgumentList(
                                                                ArgumentList(
                                                                    SingletonSeparatedList<ArgumentSyntax>(
                                                                        Argument(
                                                                            ThisExpression()))))))))),
                                        ReturnStatement(
                                            IdentifierName(StateMachineStateFieldName))))))),
                    FieldDeclaration(
                        VariableDeclaration(
                            GenericName(
                                Identifier("StateMachineState"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]{
                                            IdentifierName("TS"),// TODO
                                            Token(SyntaxKind.CommaToken),
                                            IdentifierName("ExecutionTransition"),// TODO
                                            Token(SyntaxKind.CommaToken),
                                            IdentifierName("GeneratedExecutable")}))))// TODO
                        .WithVariables(
                            SingletonSeparatedList<VariableDeclaratorSyntax>(
                                VariableDeclarator(
                                    Identifier("stateMachine")))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PrivateKeyword)))});
        }

        private ClassDeclarationSyntax AddLog(ClassDeclarationSyntax c)
        {
            var prefix = "// ";
            var output = new SyntaxTrivia[] {
                Comment(prefix),
                Comment(prefix + " Generated on " + DateTime.Now.ToString()),
                Comment(prefix)
            }
                    .Concat(logEntries.Select(m => Comment(prefix + (m ?? ""))))
                    .Concat(new SyntaxTrivia[] { Comment(prefix) })
                    .ToArray();

            return c.WithCloseBraceToken(
            Token(TriviaList(output),
                SyntaxKind.CloseBraceToken,
                TriviaList()));
        }

    }
}
