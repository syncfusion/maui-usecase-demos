using System.Globalization;

namespace Todo.Converters;

public sealed class SearchHighlightMultiValueConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        var title = values is { Length: > 0 } ? values[0] as string ?? string.Empty : string.Empty;
        var query = values is { Length: > 1 } ? values[1] as string ?? string.Empty : string.Empty;
        var completed = values is { Length: > 2 } && values[2] is true;
        var completionDecoration = completed ? TextDecorations.Strikethrough : TextDecorations.None;
        var formattedText = new FormattedString();

        if (string.IsNullOrEmpty(query))
        {
            formattedText.Spans.Add(new Span { Text = title, TextDecorations = completionDecoration });
            return formattedText;
        }

        var highlightColor = Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#8A7FD9")
            : Color.FromArgb("#5C4EAE");

        var position = 0;
        while (position < title.Length)
        {
            var matchIndex = title.IndexOf(query, position, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                formattedText.Spans.Add(new Span { Text = title[position..], TextDecorations = completionDecoration });
                break;
            }

            if (matchIndex > position)
                formattedText.Spans.Add(new Span
                {
                    Text = title[position..matchIndex],
                    TextDecorations = completionDecoration
                });

            formattedText.Spans.Add(new Span
            {
                Text = title.Substring(matchIndex, query.Length),
                TextColor = highlightColor,
                FontAttributes = FontAttributes.Bold,
                TextDecorations = completionDecoration | TextDecorations.Underline
            });
            position = matchIndex + query.Length;
        }

        return formattedText;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
