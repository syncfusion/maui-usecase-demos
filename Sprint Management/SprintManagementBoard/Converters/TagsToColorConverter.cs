using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SprintManagementBoard;

    /// <summary>
    /// TagsToColorConverter is a value converter that takes a collection of tags (or a single tag) and returns a corresponding color based on the content of the tags. 
    /// </summary>
    public class TagsToColorConverter : IValueConverter
    {
        /// <summary>
        /// Method to convert a collection of tags (or a single tag) into a color.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                IEnumerable<string>? tags = null;
                if (value is IEnumerable<string> t) tags = t;
                else if (value is IList<string> lt) tags = lt;
                else if (value is string s)
                {
                    tags = new[] { s };
                }

                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        if (string.IsNullOrWhiteSpace(tag)) continue;
                        var lower = tag.Trim().ToLowerInvariant();
                        if (lower.Contains("bug") || lower.Contains("breaking"))
                            return Colors.IndianRed;
                        if (lower.Contains("customer") || lower.Contains("task"))
                            return Colors.DodgerBlue;
                        if (lower.Contains("system"))
                            return Colors.SeaGreen;
                        if (lower.Contains("review") || lower.Contains("testing"))
                            return Colors.SandyBrown;
                    }
                }

                // fallback color
                return Colors.MediumPurple;
            }
            catch
            {
                return Colors.MediumPurple;
            }
        }

        /// <summary>
        /// Method to convert back from color to tags, which is not implemented in this case and returns null.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
