using Microsoft.Maui.Graphics;

namespace ChatSample
{
    public class OutgoingChatBubbleDrawable : IDrawable
    {
        public string Text { get; set; } = "";
        public Color BubbleColor { get; set; } = Color.FromArgb("#007AFF");
        public Color TextColor { get; set; } = Colors.White;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // iOS iMessage style bubble with smooth curved tail at bottom-right
            var path = new PathF();
            
            float cornerRadius = 18f;
            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float tailWidth = 10f;
            float tailHeight = 6f;

            // Start from top-left corner
            path.MoveTo(cornerRadius, 0);
            
            // Top edge to top-right corner
            path.LineTo(width - cornerRadius, 0);
            path.QuadTo(width, 0, width, cornerRadius);
            
            // Right edge down to just above the tail
            path.LineTo(width, height - cornerRadius - tailHeight);
            
            // Smooth curve into the tail (iOS style)
            path.CurveTo(
                width, height - tailHeight - 2,          // Control point 1
                width - 2, height - tailHeight,          // Control point 2
                width - 5, height - tailHeight + 1);     // End point before tail
            
            // Create the curved tail pointing bottom-right
            path.CurveTo(
                width - 3, height - 3,                   // Control point 1
                width + 1, height - 1,                   // Control point 2 (extends right)
                width, height);                          // Tail tip
            
            // Curve back from tail tip into bubble
            path.CurveTo(
                width - 2, height,                       // Control point 1
                width - tailWidth, height - 2,           // Control point 2
                width - tailWidth - 2, height - tailHeight + 1); // Back to bubble
            
            // Bottom edge to bottom-left corner
            path.LineTo(cornerRadius, height - tailHeight);
            path.QuadTo(0, height - tailHeight, 0, height - tailHeight - cornerRadius);
            
            // Left edge up to top-left corner
            path.LineTo(0, cornerRadius);
            path.QuadTo(0, 0, cornerRadius, 0);
            
            path.Close();

            // Fill the bubble
            canvas.FillColor = BubbleColor;
            canvas.FillPath(path);
        }
    }

    public class IncomingChatBubbleDrawable : IDrawable
    {
        public string Text { get; set; } = "";
        public Color BubbleColor { get; set; } = Color.FromArgb("#E5E5EA");
        public Color TextColor { get; set; } = Colors.Black;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // iOS iMessage style bubble with smooth curved tail at bottom-left
            var path = new PathF();
            
            float cornerRadius = 18f;
            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float tailWidth = 10f;
            float tailHeight = 6f;

            // Start from top-left corner
            path.MoveTo(cornerRadius, 0);
            
            // Top edge to top-right corner
            path.LineTo(width - cornerRadius, 0);
            path.QuadTo(width, 0, width, cornerRadius);
            
            // Right edge down to bottom-right corner
            path.LineTo(width, height - tailHeight - cornerRadius);
            path.QuadTo(width, height - tailHeight, width - cornerRadius, height - tailHeight);
            
            // Bottom edge to just before the tail
            path.LineTo(tailWidth + 2, height - tailHeight + 1);
            
            // Curve from bubble into tail
            path.CurveTo(
                tailWidth, height - 2,                   // Control point 1
                2, height,                               // Control point 2
                0, height);                              // Tail tip (extends left)
            
            // Curve back from tail tip
            path.CurveTo(
                -1, height - 1,                          // Control point 1
                3, height - 3,                           // Control point 2
                5, height - tailHeight + 1);             // Back to bubble
            
            // Smooth curve back into left edge
            path.CurveTo(
                2, height - tailHeight,                  // Control point 1
                0, height - tailHeight - 2,              // Control point 2
                0, height - tailHeight - cornerRadius);  // Left edge
            
            // Left edge up to top-left corner
            path.LineTo(0, cornerRadius);
            path.QuadTo(0, 0, cornerRadius, 0);
            
            path.Close();

            // Fill the bubble
            canvas.FillColor = BubbleColor;
            canvas.FillPath(path);
        }
    }
}
