using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

int size = 256;
using (Bitmap bitmap = new Bitmap(size, size))
using (Graphics g = Graphics.FromImage(bitmap))
{
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.Clear(Color.White);

    // Draw varistor symbol background
    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(240, 240, 245)))
    {
        g.FillEllipse(bgBrush, 10, 10, size - 20, size - 20);
    }

    // Draw varistor symbol
    using (Pen pen = new Pen(Color.FromArgb(70, 130, 180), 14))
    {
        pen.LineJoin = LineJoin.Round;
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;

        // Draw box shape (varistor body)
        Rectangle rect = new Rectangle(70, 90, 116, 76);
        g.DrawRectangle(pen, rect);

        // Draw connecting lines
        g.DrawLine(pen, 30, 128, 70, 128); // Left line
        g.DrawLine(pen, 186, 128, 226, 128); // Right line
    }

    // Draw dollar sign
    using (Font font = new Font("Arial", 60, FontStyle.Bold))
    using (SolidBrush brush = new SolidBrush(Color.FromArgb(34, 139, 34)))
    {
        StringFormat sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString("$", font, brush, new RectangleF(70, 90, 116, 76), sf);
    }

    // Save as PNG
    string pngPath = Path.Combine(Directory.GetCurrentDirectory(), "varistor.png");
    bitmap.Save(pngPath, ImageFormat.Png);
    Console.WriteLine($"PNG created at: {pngPath}");
}
