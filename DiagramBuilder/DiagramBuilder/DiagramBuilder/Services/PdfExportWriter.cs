using System.Globalization;
using System.Text;

namespace DiagramBuilder.Services;

public static class PdfExportWriter
{
    private const double PageWidth = 612;
    private const double PageHeight = 792;

    public static byte[] FromJpeg(byte[] jpeg)
    {
        var (imageWidth, imageHeight) = ReadJpegSize(jpeg);
        var scale = Math.Min(PageWidth / imageWidth, PageHeight / imageHeight);
        var drawWidth = imageWidth * scale;
        var drawHeight = imageHeight * scale;
        var x = (PageWidth - drawWidth) / 2;
        var y = (PageHeight - drawHeight) / 2;
        var content = Encoding.ASCII.GetBytes(string.Format(
            CultureInfo.InvariantCulture,
            "q {0:0.###} 0 0 {0:0.###} {1:0.###} {2:0.###} cm /Im0 Do Q",
            scale, x, y));

        using var output = new MemoryStream();
        var offsets = new List<long> { 0 };
        WriteAscii(output, "%PDF-1.4\n%----\n");

        WriteObject(output, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
        WriteObject(output, offsets, 2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        WriteObject(output, offsets, 3,
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /XObject << /Im0 4 0 R >> >> /Contents 5 0 R >>");

        offsets.Add(output.Position);
        WriteAscii(output, $"4 0 obj\n<< /Type /XObject /Subtype /Image /Width {imageWidth} /Height {imageHeight} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {jpeg.Length} >>\nstream\n");
        output.Write(jpeg);
        WriteAscii(output, "\nendstream\nendobj\n");

        offsets.Add(output.Position);
        WriteAscii(output, $"5 0 obj\n<< /Length {content.Length} >>\nstream\n");
        output.Write(content);
        WriteAscii(output, "\nendstream\nendobj\n");

        var xrefOffset = output.Position;
        WriteAscii(output, $"xref\n0 {offsets.Count}\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1))
        {
            WriteAscii(output, $"{offset:0000000000} 00000 n \n");
        }

        WriteAscii(output,
            $"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return output.ToArray();
    }

    private static (int Width, int Height) ReadJpegSize(byte[] jpeg)
    {
        if (jpeg.Length < 4 || jpeg[0] != 0xFF || jpeg[1] != 0xD8)
        {
            throw new InvalidDataException("The diagram export did not return a valid JPEG image.");
        }

        var index = 2;
        while (index + 3 < jpeg.Length)
        {
            if (jpeg[index++] != 0xFF)
            {
                continue;
            }

            while (index < jpeg.Length && jpeg[index] == 0xFF)
            {
                index++;
            }

            if (index >= jpeg.Length)
            {
                break;
            }

            var marker = jpeg[index++];
            if (marker is 0xD9 or 0xDA)
            {
                break;
            }

            if (index + 1 >= jpeg.Length)
            {
                break;
            }

            var segmentLength = (jpeg[index] << 8) | jpeg[index + 1];
            if (segmentLength < 2 || index + segmentLength > jpeg.Length)
            {
                break;
            }

            if (marker is >= 0xC0 and <= 0xC3 or >= 0xC5 and <= 0xC7 or >= 0xC9 and <= 0xCB or >= 0xCD and <= 0xCF)
            {
                if (segmentLength < 7)
                {
                    break;
                }

                var height = (jpeg[index + 3] << 8) | jpeg[index + 4];
                var width = (jpeg[index + 5] << 8) | jpeg[index + 6];
                if (width > 0 && height > 0)
                {
                    return (width, height);
                }
            }

            index += segmentLength;
        }

        throw new InvalidDataException("The diagram JPEG does not contain image dimensions.");
    }

    private static void WriteObject(Stream stream, List<long> offsets, int number, string body)
    {
        offsets.Add(stream.Position);
        WriteAscii(stream, $"{number} 0 obj\n{body}\nendobj\n");
    }

    private static void WriteAscii(Stream stream, string value) =>
        stream.Write(Encoding.ASCII.GetBytes(value));
}
