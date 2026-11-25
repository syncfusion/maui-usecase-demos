namespace SmartExpense;

/// <summary>
/// Defines application-wide constant values used across the app.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Default local database file name.
    /// </summary>
    public const string DatabaseName = "smartexpense.db3";

    /// <summary>
    /// Standard date display format.
    /// </summary>
    public const string DateFormat = "dd MMM yyyy";

    /// <summary>
    /// Standard currency display format.
    /// </summary>
    public const string CurrencyFormat = "C2";

    public const string SyncfusionLicenseKey = "YOUR_LICENSE_KEY";
}

/// <summary>
/// Defines icon glyphs used in insights and summaries.
/// </summary>
public static class InsightIcons
{
    /// <summary>
    /// Icon representing account balance.
    /// </summary>
    public const string Balance = "\ue734";

    /// <summary>
    /// Icon representing income.
    /// </summary>
    public const string Income = "\ue735";

    /// <summary>
    /// Icon representing expenses.
    /// </summary>
    public const string Expense = "\ue736";

    /// <summary>
    /// Icon representing savings.
    /// </summary>
    public const string Savings = "\ue737";
}

/// <summary>
/// Defines color constants used in insights and summaries.
/// </summary>
public static class InsightColors
{
    /// <summary>
    /// Color used for balance indicators.
    /// </summary>
    public static readonly Color Balance = Color.FromArgb("#9215F3");

    /// <summary>
    /// Color used for income indicators.
    /// </summary>
    public static readonly Color Income = Color.FromArgb("#00E190");

    /// <summary>
    /// Color used for expense indicators.
    /// </summary>
    public static readonly Color Expense = Color.FromArgb("#FF4E4E");

    /// <summary>
    /// Color used for savings indicators.
    /// </summary>
    public static readonly Color Savings = Color.FromArgb("#116DF9");
}
