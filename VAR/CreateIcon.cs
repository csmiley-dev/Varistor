using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace VAR
{
    // This is a utility class to create the Varistor icon
    // Run this once to generate the icon file
    public class CreateIcon
    {
        public static void GenerateVaristorIcon(string outputPath)
        {
            int size = 256; // Icon size
            using (Bitmap bitmap = new Bitmap(size, size))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw varistor symbol
                using (Pen pen = new Pen(Color.FromArgb(70, 130, 180), 12))
                {
                    pen.LineJoin = LineJoin.Round;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    // Draw box shape (varistor body)
                    Rectangle rect = new Rectangle(60, 80, 136, 96);
                    g.DrawRectangle(pen, rect);

                    // Draw connecting lines
                    g.DrawLine(pen, 20, 128, 60, 128); // Left line
                    g.DrawLine(pen, 196, 128, 236, 128); // Right line
                }

                // Draw dollar sign instead of diagonal line
                using (Font font = new Font("Arial", 80, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(34, 139, 34)))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("$", font, brush, new RectangleF(60, 80, 136, 96), sf);
                }

                // Save as PNG first
                string pngPath = outputPath.Replace(".ico", ".png");
                bitmap.Save(pngPath, ImageFormat.Png);

                // Convert to ICO format
                using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    // ICO header
                    bw.Write((short)0); // Reserved
                    bw.Write((short)1); // Type (1 = ICO)
                    bw.Write((short)1); // Number of images

                    // Image directory entry
                    bw.Write((byte)size); // Width (0 = 256)
                    bw.Write((byte)size); // Height (0 = 256)
                    bw.Write((byte)0);    // Color palette
                    bw.Write((byte)0);    // Reserved
                    bw.Write((short)1);   // Color planes
                    bw.Write((short)32);  // Bits per pixel

                    // Save PNG to memory stream to get size
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        byte[] pngData = ms.ToArray();

                        bw.Write((int)pngData.Length);  // Size of image data
                        bw.Write((int)22);              // Offset of image data

                        // Write PNG data
                        bw.Write(pngData);
                    }
                }

                Console.WriteLine($"Icon created successfully at: {outputPath}");
            }
        }

        // Uncomment and run this to generate the icon
        /*
        static void Main()
        {
            string iconPath = Path.Combine(Directory.GetCurrentDirectory(), "varistor.ico");
            GenerateVaristorIcon(iconPath);
        }
        */
    }
}
