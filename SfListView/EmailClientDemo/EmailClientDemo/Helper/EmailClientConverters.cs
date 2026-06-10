using Syncfusion.Maui.DataSource.Extensions;
using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace EmailClientDemo
{
    #region DateTimeToStringConverter

    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string? text = value as string;
            return ImageSource.FromResource(typeof(ImageSourceConverter).GetTypeInfo().Assembly.GetName().Name + ".Resources.Images." + text, typeof(ImageSourceConverter).GetTypeInfo().Assembly);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Converter class helps to convert DateTime to string format for email display.
    /// </summary>
    public class EmailDateTimeConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null)
				return string.Empty;

			var datetime = (DateTime)value;
			// Always show time (e.g., 9:01 AM) per UI requirement
			return datetime.ToLocalTime().ToString("h:mm tt");
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region EmailOpacityConverter

	/// <summary>
	/// Converter class helps to convert opacity based on IsOpened property.
	/// </summary>
	public class EmailOpacityConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value != null)
			{
				var isOpened = (bool)value;

				if (!isOpened)
				{
					return 1.0;
				}
				else
				{
					return 0.6;
				}
			}
			return 0.6;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region FontAttributeConverter

	/// <summary>
	/// Converter class helps to convert FontAttribute based on IsOpened property.
	/// </summary>
	public class EmailFontAttributeConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value != null)
			{
				var isOpened = (bool)value;

				if (!isOpened)
				{
					return FontAttributes.Bold;
				}
			}

			return FontAttributes.None;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region GroupHeaderTextConverter

	/// <summary>
	/// Converter class helps to convert GroupHeader text based on Date property.
	/// </summary>
	public class EmailGroupHeaderConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			switch ((EmailGroupName)value!)
			{
				case EmailGroupName.Today:
					return "Today";
				case EmailGroupName.Yesterday:
					return "Yesterday";
				case EmailGroupName.Earlier:
					return "Earlier";
				default:
					return "";
			}
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region FavoriteIconConverter

	/// <summary>
	/// Converter class to show appropriate favorite icon.
	/// </summary>
	public class EmailFavoriteIconConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value != null && (bool)value)
				return "\ue7CF"; // Filled star
			else
				return "\ue73A"; // Outline star
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region FavoriteColorConverter

	/// <summary>
	/// Converter class to show appropriate favorite color.
	/// </summary>
	public class EmailFavoriteColorConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value != null && (bool)value)
				return Color.FromArgb("#F9BC16"); // Yellow/gold for favorited
			else
				return Color.FromArgb("#666666"); // Gray for not favorited
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region UnreadIndicatorConverter

	/// <summary>
	/// Converter class to show unread indicator (blue dot).
	/// </summary>
	public class EmailUnreadIndicatorConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value != null)
			{
				var isOpened = (bool)value;
				return !isOpened; // Show blue dot if not opened
			}
			return false;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region TabBackgroundConverter

	/// <summary>
	/// Converter class to show appropriate background color for tab based on selection.
	/// </summary>
	public class TabBackgroundConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return Colors.Transparent;

			bool isFocusedSelected = (bool)value;
			string tabName = parameter.ToString() ?? "";

			// Show white background if this tab is selected
			if ((tabName == "Focused" && isFocusedSelected) || (tabName == "Other" && !isFocusedSelected))
				return Color.FromArgb("#FFFFFF");
			else
				return Colors.Transparent;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region TabTextColorConverter

	/// <summary>
	/// Converter class to show appropriate text color for tab based on selection.
	/// </summary>
	public class TabTextColorConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return Color.FromArgb("#666666");

			bool isFocusedSelected = (bool)value;
			string tabName = parameter.ToString() ?? "";

			// Show black text if this tab is selected, gray if not
			if ((tabName == "Focused" && isFocusedSelected) || (tabName == "Other" && !isFocusedSelected))
				return Color.FromArgb("#000000");
			else
				return Color.FromArgb("#666666");
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region TabFontFamilyConverter

	/// <summary>
	/// Converter class to show appropriate font family for tab based on selection.
	/// </summary>
	public class TabFontFamilyConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return "Roboto-Regular";

			bool isFocusedSelected = (bool)value;
			string tabName = parameter.ToString() ?? "";

			// Show medium font if this tab is selected, regular if not
			if ((tabName == "Focused" && isFocusedSelected) || (tabName == "Other" && !isFocusedSelected))
				return "Roboto-Medium";
			else
				return "Roboto-Regular";
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	#region TabBorderVisibilityConverter

	/// <summary>
	/// Converter class to show/hide the white border background for tab based on selection.
	/// </summary>
	public class TabBorderVisibilityConverter : IValueConverter
	{
		public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return false;

			bool isFocusedSelected = (bool)value;
			string tabName = parameter.ToString() ?? "";

			// Show border if this tab is selected
			return (tabName == "Focused" && isFocusedSelected) || (tabName == "Other" && !isFocusedSelected);
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	#endregion
}
