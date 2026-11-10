// Original generic device pictograms for Tailscale Auto.
// Copyright (c) 2026 MiSimple. Licensed under the MIT License.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

class GenerateGenericIcons {
    static GraphicsPath RoundRect(RectangleF r, float radius) {
        var p = new GraphicsPath(); float d = radius * 2;
        p.AddArc(r.Left, r.Top, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Top, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure(); return p;
    }
    static void Save(string path, Action<Graphics, Brush, Pen> draw) {
        using (var image = new Bitmap(128, 128, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(image))
        using (var brush = new SolidBrush(Color.FromArgb(40, 153, 208)))
        using (var pen = new Pen(brush, 6)) {
            g.Clear(Color.Transparent); g.SmoothingMode = SmoothingMode.AntiAlias;
            draw(g, brush, pen); image.Save(path, ImageFormat.Png);
        }
    }
    static void Main(string[] args) {
        if (args.Length != 1) throw new ArgumentException("Expected output directory.");
        Directory.CreateDirectory(args[0]);
        Save(Path.Combine(args[0], "windows.png"), (g, b, p) => {
            using (var frame = RoundRect(new RectangleF(12, 20, 104, 70), 7)) g.DrawPath(p, frame);
            g.FillRectangle(b, 58, 91, 12, 14);
            using (var stand = RoundRect(new RectangleF(39, 105, 50, 7), 3)) g.FillPath(b, stand);
        });
        Save(Path.Combine(args[0], "ios.png"), (g, b, p) => {
            using (var shell = RoundRect(new RectangleF(33, 8, 62, 112), 12)) g.DrawPath(p, shell);
            g.DrawLine(p, 40, 29, 88, 29); g.DrawLine(p, 40, 100, 88, 100);
            using (var speaker = new Pen(b, 3)) g.DrawLine(speaker, 57, 19, 71, 19);
            g.FillEllipse(b, 61, 107, 6, 6);
        });
        Save(Path.Combine(args[0], "android.png"), (g, b, p) => {
            using (var shell = RoundRect(new RectangleF(9, 27, 110, 74), 10)) g.DrawPath(p, shell);
            using (var screen = RoundRect(new RectangleF(20, 38, 83, 52), 3)) g.DrawPath(p, screen);
            g.FillEllipse(b, 108, 61, 5, 5);
        });
    }
}
