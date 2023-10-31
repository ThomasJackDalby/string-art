using StringArt.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringArt.Processing
{
    public class GCodeExporter
    {
        private static readonly ILogger logger = LoggingManager.GetLogger<GCodeExporter>();


        public void Export(string filePath, StringArtImage image)
        {
            using Stream stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite);
            using GCodeWriter writer = new(stream);

            writer.CommentLine($"Exported @{DateTime.Now}");
            writer.CommentLine($"Number of pins: {image.NumberOfPins}");
            writer.CommentLine($"Number of lengths: {image.Pins.Length}");

            // G21 units in mm

            writer.G(21).G(30);

            // absolute units

            // G00 // rapid move

            // we could wrap the gcode writer in a class specific to the string art machine?

            double gearRadius = 300;
            double singlePinDistance = image.AngleDelta * gearRadius;

            double currentAngle = 0;
            foreach (int pin in image.Pins)
            {
                double pinAngle = pin * image.AngleDelta;

                double angleDelta = pinAngle - currentAngle;
                if (angleDelta > Math.PI) angleDelta -= 2 * Math.PI;
                else if (angleDelta < -Math.PI) angleDelta += 2 * Math.PI;
                

                // go in the direction 
                // rotate to just before the next pin
                // need to take into account direction


                writer.G00(y:5, comment: "move thread out");
                writer.G00(x: pin + 5, comment: "move board round");
                writer.G00(y: 0, comment: "move thread in");


            }
        }
    }

    public class GCodeWriter : IDisposable
    {
        private readonly StreamWriter writer;
        public GCodeWriter(Stream stream, string? title = null)
        {
            writer = new StreamWriter(stream);
            writer.WriteLine("%");
        }

        public GCodeWriter G(int code,
            double? x = null,
            double? y = null,
            double? z = null,
            double? i = null,
            double? j = null,
            double? f = null,
            string? comment = null)
        {
            writer.Write($"G{code:00}");
            if (x.HasValue) writer.Write($" X{x.Value:N3}");
            if (y.HasValue) writer.Write($" Y{y.Value:N3}");
            if (z.HasValue) writer.Write($" Z{z.Value:N3}");
            if (i.HasValue) writer.Write($" I{i.Value:N3}");
            if (j.HasValue) writer.Write($" J{j.Value:N3}");
            if (f.HasValue) writer.Write($" F{f.Value:N3}");
            if (comment is not null) writer.Write($" ; {comment}");
            writer.WriteLine();
            return this;
        }

        public GCodeWriter G20() => write("G20");
        public GCodeWriter G21() => write("G21").CommentLine("Units in mm");

        public GCodeWriter G00(
            double? x = null,
            double? y = null,
            double? z = null,
            double? f = null,
            string? comment = null)
            => G(0, x, y, z, f, comment: comment);

        public GCodeWriter CommentLine(string comment)
        {
            writer.WriteLine($"; {comment}");
            return this;
        }
        public void EndLine()
        {
            writer.WriteLine();
        }
        public void Dispose()
        {
            writer.WriteLine("%");
            writer.Dispose();
        }

        private GCodeWriter write(string value)
        {
            writer.Write(value);
            return this;
        }
    }

    // G90
    // G80

    //  G28, to rapid to the home position.
    //  G17, to select the x, y circular motion field.
    //  G20, to select the inch coordinate system. (G21, to select metric)
    //  G40, to cancel cutter compensation.
    //  G49, to cancel the cutter height compensation.
}
