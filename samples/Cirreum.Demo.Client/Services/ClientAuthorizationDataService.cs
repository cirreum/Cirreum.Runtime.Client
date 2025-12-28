namespace Cirreum.Demo.Client.Services;

using Cirreum.Authorization;
using Cirreum.Authorization.Analysis;
using Cirreum.Authorization.Documentation.Formatters;
using Cirreum.Authorization.Modeling;
using Cirreum.Authorization.Modeling.Export;

/// <summary>
/// Client-side implementation that performs live analysis using AuthorizationModel.
/// Use this for Blazor WASM or when running analyzers directly in the client.
/// </summary>
public class ClientAuthorizationDataService(
	IAuthorizationRoleRegistry roleRegistry,
	IServiceProvider serviceProvider
) : IAuthorizationDataService {

	private AnalysisReport? _cachedAnalysisReport;
	private IReadOnlyList<RoleHierarchyInfo>? _cachedRoleHierarchyInfos;

	public AuthorizationDataSource CurrentSource => AuthorizationDataSource.Client;

	public bool IsAvailable => true; // Always available in WASM

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

	public Task<IReadOnlyList<RoleHierarchyInfo>> GetAllRoleHierarchyInfoAsync() {
		this._cachedRoleHierarchyInfos ??= this.BuildRoleHierarchyInfos();
		return Task.FromResult(this._cachedRoleHierarchyInfos);
	}

	public Task<string> GetAuthorizationFlowDiagramAsync() {
		return Task.FromResult(AuthorizationFlowDiagram);
	}

	public Task<string> GetRoleHierarchyDiagramAsync() {
		return Task.FromResult(RoleHierarchyRenderer.ToMermaidDiagram(roleRegistry));
	}

	public async Task RefreshAsync(int maxRoleDepth) {
		// Clear all caches
		this._cachedRoleHierarchyInfos = null;

		// Initialize provider with services and clears the internal cache
		AuthorizationModel.Instance.Initialize(serviceProvider);

		// Run analysis using the composite analyzer
		var analysisOptions = new AnalysisOptions {
			MaxHierarchyDepth = maxRoleDepth,
			IncludeInfoIssues = true,
			ExcludedCategories = []
		};

		var analyzer = DefaultAnalyzerProvider.CreateAnalyzer(roleRegistry, serviceProvider, analysisOptions);
		this._cachedAnalysisReport = await analyzer.AnalyzeAllAsync();
	}

	private List<RoleHierarchyInfo> BuildRoleHierarchyInfos() {
		var allRoles = roleRegistry.GetRegisteredRoles();
		var result = new List<RoleHierarchyInfo>();

		foreach (var role in allRoles) {
			var childRoles = roleRegistry.GetInheritedRoles(role);
			var parentRoles = roleRegistry.GetInheritingRoles(role);
			var depth =
				this.CalculateHierarchyDepth(role, []);

			result.Add(new RoleHierarchyInfo(
				role,
				[.. childRoles],
				[.. parentRoles],
				childRoles.Count,
				parentRoles.Count,
				depth
			));
		}

		return [.. result
			.OrderBy(r => r.HierarchyDepth)
			.ThenBy(r => r.RoleString)];
	}

	private int CalculateHierarchyDepth(Role role, HashSet<Role> visited) {
		if (visited.Contains(role)) {
			return 0;
		}

		visited.Add(role);

		var inheritedRoles = roleRegistry.GetInheritedRoles(role);
		if (inheritedRoles.Count == 0) {
			return 0;
		}

		return inheritedRoles.Max(inherited => this.CalculateHierarchyDepth(inherited, [.. visited])) + 1;
	}

	/// <summary>
	/// Static authorization flow diagram showing the request authorization pipeline
	/// </summary>
	private const string AuthorizationFlowDiagram = """
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
		""";
}
