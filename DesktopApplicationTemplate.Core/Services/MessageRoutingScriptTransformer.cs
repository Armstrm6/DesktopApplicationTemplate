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
    /// Rewrites <c>{ServiceName}.InputMessage</c> and <c>{ServiceName}.OutputMessage</c> member access expressions
    /// into string literals backed by the routing service. Legacy tokens using <c>LastInputMessage</c> and
    /// <c>LastOutputMessage</c> remain supported for backward compatibility.
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
            if (visited?.Expression is not IdentifierNameSyntax identifier ||
                visited.Name is not IdentifierNameSyntax attributeNode)
            {
                return visited;
            }

            var attributeToken = attributeNode.Identifier.ValueText;
            var normalizedAttribute = MessageRoutingAttributeHelper.Normalize(attributeToken, out var direction);

            _references.Add(new MessageRoutingReference(identifier.Identifier.ValueText, normalizedAttribute, direction));

            string? message;
            if (direction.HasValue)
            {
                if (!_routingService.TryGetMessage(identifier.Identifier.ValueText, direction.Value, out message) || message is null)
                {
                    message = string.Empty;
                }
            }
            else if (!_routingService.TryGetAttribute(identifier.Identifier.ValueText, normalizedAttribute, out message) || message is null)
            {
                message = string.Empty;
            }

            return SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(message));
        }
    }
}
