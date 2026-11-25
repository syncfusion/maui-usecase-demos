namespace SprintManagementBoard;

/// <summary>
/// Single page for all platforms. Layout differences are expressed in XAML with OnIdiom.
/// </summary>
public partial class MainPage : ContentPage
{
	private readonly TasksViewModel _viewModel;
	private readonly ITaskService _taskService;
	private string _selectedView = "kanban";

	public MainPage()
	{
		InitializeComponent();
		_taskService = SprintManagementBoard.App.Services?.GetService(typeof(ITaskService)) as ITaskService ?? new MockTaskService();
		_viewModel = SprintManagementBoard.App.Services?.GetService(typeof(TasksViewModel)) as TasksViewModel ?? new TasksViewModel(_taskService);
		BindingContext = _viewModel;
		RequestedThemeChanged();
		Application.Current!.RequestedThemeChanged += (_, _) => RequestedThemeChanged();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_viewModel.LoadCommand?.Execute(null);
		SetSelectedSidebar(_selectedView);
#if DEBUG && WINDOWS
		var start = "SPRINT_START_VIEW";
		if (start == "scheduler" || start == "datagrid") Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(3), () => ShowView(start));
		else if (start == "popup") Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(3), () => NewTask_Clicked(null, EventArgs.Empty));
#endif
	}

	private static async Task SafeExecuteAsync(Func<Task> action)
	{
		try { await action(); }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

	// ---------- Date range ----------

	private void ShowDateRange_Clicked(object? sender, EventArgs e)
	{
		RangeStartPicker.SelectedDate = _viewModel.StartDateFilter ?? DateTime.Today;
		RangeEndPicker.SelectedDate = _viewModel.EndDateFilter ?? DateTime.Today.AddDays(7);
		dateRangePopup.IsVisible = true;
	}

	private async void ApplyRange_Clicked(object? sender, EventArgs e)
	{
		_viewModel.StartDateFilter = RangeStartPicker.SelectedDate;
		_viewModel.EndDateFilter = RangeEndPicker.SelectedDate;
		DateRangeLabel.Text = $"{RangeStartPicker.SelectedDate:dd MMM} - {RangeEndPicker.SelectedDate:dd MMM yyyy}";
		await SafeExecuteAsync(() => _viewModel.ApplyFilters());
		dateRangePopup.IsVisible = false;
	}

	private async void ClearRange_Clicked(object? sender, EventArgs e)
	{
		_viewModel.StartDateFilter = null;
		_viewModel.EndDateFilter = null;
		DateRangeLabel.Text = "All dates";
		await SafeExecuteAsync(() => _viewModel.ApplyFilters());
		dateRangePopup.IsVisible = false;
	}

	private void CancelRange_Clicked(object? sender, EventArgs e) => dateRangePopup.IsVisible = false;

	// ---------- Task popup ----------

	private void NewTask_Clicked(object? sender, EventArgs e)
	{
		PopupTitleEntry.Text = string.Empty;
		PopupDescriptionEditor.Text = string.Empty;
		if (PopupUserPicker.ItemsSource != null) PopupUserPicker.SelectedIndex = 0;
		PopupStatusPicker.SelectedIndex = 0;
		PopupStartDatePicker.SelectedDate = DateTime.Today;
		PopupEndDatePicker.SelectedDate = DateTime.Today.AddDays(1);
		PopupProgressEntry.Text = "0";
		taskPopup.IsVisible = true;
	}

	private async void SavePopup_Clicked(object? sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(PopupTitleEntry.Text))
		{
			await DisplayAlertAsync("Title required", "Please enter a task title.", "OK");
			return;
		}

		var selectedUser = PopupUserPicker.SelectedItem as UserModel;
		var task = new TaskModel
		{
			Title = PopupTitleEntry.Text.Trim(),
			Description = PopupDescriptionEditor.Text,
			AssigneeId = selectedUser?.Id,
			ProjectId = selectedUser?.ProjectId ?? _viewModel.SelectedProject?.Id ?? "Project1",
			Status = PopupStatusPicker.SelectedItem?.ToString() ?? "Todo",
			StartDate = PopupStartDatePicker.SelectedDate,
			EndDate = PopupEndDatePicker.SelectedDate,
			Progress = int.TryParse(PopupProgressEntry.Text, out var p) ? Math.Clamp(p, 0, 100) : 0
		};

		await _taskService.CreateTaskAsync(task);
		await SafeExecuteAsync(async () => { await _viewModel.LoadAsync(); await _viewModel.ApplyFilters(); });
		taskPopup.IsVisible = false;
	}

	private void CancelPopup_Clicked(object? sender, EventArgs e) => taskPopup.IsVisible = false;

	// ---------- View switching ----------

	private void ShowKanban_Clicked(object? sender, EventArgs e) => ShowView("kanban");
	private void ShowScheduler_Clicked(object? sender, EventArgs e) => ShowView("scheduler");
	private void ShowDataGrid_Clicked(object? sender, EventArgs e) => ShowView("datagrid");

	private void ShowView(string view)
	{
		_selectedView = view;
		kanbanView.IsVisible = view == "kanban";
		schedulerView.IsVisible = view == "scheduler";
		dataGridView.IsVisible = view == "datagrid";
		SetSelectedSidebar(view);
	}

	private void SetSelectedSidebar(string selected)
	{
		Apply(KanbanFrame, KanbanIcon, KanbanLabel, selected == "kanban");
		Apply(SchedulerFrame, SchedulerIcon, SchedulerLabel, selected == "scheduler");
		Apply(DataGridFrame, DataGridIcon, DataGridLabel, selected == "datagrid");

		static void Apply(Border frame, Microsoft.Maui.Controls.Shapes.Path icon, Label label, bool active)
		{
			var idle = (Brush)Application.Current!.Resources["NavIdleBrush"];
			var on = (Brush)Application.Current!.Resources["NavActiveBrush"];
			frame.BackgroundColor = active ? Color.FromArgb("#4F46E5") : Colors.Transparent;
			icon.Stroke = active ? on : idle;
			label.TextColor = active ? Colors.White : Color.FromArgb("#9CA3AF");
		}
	}

	// Fit the four columns to the host width; below the minimum, the board scrolls horizontally inside SfKanban.
	private void KanbanHost_SizeChanged(object? sender, EventArgs e)
	{
		if (kanbanView.Width <= 0) return;
		var min = DeviceInfo.Idiom == DeviceIdiom.Phone ? 250 : 190;
		var width = Math.Max(min, Math.Floor((kanbanView.Width - 24) / 4));
		KanbanBoard.MinimumColumnWidth = min;
		KanbanBoard.MaximumColumnWidth = width;
		KanbanBoard.ColumnWidth = width;
	}

	private void RequestedThemeChanged() => (Application.Current as App)?.ApplySyncfusionTheme();
}
