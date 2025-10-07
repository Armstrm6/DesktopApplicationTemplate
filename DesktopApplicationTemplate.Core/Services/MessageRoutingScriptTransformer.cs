using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DesktopApplicationTemplate.Core.Services;

/// <summary>
/// Rewrites script snippets to inline message routing tokens as literal values.
/// </summary>
public static class MessageRoutingScriptTransformer
{
    private static readonly CSharpParseOptions ScriptParseOptions = new(kind: SourceCodeKind.Script);

    /// <summary>
    /// Rewrites <c>{ServiceName}.LastInputMessage</c> and <c>{ServiceName}.LastOutputMessage</c> member access
    /// expressions into string literals backed by the routing service.
    /// </summary>
    /// <param name="script">The user provided script.</param>
    /// <param name="routingService">Routing service supplying the latest message snapshots.</param>
    /// <returns>The rewritten script text.</returns>
    public static string InjectRoutingLiterals(string? script, IMessageRoutingService routingService, string? referencingServiceName = null)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            return string.Empty;
        }

        if (routingService is null)
        {
            throw new ArgumentNullException(nameof(routingService));
        }

        var syntaxTree = CSharpSyntaxTree.ParseText(script, ScriptParseOptions);
        var rewriter = new RoutingTokenRewriter(routingService);
        var rewritten = rewriter.Visit(syntaxTree.GetRoot());
        if (!string.IsNullOrWhiteSpace(referencingServiceName))
        {
            routingService.SetReferences(referencingServiceName, rewriter.References);
        }
        return rewritten.ToFullString();
    }

    private sealed class RoutingTokenRewriter : CSharpSyntaxRewriter
    {
        private readonly IMessageRoutingService _routingService;
        private readonly List<MessageRoutingReference> _references = new();

        public RoutingTokenRewriter(IMessageRoutingService routingService)
        {
            _routingService = routingService;
        }

        public IReadOnlyList<MessageRoutingReference> References => _references;

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node is null)
            {
                return null;
            }

            var visited = (MemberAccessExpressionSyntax?)base.VisitMemberAccessExpression(node);
            if (visited?.Expression is not IdentifierNameSyntax identifier)
            {
                return visited;
            }

            if (!TryGetDirection(visited.Name.Identifier.ValueText, out var direction))
            {
                return visited;
            }

            _references.Add(new MessageRoutingReference(identifier.Identifier.ValueText, direction));
            if (!_routingService.TryGetMessage(identifier.Identifier.ValueText, direction, out var message) || message is null)
            {
                message = string.Empty;
            }

            return SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(message));
        }

        private static bool TryGetDirection(string identifier, out MessageRoutingDirection direction)
        {
            if (string.Equals(identifier, "LastInputMessage", StringComparison.Ordinal))
            {
                direction = MessageRoutingDirection.Input;
                return true;
            }

            if (string.Equals(identifier, "LastOutputMessage", StringComparison.Ordinal))
            {
                direction = MessageRoutingDirection.Output;
                return true;
            }

            direction = MessageRoutingDirection.Input;
            return false;
        }
    }
}
