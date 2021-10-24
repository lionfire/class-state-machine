﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Validation;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Runtime.Loader;
using LionFire.ExtensionMethods.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace LionFire.StateMachines.Class.Generation
{
    // TIP: Use http://roslynquoter.azurewebsites.net/ for generating new code

    internal class MySyntaxReceiver : ISyntaxReceiver
    {
        public ClassDeclarationSyntax ClassToAugment { get; private set; }

        // StateMachineAttribute
        public AttributeSyntax Attribute { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is ClassDeclarationSyntax cds)
            {
                var attr = cds.AttributeLists.SelectMany(al => al.Attributes).Where(a =>
                {
                    var name = a.Name.NormalizeWhitespace().ToFullString();
                    return name == "StateMachine" || name == "StateMachineAttribute" || name.Contains("StateMachine");
                }).FirstOrDefault();

                if (attr != null)
                {
                    Attribute = attr;
                }
                ClassToAugment = cds;
            }
        }
    }

    [Generator]
    public class StateMachineGenerator : ISourceGenerator
    {

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = context.SyntaxReceiver as MySyntaxReceiver;

            if (syntaxReceiver?.ClassToAugment != null)
            {
                var prefix = SyntaxNodeHelper.GetPrefix(syntaxReceiver.ClassToAugment);

                var sourceBuilder = new StringBuilder(@$"using System;

namespace {prefix.TrimEnd('.')} 
{{
    public partial class {syntaxReceiver.ClassToAugment.Identifier} 
    {{
        public void SayHello() 
        {{
            Console.WriteLine(""Namespace: {prefix}"");
            Console.WriteLine(""The following syntax trees existed in the compilation that created this program:"");
");

                // using the context, get a list of syntax trees in the users compilation
                var syntaxTrees = context.Compilation.SyntaxTrees;

            // add the filepath of each tree to the class we're building
            foreach (SyntaxTree tree in syntaxTrees)
            {
                sourceBuilder.AppendLine($@"Console.WriteLine("" - {tree.FilePath.Replace("\\", "/")}"");");
            }
            // finish creating the source to inject
            sourceBuilder.Append($@"
        }}
    }}
}}
");
                context.AddSource($"{syntaxReceiver.ClassToAugment.Identifier}._StateMachine", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        // ----------

        #region Configuration

        #region Defaults

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

        #endregion

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

        #endregion


#if OBSOLETE
        AttributeData attributeData;

        public StateMachineGenerator(AttributeData attributeData)
        {
            Log("Created generator");
            Requires.NotNull(attributeData, nameof(attributeData));
            this.attributeData = attributeData;

            AssemblyLoadContext.Default.Resolving += Default_Resolving;
        }

        private Assembly Default_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            Log("Resolving - " + arg2);
            return null;
        }
#endif

        private List<string> logEntries = new List<string>();

        public const BindingFlags bf = BindingFlags.Static | BindingFlags.Public;
        const string unusedIndicator = " - ";
        const string usedIndicator = " * ";

        #region Log

        //[Conditional("DEBUG")]
        public void Log(string msg = null)
        {
            Console.WriteLine(msg);
            logEntries.Add(msg);
        }

        #endregion

#if TOPORT
        public void GenerateAsync(, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            SourceGeneratorContext context
            //SyntaxList<MemberDeclarationSyntax> result;
            if (context.ProcessingNode == null) throw new ArgumentNullException("context.ProcessingMember");
            var dClass = (ClassDeclarationSyntax)context.ProcessingNode;

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

                var logText = "#r \"" + r.Display + "\"";
                
                Assembly loadedAssembly = null;
                try
                {
                    loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(r.Display);
                    
                }
                catch (FileLoadException lfe) when (lfe.Message == "Assembly with same name is already loaded")
                {
                    alreadyLoadedAssemblies.Add(r.Display);
                    logText += " (Already loaded)";
                }
                catch (Exception ex)
                {
                    Log($"Failed to load assembly {r.Display}.  Exception: " + ex.ToString());
                    continue;
                }
                if(loadedAssembly != null) assemblies.Add(loadedAssembly);
                
                Log(logText);
            }
            Log();

            return await Part2(context, progress, cancellationToken);
        }

        List<Assembly> assemblies = new System.Collections.Generic.List<Assembly>();
        List<string> alreadyLoadedAssemblies = new System.Collections.Generic.List<string>();

        //int i = 0;

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
                    //{
                    //    if (a.FullName.StartsWith("LionFire.Execution.Abstractions"))
                    //    {
                    // Log("LionFire.Execution.Abstractions types:");
                    //        foreach (var t in a.GetTypes())
                    //        {
                    //            Log(".. " + t.FullName);
                    //        }
                    //    }
                    //}
                    if (type != null) break;
                }
            }

            if (type == null)
            {
                foreach (var a in alreadyLoadedAssemblies)
                {
                    var name = combined + ", " + Path.GetFileNameWithoutExtension(a) + ", Version=2.0.49.0, Culture=neutral, PublicKeyToken=null";
                    try
                    {
                        type = Type.GetType(name);
                    }
                    catch { }
                    Log("Test: " + this.GetType().AssemblyQualifiedName);
                    Log("Qualified: " +name);
                    if (type != null) break;
                }
            }

#if NETSTANDARD2_0
            if (type == null)
            {
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = a.GetType(combined);
                    if (type != null) break;
                    //else
                    //{
                    //    if (a.FullName.StartsWith("LionFire.Execution.Abstractions"))
                    //    {
                    //        foreach (var t in a.GetTypes())
                    //        {
                    //            Log(".. " + t.FullName);
                    //        }
                    //    }
                    //}
                }
            }
#endif

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
            ClassDeclarationSyntax c = null;
            try
            {
                var dClass = (ClassDeclarationSyntax)context.ProcessingNode;
                c = ClassDeclaration(dClass.Identifier).AddModifiers(Token(SyntaxKind.PartialKeyword));
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
                                                    IdentifierName(nameof(StateMachineState<object, object, object>.Transition))))
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

            }
            catch (Exception ex)
            {
                if (c != null)
                {
                    Log("#error Code generation resulted in an exception.  See code generation file for details.");
                    Log("Unhandled exception: " + ex.ToString().Replace(Environment.NewLine, Environment.NewLine + " // "));
                }
                else
                {
                    throw;
                }
            }

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
                                                                    IdentifierName("Create")))
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
               .Concat(logEntries.Select(m => (m??"").StartsWith("#error") 
            ? Trivia(ErrorDirectiveTrivia(true)
                .WithEndOfDirectiveToken(
                                Token(
                                    TriviaList(
                                        PreprocessingMessage(m.Substring("#error".Length))),
                                    SyntaxKind.EndOfDirectiveToken,
                                    TriviaList()))
                )

            : Comment(prefix + (m ?? "")))
                    //.Concat(logEntries.Select(m => Comment(prefix + (m ?? ""))))
                    .Concat(new SyntaxTrivia[] { Comment(prefix) })
                    .ToArray());

            return c.WithCloseBraceToken(
            Token(TriviaList(output),
                SyntaxKind.CloseBraceToken,
                TriviaList()));
        }
#endif

    }
}
