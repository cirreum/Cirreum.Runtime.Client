namespace Cirreum.Demo.Client.Services;

using Cirreum.Authorization;
using Cirreum.Authorization.Analysis;
using Cirreum.Authorization.Modeling.Export;
using System.Net.Http.Json;

/// <summary>
/// API-based implementation that fetches analysis data from a server endpoint.
/// Use this when running the analyzer on the server and sending results to the client.
/// </summary>
public class ApiAuthorizationDataService(
	HttpClient httpClient,
	string baseUrl = "/api/authorization"
) : IAuthorizationDataService {

	private readonly string _baseUrl = baseUrl.TrimEnd('/');

	// Cached data
	private AnalysisReport? _cachedReport;
	private DomainCatalog? _cachedResourceCatalog;
	private IEnumerable<Role>? _cachedRoles;
	private string? _cachedAuthFlowDiagram;
	private string? _cachedRoleHierarchyDiagram;

	public async Task<AnalysisReport> GetAnalysisReportAsync(int maxRoleDepth) {
		if (this._cachedReport == null) {
			await this.RefreshAsync(maxRoleDepth);
		}
		return this._cachedReport!;
	}

	public async Task<AnalysisSummary> GetAnalysisSummaryAsync(int maxRoleDepth) {
		var report = await this.GetAnalysisReportAsync(maxRoleDepth);
		return report.GetSummary();
	}

	public async Task<DomainCatalog> GetCatalogAsync() {
		if (this._cachedResourceCatalog == null) {
			this._cachedResourceCatalog = await httpClient.GetFromJsonAsync<DomainCatalog>(
				$"{this._baseUrl}/resource-catalog") ?? new DomainCatalog();
		}
		return this._cachedResourceCatalog;
	}

	public async Task<IEnumerable<Role>> GetRolesAsync() {
		if (this._cachedRoles == null) {
			// Roles are serialized as their string representation (namespace:name)
			var roleStrings = await httpClient.GetFromJsonAsync<List<string>>(
				$"{this._baseUrl}/roles") ?? [];

			this._cachedRoles = [.. roleStrings.Select(ParseRole)];
		}
		return this._cachedRoles;
	}

	public async Task<RoleHierarchyInfo> GetRoleHierarchyInfoAsync(Role role) {
		var response = await httpClient.GetFromJsonAsync<RoleHierarchyInfoDto>(
			$"{this._baseUrl}/roles/{Uri.EscapeDataString(role.ToString())}/hierarchy");

		if (response == null) {
			return new RoleHierarchyInfo(role, [], [], 0, 0, 0);
		}

		return new RoleHierarchyInfo(
			role,
			[.. response.ChildRoles.Select(ParseRole)],
			[.. response.ParentRoles.Select(ParseRole)],
			response.InheritsFromCount,
			response.InheritedByCount,
			response.CurrentOrder
		);
	}

	/// <summary>
	/// Parses a role string in the format "namespace:name" into a Role object.
	/// Note: Application roles (app:*) cannot be created via the public constructor,
	/// so we need to look them up from ApplicationRoles or handle them specially.
	/// </summary>
	private static Role ParseRole(string roleString) {
		var colonIndex = roleString.IndexOf(':');
		if (colonIndex <= 0 || colonIndex >= roleString.Length - 1) {
			throw new FormatException($"Invalid role format: '{roleString}'. Expected format: 'namespace:name'");
		}

		var ns = roleString[..colonIndex];
		var name = roleString[(colonIndex + 1)..];

		// Application roles (app:*) cannot be created via public constructor
		// They must be looked up from the predefined ApplicationRoles
		if (ns.Equals(Role.AppNamespace, StringComparison.OrdinalIgnoreCase)) {
			// Try to find the predefined application role
			var appRole = FindApplicationRole(name);
			if (appRole != null) {
				return appRole;
			}
			// If not found, this is an unknown app role - this shouldn't happen in normal usage
			throw new InvalidOperationException($"Unknown application role: '{roleString}'");
		}

		return new Role(ns, name);
	}

	/// <summary>
	/// Finds a predefined application role by name.
	/// </summary>
	private static Role? FindApplicationRole(string name) {
		// Check against known application roles from ApplicationRoles static class
		var normalizedName = name.ToLowerInvariant();

		return normalizedName switch {
			"user" => ApplicationRoles.AppUserRole,
			"internal" => ApplicationRoles.AppInternalRole,
			"agent" => ApplicationRoles.AppAgentRole,
			"manager" => ApplicationRoles.AppManagerRole,
			"admin" => ApplicationRoles.AppAdminRole,
			"system" => ApplicationRoles.AppSystemRole,
			_ => null
		};
	}

	public async Task<string> GetAuthorizationFlowDiagramAsync() {
		this._cachedAuthFlowDiagram ??= await httpClient.GetStringAsync(
				$"{this._baseUrl}/diagrams/auth-flow");
		return this._cachedAuthFlowDiagram;
	}

	public async Task<string> GetRoleHierarchyDiagramAsync() {
		this._cachedRoleHierarchyDiagram ??= await httpClient.GetStringAsync(
				$"{this._baseUrl}/diagrams/role-hierarchy");
		return this._cachedRoleHierarchyDiagram;
	}

	public async Task RefreshAsync(int maxRoleDepth) {
		// Clear all caches
		this._cachedReport = null;
		this._cachedResourceCatalog = null;
		this._cachedRoles = null;
		this._cachedAuthFlowDiagram = null;
		this._cachedRoleHierarchyDiagram = null;

		// Fetch fresh report
		this._cachedReport = await httpClient.GetFromJsonAsync<AnalysisReport>(
			$"{this._baseUrl}/report") ?? AnalysisReport.ForCategory("Empty");
	}

	/// <summary>
	/// DTO for role hierarchy info from the API
	/// </summary>
	private record RoleHierarchyInfoDto {
		public List<string> ChildRoles { get; init; } = [];
		public List<string> ParentRoles { get; init; } = [];
		public int InheritsFromCount { get; init; }
		public int InheritedByCount { get; init; }
		public int CurrentOrder { get; init; }
	}
}
