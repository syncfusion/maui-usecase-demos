using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSample
{
    public partial class CustomIncomingMessageTemplateGrid : Grid
    {
        public CustomIncomingMessageTemplateGrid()
        {
            // iOS-style margins
            Padding = new Thickness(10, 2, 60, 2); // Right margin for left-aligned messages
            HorizontalOptions = LayoutOptions.Fill;

            var container = new Grid
            {
                MaximumWidthRequest = 260, // iOS typical message width
                HorizontalOptions = LayoutOptions.Start
            };

            var bubble = new GraphicsView
            {
                Drawable = new IncomingChatBubbleDrawable(),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var label = new Label
            {
                Margin = new Thickness(20, 10, 14, 16), // Equal vertical padding (10 top, 16 bottom for tail space)
                TextColor = Colors.Black,
                FontSize = 17, // iOS default message font size
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Start,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            label.SetBinding(Label.TextProperty, "Text");

            label.SizeChanged += (s, e) =>
            {
                bubble.WidthRequest = label.Width + 34; // Account for left/right padding + tail
                bubble.HeightRequest = label.Height + 26; // Account for top/bottom padding + tail extension
            };

            container.Children.Add(bubble);
            container.Children.Add(label);

            Children.Add(container);
        }
    }

    public partial class CustomOutgoingMessageTemplateGrid : Grid
    {
        public CustomOutgoingMessageTemplateGrid()
        {
            // iOS-style margins
            Padding = new Thickness(60, 2, 10, 2); // Left margin for right-aligned messages
            HorizontalOptions = LayoutOptions.Fill;

            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var container = new Grid
            {
                MaximumWidthRequest = 260, // iOS typical message width
                HorizontalOptions = LayoutOptions.End
            };

            var bubble = new GraphicsView
            {
                Drawable = new OutgoingChatBubbleDrawable(),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var label = new Label
            {
                Margin = new Thickness(14, 10, 20, 16), // Equal vertical padding (10 top, 16 bottom for tail space)
                TextColor = Colors.White,
                FontSize = 17, // iOS default message font size
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Start,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            label.SetBinding(Label.TextProperty, "Text");

            label.SizeChanged += (s, e) =>
            {
                bubble.WidthRequest = label.Width + 34; // Account for left/right padding + tail
                bubble.HeightRequest = label.Height + 26; // Account for top/bottom padding + tail extension
            };

            container.Children.Add(bubble);
            container.Children.Add(label);

            Children.Add(container);
            Grid.SetRow(container, 0);
        }
    }
}
