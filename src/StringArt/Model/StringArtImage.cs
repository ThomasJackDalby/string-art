using StringArt.Tools;
using System.Drawing;

namespace StringArt.Model
{
    public record StringArtImage(int Radius, int NumberOfPins, int[] Pins, int[]? Data = null)
    {
        public double AngleDelta { get; } = 2 * Math.PI / NumberOfPins;

        public void SaveExact(string filePath)
        {
            Bitmap image = new(2 * Radius, 2 * Radius);
            Graphics graphics = Graphics.FromImage(image);
            graphics.FillRectangle(Brushes.White, 0, 0, image.Width, image.Height);
            Vector2D[] positions = CalculatePinPositions().ToArray();
            Vector2D previous = positions[Pins[0]];
            for (int i=1;i<Pins.Length;i++)
            {
                Vector2D next = positions[Pins[i]];
                graphics.DrawLine(Pens.Black, (float)previous.X, (float)previous.Y, (float)next.X, (float)next.Y);
                previous = next;
            }
            image.Save(filePath);
        }
        //public void SaveVirtual(string filePath)
        //{
        //    Bitmap image = new(2 * radius, 2 * radius);
        //    for (int x = 0; x < 2 * radius; x++)
        //    {
        //        for (int y = 0; y < 2 * radius; y++)
        //        {
        //            int index = y * 2 * radius + x;
        //            int value = data[index];
        //            image.SetPixel(x, y, Color.FromArgb(value, value, value));
        //        }
        //    }
        //    image.Save(filePath);
        //}
        public void SavePins(string filePath)
        {
            if (!filePath.EndsWith(".pins")) filePath = $"{filePath}.pins";
            File.WriteAllText(filePath, String.Join(',', Pins.Select(p => p.ToString())));
        }

        public Vector2D CalculatePinPosition(int pinIndex)
        {
            double angle = pinIndex * AngleDelta;
            double x = Radius * (1 + Math.Cos(angle));
            double y = Radius * (1 + Math.Sin(angle));
            return new Vector2D(x, y);
        }
        public IEnumerable<Vector2D> CalculatePinPositions()
        {
            for (int pin = 0; pin < NumberOfPins; pin++) yield return CalculatePinPosition(pin);
        }


        public static StringArtImage LoadPins(string filePath, int radius, int numberOfPins)
        {
            int[] pins = File.ReadAllText(filePath)
                .Trim()
                .Split(",")
                .Select(p => Int32.Parse(p))
                .ToArray();

            return new StringArtImage(radius, numberOfPins, pins, null);
        }
    }
}