// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

using MonoDevelop.MSBuild.Analysis;
using MonoDevelop.MSBuild.Language;
using MonoDevelop.MSBuild.Language.Expressions;

namespace MonoDevelop.MSBuild.Analyzers
{
	[MSBuildAnalyzer]
	class RuntimeIdentifierOrRuntimeIdentifiersAnalyzer : MSBuildAnalyzer
	{
		readonly MSBuildDiagnosticDescriptor UseRuntimeIdentifiersForMultipleRIDs = new MSBuildDiagnosticDescriptor (
			"UseRuntimeIdentifiersForMultipleRIDs",
			"Use RuntimeIdentifiers for multiple RIDs",
			"When targeting multiple RIDs, use the RuntimeIdentifiers property instead of RuntimeIdentifier",
			MSBuildDiagnosticSeverity.Error
		);

		readonly MSBuildDiagnosticDescriptor UseRuntimeIdentifierForSingleRID = new MSBuildDiagnosticDescriptor (
			"UseRuntimeIdentifierForSingleRID",
			"Use RuntimeIdentifier for single RID",
			"When targeting a single RID, use the RuntimeIdentifier property instead of RuntimeIdentifiers",
			MSBuildDiagnosticSeverity.Warning
		);

		public override ImmutableArray<MSBuildDiagnosticDescriptor> SupportedDiagnostics { get; }

		public RuntimeIdentifierOrRuntimeIdentifiersAnalyzer ()
		{
			SupportedDiagnostics = ImmutableArray.Create (
				UseRuntimeIdentifiersForMultipleRIDs, UseRuntimeIdentifierForSingleRID
			);
		}

		public override void Initialize (MSBuildAnalysisContext context)
		{
			context.RegisterPropertyWriteAction (AnalyzeRuntimeIdentifier, "RuntimeIdentifier");
			context.RegisterPropertyWriteAction (AnalyzeRuntimeIdentifiers, "RuntimeIdentifiers");
			context.RegisterCoreDiagnosticFilter (CoreDiagnosticFilter, CoreDiagnostics.UnexpectedList.Id);
		}

		void AnalyzeRuntimeIdentifier (PropertyWriteDiagnosticContext ctx)
		{
			// right now the visitor parses the expression with lists disabled because the type system says it doesn't expect lists, so we get a text node
			// however, once we attach parsed expressions into the AST they will likely have all options enabled and could be lists
			if (ctx.Node is ListExpression || (ctx.Node is ExpressionText t && t.Value.IndexOf (';') > -1)) {
				ctx.ReportDiagnostic (new MSBuildDiagnostic (UseRuntimeIdentifiersForMultipleRIDs, ctx.Element.Span));
			}
		}

		void AnalyzeRuntimeIdentifiers (PropertyWriteDiagnosticContext ctx)
		{
			if (ctx.Node is ExpressionText t && t.Value.IndexOf (';') < 0) {
				ctx.ReportDiagnostic (new MSBuildDiagnostic (UseRuntimeIdentifierForSingleRID, ctx.Element.Span));
			}
		}

		bool CoreDiagnosticFilter (MSBuildDiagnostic arg)
			=> arg.Properties != null && arg.Properties.TryGetValue ("Name", out var value) && (string)value == "RuntimeIdentifier";
	}
}
