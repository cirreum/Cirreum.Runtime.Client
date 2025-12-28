namespace Cirreum.Demo.Client.Services;

/// <summary>
/// Role hierarchy information for display purposes
/// </summary>
public record RoleHierarchyInfo(
	Role Role,
	IReadOnlyList<Role> ChildRoles,
	IReadOnlyList<Role> ParentRoles,
	int InheritsFromCount,
	int InheritedByCount,
	int CurrentOrder
);