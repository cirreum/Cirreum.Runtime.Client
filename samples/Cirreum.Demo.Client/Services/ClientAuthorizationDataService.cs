namespace Cirreum.Demo.Client.Services;

using Cirreum.Authorization;
using Cirreum.Authorization.Analysis;
using Cirreum.Authorization.Documentation.Formatters;
using Cirreum.Authorization.Modeling;
using Cirreum.Authorization.Modeling.Export;

/// <summary>
/// Client-side implementation that performs live analysis using AuthorizationRuleProvider.
/// Use this for Blazor WASM or when running analyzers directly in the client.
/// </summary>
public class ClientAuthorizationDataService(
	IAuthorizationRoleRegistry roleRegistry,
	IServiceProvider serviceProvider
) : IAuthorizationDataService {

	private AnalysisReport? _cachedAnalysisReport;

	public async Task<AnalysisReport> GetAnalysisReportAsync(int maxRoleDepth) {
		if (this._cachedAnalysisReport == null) {
			await this.RefreshAsync(maxRoleDepth);
		}
		return this._cachedAnalysisReport!;
	}

	public async Task<AnalysisSummary> GetAnalysisSummaryAsync(int maxRoleDepth) {
		var report = await this.GetAnalysisReportAsync(maxRoleDepth);
		return report.GetSummary();
	}

	public Task<DomainCatalog> GetCatalogAsync() {
		return Task.FromResult(AuthorizationModel.Instance.GetCatalog());
	}

	public Task<IEnumerable<Role>> GetRolesAsync() {
		return Task.FromResult<IEnumerable<Role>>(roleRegistry.GetRegisteredRoles());
	}

	public Task<RoleHierarchyInfo> GetRoleHierarchyInfoAsync(Role role) {
		var childRoles = roleRegistry.GetInheritedRoles(role);
		var parentRoles = roleRegistry.GetInheritingRoles(role);
		var hierarchyDepth = GetHierarchyDepth(role, roleRegistry, []);

		return Task.FromResult(new RoleHierarchyInfo(
			role,
			[.. childRoles],
			[.. parentRoles],
			childRoles.Count,
			parentRoles.Count,
			hierarchyDepth
		));
	}

	public Task<string> GetAuthorizationFlowDiagramAsync() {
		return Task.FromResult(@"
			flowchart TD
				A[Request] --> B{Authenticated?}
				B -->|No| C[UnauthenticatedAccessException]
				B -->|Yes| D[Get User Roles]
				D --> E[Resolve Effective Roles<br/>via Inheritance]
				E --> F[Create Authorization Context]
				F --> G{Resource Validators?}
				G -->|Yes| H[Run Resource Validators]
				G -->|No| I{Policy Validators?}
				H --> I
				I -->|Yes| J[Run Policy Validators<br/>in Order]
				I -->|No| K{Any Protection?}
				J --> L{All Pass?}
				K -->|No| M[⚠️ Unprotected Resource]
				K -->|Yes| L
				L -->|No| N[ForbiddenAccessException]
				L -->|Yes| O[✅ Access Granted]
				M --> O
		");
	}

	public Task<string> GetRoleHierarchyDiagramAsync() {
		return Task.FromResult(RoleHierarchyRenderer.ToMermaidDiagram(roleRegistry));
	}

	public async Task RefreshAsync(int maxRoleDepth) {

		// Initialize provider with services and clears the internal cache
		AuthorizationModel.Instance.Initialize(serviceProvider);

		// Run analysis using the composite analyzer
		var analysisOptions = new AnalysisOptions {
			MaxHierarchyDepth = 10,
			IncludeInfoIssues = true,
			ExcludedCategories = []
		};

		var analyzer = DefaultAnalyzerProvider.CreateAnalyzer(roleRegistry, serviceProvider, analysisOptions);
		this._cachedAnalysisReport = await analyzer.AnalyzeAllAsync();
	}

	private static int GetHierarchyDepth(Role role, IAuthorizationRoleRegistry registry, HashSet<Role> visited) {
		if (visited.Contains(role)) {
			return 0;
		}

		visited.Add(role);

		var inheritedRoles = registry.GetInheritedRoles(role);
		if (!inheritedRoles.Any()) {
			return 0;
		}

		return inheritedRoles.Max(inherited => GetHierarchyDepth(inherited, registry, [.. visited])) + 1;
	}
}
