namespace Cirreum.Demo.Client.Services;

/// <summary>
/// Contains hierarchy information for a role
/// </summary>
public record RoleHierarchyInfo(
	Role Role,
	IReadOnlyList<Role> ChildRoles,
	IReadOnlyList<Role> ParentRoles,
	int InheritsFromCount,
	int InheritedByCount,
	int HierarchyDepth
) {
	/// <summary>
	/// The role as a string for display/filtering purposes
	/// </summary>
	public string RoleString => this.Role.ToString();
}