namespace SprintManagementBoard;

/// <summary>
/// UserModel represents a user in the sprint management board, containing properties such as a unique identifier, name, associated project, and a list of tasks assigned to the user.
/// </summary>
public class UserModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the project to which the user is associated.
    /// </summary>
    public string ProjectId { get; set; } = "Project1";
}
