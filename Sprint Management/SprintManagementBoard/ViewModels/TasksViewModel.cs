using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Kanban;
using Syncfusion.Maui.Scheduler;

namespace SprintManagementBoard;

/// <summary>
/// TasksViewModel serves as the main ViewModel for managing tasks, projects, and users in the Sprint Management Board application. It handles data loading, filtering, and interaction logic for the Kanban board, scheduler, and task details.
/// </summary>
public class TasksViewModel : INotifyPropertyChanged
{
    private readonly ITaskService _taskService;
    private ProjectModel? _selectedProject;
    private ObservableCollection<KanbanModel> _kanbanData = new();
    private ObservableCollection<SchedulerResource> _users = new();
    private ObservableCollection<TaskModel> _gridUsers = new();
    private DateTime? _startDateFilter;
    private DateTime? _endDateFilter;
    private bool _isBusy;
    private readonly Random _random = new();

    /// <summary>
    /// TasksViewModel constructor initializes the ViewModel with the provided ITaskService for data operations and sets up commands for loading data, editing tasks, moving tasks, saving tasks, and rescheduling tasks.
    /// </summary>
    /// <param name="taskService"></param>
    public TasksViewModel(ITaskService taskService)
    {
        _taskService = taskService;
        LoadCommand = new Command(async () => await LoadAsync());
        EditTaskCommand = new Command<object>(async (o) => await OpenEditModalAsync(o));
        MoveTaskCommand = new Command<object>(async (o) => await OnTaskMovedAsync(o));
        SaveTaskCommand = new Command<TaskModel>(async (t) => await SaveTaskAsync(t));
        RescheduleTaskCommand = new Command<object>(async (o) => await OnRescheduleAsync(o));
    }

    /// <summary>
    /// Gets or sets the currently selected project for filtering tasks.
    /// </summary>
    public ProjectModel? SelectedProject
    {
        get => _selectedProject;
        set
        {
            if (_selectedProject == value) return;
            _selectedProject = value;
            OnPropertyChanged(nameof(SelectedProject));
            _ = ApplyFilters();
        }
    }

    /// <summary>
    /// Gets the collection of tasks loaded from the data source.
    /// </summary>
    public ObservableCollection<TaskModel> Tasks { get; } = new();

    /// <summary>
    /// Gets or sets the collection of KanbanModel items used to display tasks on the Kanban board.
    /// </summary>
    public ObservableCollection<KanbanModel> KanbanData
    {
        get => _kanbanData;
        private set
        {
            if (ReferenceEquals(_kanbanData, value)) return;
            _kanbanData = value;
            OnPropertyChanged(nameof(KanbanData));
        }
    }

    /// <summary>
    /// Gets or sets the collection of SchedulerResource items representing users for the scheduler view.
    /// </summary>
    public ObservableCollection<SchedulerResource> Users
    {
        get => _users;
        private set
        {
            if (ReferenceEquals(_users, value)) return;
            _users = value;
            OnPropertyChanged(nameof(Users));
        }
    }

    /// <summary>
    /// Gets the collection of SchedulerAppointment items representing tasks for the scheduler view.
    /// </summary>
    public ObservableCollection<SchedulerAppointment> Appointments { get; } = new();

    /// <summary>
    /// Gets the collection of all users across projects, used for populating user pickers and maintaining a master list of users.
    /// </summary>
    public ObservableCollection<UserModel> AllUsers { get; } = new();

    /// <summary>
    /// Gets the collection of users filtered by the selected project, used for displaying in the DataGrid and other project-scoped views.
    /// </summary>
    public ObservableCollection<TaskModel> GridUsers
    {
        get => _gridUsers;
        private set
        {
            if (ReferenceEquals(_gridUsers, value)) return;
            _gridUsers = value;
            OnPropertyChanged(nameof(GridUsers));
        }
    }

    /// <summary>
    /// Gets the collection of projects derived from the loaded tasks, used for project selection and filtering.
    /// </summary>
    public ObservableCollection<ProjectModel> Projects { get; } = new();

    /// <summary>
    /// Gets the collection of visible avatar image paths for the first few assignees of the filtered tasks.
    /// </summary>
    public ObservableCollection<string> VisibleAvatars { get; } = new();

    /// <summary>
    /// Gets the text indicating the count of remaining avatars not shown in the VisibleAvatars collection.
    /// </summary>
    public string RemainingAvatarsText { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether the "+N" avatar overflow badge should be shown.
    /// </summary>
    public bool HasRemainingAvatars => !string.IsNullOrEmpty(RemainingAvatarsText);

    /// <summary>
    /// Gets or sets the start date filter for tasks.
    /// </summary>
    public DateTime? StartDateFilter
    {
        get => _startDateFilter;
        set
        {
            if (_startDateFilter == value) return;
            _startDateFilter = value;
            OnPropertyChanged(nameof(StartDateFilter));
        }
    }

    /// <summary>
    /// Gets or sets the end date filter for tasks.
    /// </summary>
    public DateTime? EndDateFilter
    {
        get => _endDateFilter;
        set
        {
            if (_endDateFilter == value) return;
            _endDateFilter = value;
            OnPropertyChanged(nameof(EndDateFilter));
        }
    }

    /// <summary>
    /// Gets whether the view model is busy loading or applying filters.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
        }
    }

    /// <summary>
    /// Gets the count of tasks in the "To Do" status.
    /// </summary>
    public int ToDoCount { get; private set; }

    /// <summary>
    /// Gets the count of tasks in the "In Progress" status.
    /// </summary>
    public int InProgressCount { get; private set; }

    /// <summary>
    /// Gets the count of tasks in the "Testing" status.
    /// </summary>
    public int TestingCount { get; private set; }

    /// <summary>
    /// Gets the count of tasks in the "Done" status.
    /// </summary>
    public int DoneCount { get; private set; }

    /// <summary>
    /// Gets the command to load tasks and related data from the data source.
    /// </summary>
    public Command LoadCommand { get; }

    /// <summary>
    /// Gets the command to open the edit modal for a task, allowing users to modify task details.
    /// </summary>
    public Command<object> EditTaskCommand { get; }

    /// <summary>
    /// Gets the command to handle task movement events from the Kanban board.
    /// </summary>
    public Command<object> MoveTaskCommand { get; }

    /// <summary>
    /// Gets the command to save task changes back to the data source after editing.
    /// </summary>
    public Command<TaskModel> SaveTaskCommand { get; }

    /// <summary>
    /// Gets the command to handle task rescheduling events from the scheduler view.
    /// </summary>
    public Command<object> RescheduleTaskCommand { get; }

    /// <summary>
    /// Event triggered when a property value changes, used to notify the UI of updates to bindable properties.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;


    /// <summary>
    /// Method to load tasks, projects, and users from the data source using the ITaskService.
    /// </summary>
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var items = await _taskService.GetAllAsync();
            Tasks.Clear();
            foreach (var t in items)
            {
                Tasks.Add(t);
            }

            Projects.Clear();
            var projectGroups = Tasks.GroupBy(t => string.IsNullOrEmpty(t.ProjectId) ? "Project1" : t.ProjectId);
            foreach (var g in projectGroups)
            {
                var pid = g.Key;
                var name = pid.StartsWith("project", StringComparison.OrdinalIgnoreCase)
                    ? pid.Replace("project", "Project ", StringComparison.OrdinalIgnoreCase)
                    : pid;
                Projects.Add(new ProjectModel { Id = pid, Name = name });
            }

            if (SelectedProject == null && Projects.Count > 0)
            {
                SelectedProject = Projects.First();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Method to apply filters based on the selected project and date range, updating the Kanban board, scheduler appointments, avatar previews, and user lists accordingly.
    /// </summary>
    public async Task ApplyFilters()
    {
        await _applyFiltersLock.WaitAsync();
        try
        {
            IsBusy = true;
            var filtered = Tasks.AsEnumerable();
            GridUsers.Clear();

            if (SelectedProject != null)
            {
                foreach (var item in Tasks.Where(t =>
                    string.Equals(t.ProjectId, SelectedProject.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    GridUsers.Add(item);
                }
            }

            filtered = GridUsers;
            if (StartDateFilter.HasValue && EndDateFilter.HasValue)
            {
                var s = StartDateFilter.Value.Date;
                var e = EndDateFilter.Value.Date;
                filtered = filtered.Where(t => (t.EndDate.HasValue && t.EndDate.Value.Date >= s) && (t.StartDate.HasValue && t.StartDate.Value.Date <= e));
            }

            // materialize filtered results so we can reuse the same set for Kanban/appointments/grid
            var filteredList = filtered.ToList();

            // update avatar preview: show up to 3 avatars (from first tasks) and a remaining count based on total tasks
            VisibleAvatars.Clear();
            var assigneesInOrder = filteredList.Select(t => t.AssigneeId).Where(id => !string.IsNullOrEmpty(id)).ToList();
            var avatarsToShow = assigneesInOrder.Take(3).ToList();
            foreach (var a in avatarsToShow)
            {
                // Currently using a generic avatar placeholder; replace with per-user images if available
                VisibleAvatars.Add("user.png");
            }
            var remaining = Math.Max(0, filteredList.Count - avatarsToShow.Count);
            RemainingAvatarsText = remaining > 0 ? $"+{remaining}" : string.Empty;
            OnPropertyChanged(nameof(RemainingAvatarsText));
            OnPropertyChanged(nameof(HasRemainingAvatars));
            OnPropertyChanged(nameof(VisibleAvatars));


            var kanbanData = new ObservableCollection<KanbanModel>();
            foreach (var t in filteredList)
            {
                var km = new KanbanModel
                {
                    ID = double.TryParse(t.Id.GetHashCode().ToString(), out var _id) ? _id : 0,
                    Title = t.Title,
                    Description = t.Description ?? string.Empty,
                    Category = t.Status,
                    Tags = t.Tags.ToList()
                };
                // Avoid adding duplicates if the same task is already present
                if (!kanbanData.Any(k => k.ID == km.ID && k.Title == km.Title && k.Category == km.Category))
                {
                    kanbanData.Add(km);
                }
            }

            KanbanData = kanbanData;

            Appointments.Clear();
            foreach (var t in filteredList)
            {
                if (t.StartDate.HasValue && t.EndDate.HasValue)
                {
                    // avoid duplicate appointments
                    if (!Appointments.Any(a => string.Equals(a.Id?.ToString(), t.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        Appointments.Add(new SchedulerAppointment
                        {
                            Id = t.Id,
                            Subject = t.Title,
                            StartTime = t.StartDate.Value,
                            EndTime = t.EndDate.Value,
                            ResourceIds = new System.Collections.ObjectModel.ObservableCollection<object> { t.AssigneeId! },
                            Background = GetRandomColor()
                        });
                    }
                }
            }

            // Populate users (fetch all users and keep both all-users and project-scoped lists)
            var allUsers = await _taskService.GetUsersAsync();

            // populate AllUsers (unfiltered) for pickers
            AllUsers.Clear();
            foreach (var u in allUsers)
            {
                AllUsers.Add(u);
            }

            var projectUsers = string.IsNullOrEmpty(SelectedProject?.Id)
                ? allUsers
                : allUsers.Where(u => string.Equals(u.ProjectId, SelectedProject.Id, StringComparison.OrdinalIgnoreCase));

            var employees = new ObservableCollection<SchedulerResource>();
            foreach (var u in projectUsers)
            {
                employees.Add(new SchedulerResource { Id = u.Id, Name = u.Name });
            }

            Users = employees;

            UpdateCounts(filteredList);
        }
        finally
        {
            IsBusy = false;
            _applyFiltersLock.Release();
        }
    }

    /// <summary>
    /// Field to ensure that only one filter application runs at
    /// </summary>
    readonly System.Threading.SemaphoreSlim _applyFiltersLock = new(1, 1);

    /// <summary>
    /// Method to generate a random color for appointment backgrounds, used in the scheduler view to visually differentiate tasks.
    /// </summary>
    /// <returns></returns>
    static readonly string[] Palette = { "#6366F1", "#0EA5E9", "#10B981", "#F59E0B", "#EC4899", "#8B5CF6", "#14B8A6" };

    Color GetRandomColor() => Color.FromArgb(Palette[_random.Next(Palette.Length)]);

    /// <summary>
    /// Method to raise the PropertyChanged event for a given property name.
    /// </summary>
    /// <param name="name"></param>
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>
    /// Method to open the edit modal for a task.
    /// </summary>
    /// <param name="arg">The args</param>
    private Task OpenEditModalAsync(object? arg)
    {
        // Placeholder: UI will wire modal navigation. Keep minimal for now.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Method to handle task movement events from the Kanban board.
    /// </summary>
    /// <param name="arg">The args</param>
    private async Task OnTaskMovedAsync(object? arg)
    {
        // arg typically is KanbanDragEndEventArgs; try dynamic handling
        if (arg is Syncfusion.Maui.Kanban.KanbanDragEndEventArgs de)
        {
            var data = de.Data as KanbanModel;
            if (data == null) return;
            // Find corresponding TaskModel by title match as demo (IDs mapping would be better)
            var task = Tasks.FirstOrDefault(t => t.Title == data.Title);
            if (task == null) return;
            task.Status = data.Category?.ToString() ?? task.Status;
            await _taskService.UpdateTaskAsync(task);
            await LoadAsync();
        }
    }

    /// <summary>
    /// Method to save task changes back to the data source after editing.
    /// </summary>
    /// <param name="task">The task.</param>
    public async Task SaveTaskAsync(TaskModel task)
    {
        await _taskService.UpdateTaskAsync(task);
        await LoadAsync();
    }

    /// <summary>
    /// Method to handle task rescheduling events from the scheduler view.
    /// </summary>
    /// <param name="source">The source.</param>
    private void UpdateCounts(IEnumerable<TaskModel>? source = null)
    {
        var list = (source ?? Tasks).ToList();

        ToDoCount = list.Count(t => string.Equals(t.Status, "Open", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(t.Status, "Todo", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(t.Status, "ToDo", StringComparison.OrdinalIgnoreCase));

        InProgressCount = list.Count(t => string.Equals(t.Status, "InProgress", StringComparison.OrdinalIgnoreCase)
                                           || string.Equals(t.Status, "In Progress", StringComparison.OrdinalIgnoreCase));

        TestingCount = list.Count(t => string.Equals(t.Status, "Testing", StringComparison.OrdinalIgnoreCase));

        DoneCount = list.Count(t => string.Equals(t.Status, "Done", StringComparison.OrdinalIgnoreCase));

        OnPropertyChanged(nameof(ToDoCount));
        OnPropertyChanged(nameof(InProgressCount));
        OnPropertyChanged(nameof(TestingCount));
        OnPropertyChanged(nameof(DoneCount));
    }

    /// <summary>
    /// Method to handle task rescheduling events from the scheduler view.
    /// </summary>
    /// <param name="arg">The object.</param>
    private async Task OnRescheduleAsync(object? arg)
    {
        // Placeholder for scheduler args handling
        await Task.CompletedTask;
    }
}