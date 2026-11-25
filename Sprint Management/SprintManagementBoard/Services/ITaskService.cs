namespace SprintManagementBoard;

/// <summary>
/// ITaskService defines the contract for a service that manages tasks and users in the sprint management board.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Method to get all tasks asynchronously.
    /// </summary>
    Task<IEnumerable<TaskModel>> GetAllAsync();

    /// <summary>
    /// MEthod to get a task by its ID asynchronously.
    /// </summary>
    Task<TaskModel?> GetByIdAsync(string id);

    /// <summary>
    /// Method to create a new task asynchronously.
    /// </summary>
    Task<TaskModel> CreateTaskAsync(TaskModel task);

    /// <summary>
    /// Method to update an existing task asynchronously.
    /// </summary>
    Task<TaskModel> UpdateTaskAsync(TaskModel task);

    /// <summary>
    /// Method to delete a task by its ID asynchronously.
    /// </summary>
    Task<bool> DeleteTaskAsync(string id);

    /// <summary>
    /// Method to get all users asynchronously.
    /// </summary>
    Task<IEnumerable<UserModel>> GetUsersAsync();
}
