namespace PCLMock.CodeGeneration.Plugins
{
    using System;
    using System.Net.Http;
    using Microsoft.CodeAnalysis;

    public sealed class Http : IPlugin
    {
        private static readonly Type logSource = typeof(Http);

        public string Name => "Http";

        /// <inheritdoc />
        public Compilation InitializeCompilation(Compilation compilation) =>
            compilation
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(HttpResponseMessage).Assembly.Location));

        /// <inheritdoc />
        public SyntaxNode GetDefaultValueSyntax(
            Context context,
            MockBehavior behavior,
            ISymbol symbol,
            INamedTypeSymbol returnType)
        {
            if (behavior == MockBehavior.Loose)
            {
                return null;
            }

            return null;
        }
    }
}