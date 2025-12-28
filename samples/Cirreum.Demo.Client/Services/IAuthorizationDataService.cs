namespace Cirreum.Demo.Client.Services;

using Cirreum.Authorization;
using Cirreum.Authorization.Analysis;
using Cirreum.Authorization.Modeling.Export;

/// <summary>
/// Provides authorization data for visualization components.
/// Can be implemented for client-side analysis or server-side API calls.
/// </summary>
public interface IAuthorizationDataService {

	/// <summary>
	/// Gets the combined analysis report from all analyzers
	/// </summary>
	/// <param name="maxRoleDepth">The maximum desired depth of a role hierarchy.</param>
	Task<AnalysisReport> GetAnalysisReportAsync(int maxRoleDepth);

	/// <summary>
	/// Gets the analysis summary (derived from analysis report)
	/// </summary>
	/// <param name="maxRoleDepth">The maximum desired depth of a role hierarchy.</param>
	Task<AnalysisSummary> GetAnalysisSummaryAsync(int maxRoleDepth);

	/// <summary>
	/// Gets the domain catalog hierarchy (Domain Boundary -> Resource Kind -> Resource)
	/// </summary>
	Task<DomainCatalog> GetCatalogAsync();

	/// <summary>
	/// Gets all registered roles
	/// </summary>
	Task<IEnumerable<Role>> GetRolesAsync();

	/// <summary>
	/// Gets role hierarchy information for a specific role
	/// </summary>
	Task<RoleHierarchyInfo> GetRoleHierarchyInfoAsync(Role role);

	/// <summary>
	/// Gets the authorization flow diagram definition
	/// </summary>
	Task<string> GetAuthorizationFlowDiagramAsync();

	/// <summary>
	/// Gets the role hierarchy diagram definition
	/// </summary>
	Task<string> GetRoleHierarchyDiagramAsync();

	/// <summary>
	/// Refreshes/reloads the authorization data
	/// </summary>
	/// <param name="maxRoleDepth">The maximum desired depth of a role hierarchy.</param>
	Task RefreshAsync(int maxRoleDepth);
}