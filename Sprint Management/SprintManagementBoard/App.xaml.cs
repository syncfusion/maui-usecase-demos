using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Themes;

namespace SprintManagementBoard;

public partial class App : Application
{
	public static IServiceProvider? Services { get; set; }

	private ResourceDictionary? _overrides;

	public App()
	{
		Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JBaF1cXGFCf1FpRGFGfV5yckVHYVZSQ3xaQE0SNHVRdkdlWH5cc3RSQmheVkRyVkdWYEw=");
		InitializeComponent();
		ApplySyncfusionTheme();
		RequestedThemeChanged += (_, _) => ApplySyncfusionTheme();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());

		// Desktop windows otherwise open at the OS default size, which can be larger than the screen.
		if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
		{
			var info = DeviceDisplay.Current.MainDisplayInfo;
			var screenW = info.Width / info.Density;
			var screenH = info.Height / info.Density;
			window.MinimumWidth = 720;
			window.MinimumHeight = 520;
			window.Width = Math.Min(1360, screenW * 0.9);
			window.Height = Math.Min(860, screenH * 0.88);
			window.X = Math.Max(0, (screenW - window.Width) / 2);
			window.Y = Math.Max(0, (screenH - window.Height) / 2.5);
		}

		return window;
	}

	/// <summary>
	/// Keeps Syncfusion controls (Kanban, Scheduler, DataGrid, ComboBox) in step with the system Light/Dark theme.
	/// </summary>
	public void ApplySyncfusionTheme()
	{
		var dark = RequestedTheme == AppTheme.Dark;
		var merged = Resources.MergedDictionaries;

		var theme = merged.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
		if (theme == null)
		{
			theme = new SyncfusionThemeResourceDictionary();
			merged.Add(theme);
		}
		theme.VisualTheme = dark ? SfVisuals.MaterialDark : SfVisuals.MaterialLight;

		if (_overrides != null) merged.Remove(_overrides);

		var header = Color.FromArgb(dark ? "#232734" : "#F4F6FB");
		var column = Color.FromArgb(dark ? "#171A23" : "#F4F6FB");
		var text = Color.FromArgb(dark ? "#F3F4F6" : "#111827");
		var surface = Color.FromArgb(dark ? "#1A1D27" : "#FFFFFF");
		_overrides = new ResourceDictionary
		{
			["SfSchedulerNormalHeaderBackground"] = header,
			["SfKanbanHeaderBackgroundColor"] = header,
			["SfKanbanColumnBackgroundColor"] = column,
			["SfKanbanHeaderCollapsedBackgroundColor"] = header,
			["SfComboBoxDropdownBackground"] = surface,
			["SfComboBoxNormalDropdownSelectionBackground"] = header,
			["SfSchedulerGroupResourceHeaderBackground"] = header,
			["SfSchedulerGroupResourceTimeslotBackground"] = surface,
			["SfSchedulerNormalAppointmentTextColor"] = Colors.White,
			["SfComboBoxNormalBackground"] = header,
			["SfComboBoxFocusedBackground"] = header,
			["SfComboBoxHoverBackground"] = header,
			["SfComboBoxNormalTextColor"] = text,
			["SfComboBoxHoverTextColor"] = text,
			["SfComboBoxNormalDropdownItemsTextColor"] = text,
			["SfComboBoxSelectedDropdownItemTextColor"] = text,
			["SfComboBoxPlaceholderTextColor"] = Color.FromArgb(dark ? "#9CA3AF" : "#6B7280"),
			["SfComboBoxNormalStroke"] = Color.FromArgb(dark ? "#2C3142" : "#E3E7F0"),
			["SfComboBoxDropdownBorder"] = Color.FromArgb(dark ? "#2C3142" : "#E3E7F0"),
		};
		merged.Add(_overrides);
	}
}
