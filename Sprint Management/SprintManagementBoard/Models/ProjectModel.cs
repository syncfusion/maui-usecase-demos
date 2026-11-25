namespace SprintManagementBoard;

/// <summary>
/// ProjectModel represents a project in the sprint management board, containing properties such as a unique identifier and the name of the project.
/// </summary>
public class ProjectModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the project.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name of the project.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
