namespace SprintManagementBoard;

/// <summary>
/// TaskModel represents a task in the sprint management board, containing properties such as title, description, assignee, project association, status, priority, dates, progress, type, and tags for categorization.
/// </summary>
public class TaskModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the task.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the title of the task.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the task.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the task.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user to whom the task is assigned.
    /// </summary>
    public string? AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the project to which the task belongs.
    /// </summary>
    public string ProjectId { get; set; } = "Project1";

    /// <summary>
    /// Gets or sets the status of the task (e.g., "Todo", "In Progress", "Done").
    /// </summary>
    public string Status { get; set; } = "Todo";

    /// <summary>
    /// Gets or sets the priority of the task (e.g., "Low", "Normal", "High").
    /// </summary>
    public string Priority { get; set; } = "Normal";

    /// <summary>
    /// Gets or sets the start date of the task.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the task.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the progress of the task as a percentage (0-100).
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Gets or sets the type of the task (e.g., "Task", "Bug", "Feature").
    /// </summary>
    public string Type { get; set; } = "Task";

    /// <summary>
    /// Gets or sets a list of tags associated with the task for categorization and filtering purposes.
    /// </summary>
    public List<string> Tags { get; set; } = new();
}