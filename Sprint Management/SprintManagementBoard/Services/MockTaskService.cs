using System;
using System.Collections.ObjectModel;

namespace SprintManagementBoard;

/// <summary>
/// MockTaskService is a mock implementation of the ITaskService interface that provides in-memory data storage and retrieval for tasks and users. 
/// </summary>
public class MockTaskService : ITaskService
{
    /// <summary>
    /// Field to hold the in-memory list of tasks for mocking purposes.
    /// </summary>
    private readonly List<TaskModel> _tasks;

    /// <summary>
    /// MockTaskService constructor initializes the in-memory task list with sample data for testing and development.
    /// </summary>
    public MockTaskService()
    {
        _tasks = new List<TaskModel>
        {
            new TaskModel { Title = "Implement authentication flow", Name="Sarah Connor", Description = "Implement the new authentication flow for mobile users.", AssigneeId = "U1", ProjectId = "Project1", Status = "Open", Priority = "High", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), Progress = 0, Tags = new List<string>{"CustomerTask"} },
            new TaskModel { Title = "Refactor dashboard layout", Name="Ellen Ripley", Description = "Refactoring the dashboard masonry layout for better tablet support.", AssigneeId = "U2", ProjectId = "Project1", Status = "InProgress", Priority = "Normal", StartDate = DateTime.Today.AddDays(-2), EndDate = DateTime.Today.AddDays(2), Progress = 40, Tags = new List<string>{"CustomerTask"} },
            new TaskModel { Title = "Database migration", Name="Marty McFly", Description = "Database migration for v2.4 schema updates.", AssigneeId = "U3", ProjectId = "Project1", Status = "Done", Priority = "Low", StartDate = DateTime.Today.AddDays(-3), EndDate = DateTime.Today, Progress = 100, Tags = new List<string>{"System"} },
            new TaskModel { Title = "API performance optimization", Name="Sarah Connor", Description = "Improve API response times for dashboard endpoints.", AssigneeId = "U1", ProjectId = "Project1", Status = "InProgress", Priority = "High", StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(3), Progress = 50, Tags = new List<string>{"Performance"} },
            new TaskModel { Title = "Unit testing coverage", Name="Ellen Ripley", Description = "Increase unit test coverage to 85% across modules.", AssigneeId = "U2", ProjectId = "Project1", Status = "Open", Priority = "Normal", StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(4), Progress = 0, Tags = new List<string>{"Quality"} },
            // Project 2 tasks
            new TaskModel { Title = "Integrate payment gateway", Name="Tony Stark", Description = "Add support for new payment provider and webhooks.", AssigneeId = "U4", ProjectId = "Project2", Status = "Open", Priority = "High", StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(5), Progress = 0, Tags = new List<string>{"Payment"} },
            new TaskModel { Title = "Fix upload bug", Name="Natasha Romanoff", Description = "API endpoint returning 500 on large payload uploads.", AssigneeId = "U5", ProjectId = "Project2", Status = "InProgress", Priority = "Critical", StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(1), Progress = 30, Tags = new List<string>{"Bug","Breaking"} },
            new TaskModel { Title = "Email notification service", Name="Tony Stark", Description = "Implement event-based email notification system.", AssigneeId = "U4", ProjectId = "Project2", Status = "Open", Priority = "Normal", StartDate = DateTime.Today.AddDays(2), EndDate = DateTime.Today.AddDays(6), Progress = 0, Tags = new List<string>{"Notification"} },
            new TaskModel { Title = "Security vulnerability patch", Name="Natasha Romanoff", Description = "Fix reported security issues from audit.", AssigneeId = "U5", ProjectId = "Project2", Status = "InProgress", Priority = "Critical", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(2), Progress = 60, Tags = new List<string>{"Security"} },
            new TaskModel { Title = "UI polish for checkout", Name="John", Description = "Enhance checkout experience and responsiveness.", AssigneeId = "U6", ProjectId = "Project2", Status = "Testing", Priority = "Low", StartDate = DateTime.Today.AddDays(-2), EndDate = DateTime.Today.AddDays(1), Progress = 80, Tags = new List<string>{"UI"} },

        };
    }

    /// <summary>
    /// Method to retrieve all tasks from the in-memory list.
    /// </summary>
    public Task<IEnumerable<TaskModel>> GetAllAsync()
    {
        return Task.FromResult(_tasks.AsEnumerable());
    }

    /// <summary>
    /// Method to retrieve a specific task by its ID from the in-memory list.
    /// </summary>
    /// <param name="id">The ID</param>
    public Task<TaskModel?> GetByIdAsync(string id)
    {
        var t = _tasks.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(t);
    }

    /// <summary>
    /// Method to create a new task by adding it to the in-memory list.
    /// </summary>
    /// <param name="task">The task</param>
    public Task<TaskModel> CreateTaskAsync(TaskModel task)
    {
        task.Id = Guid.NewGuid().ToString();
        _tasks.Add(task);
        return Task.FromResult(task);
    }

    /// <summary>
    /// Method to update an existing task by finding it in the in-memory list and replacing it.
    /// </summary>
    /// <param name="task">The task</param>
    public Task<TaskModel> UpdateTaskAsync(TaskModel task)
    {
        var existing = _tasks.FirstOrDefault(x => x.Id == task.Id);
        if (existing != null)
        {
            var idx = _tasks.IndexOf(existing);
            _tasks[idx] = task;
        }
        return Task.FromResult(task);
    }

    /// <summary>
    /// Method to delete a task by finding it in the in-memory list and removing it.
    /// </summary>
    /// <param name="id">The ID</param>
    public Task<bool> DeleteTaskAsync(string id)
    {
        var existing = _tasks.FirstOrDefault(x => x.Id == id);
        if (existing != null)
        {
            _tasks.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <summary>
    /// Method to retrieve all users with their associated tasks from the in-memory list.
    /// </summary>
    public Task<IEnumerable<UserModel>> GetUsersAsync()
    {
        var users = new List<UserModel>
        {
            new UserModel{ Id = "U1", Name = "Sarah Connor", ProjectId = "Project1"},
            new UserModel{ Id = "U2", Name = "Ellen Ripley", ProjectId = "Project1"},
            new UserModel{ Id = "U3", Name = "Marty McFly", ProjectId = "Project1"},
            // project 2 resources
            new UserModel{ Id = "U4", Name = "Tony Stark", ProjectId = "Project2"},
            new UserModel{ Id = "U5", Name = "Natasha Romanoff", ProjectId = "Project2"},
            new UserModel{ Id = "U6", Name = "John", ProjectId = "Project2"},
        };

        return Task.FromResult(users.AsEnumerable());
    }

    /// <summary>
    /// Method to retrieve all tasks assigned to a specific user by filtering the in-memory task list based on the assignee ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>Task details list.</returns>
    private List<TaskModel> GetTaskDetails(string id)
    {
        List<TaskModel> tasks = new List<TaskModel>();
        foreach (var task in _tasks)
        {
            if (task.AssigneeId == id)
            {
                tasks.Add(task);
            }
        }

        return tasks;
    }
}