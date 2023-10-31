using StringArt.Tools;
using System.Drawing;

namespace StringArt.Model
{
    public class PinMapSet
    {
        public int NumberOfPins { get; }
        public int Radius { get; }

        private readonly Dictionary<(int, int), (int, byte)[]> data;

        public PinMapSet(int numberOfPins, int radius, Dictionary<(int, int), (int, byte)[]> data)
        {
            NumberOfPins = numberOfPins;
            Radius = radius;
            this.data = data;
        }

        public (int, byte)[] GetMap(int pinA, int pinB)
        {
            (int, int) coords = getPinCoordinates(pinA, pinB);
            return data[coords];
        }

        public static void SaveImages(string folderPath, PinMapSet pinMap)
        {
            Directory.CreateDirectory(folderPath);

            for (int startPin = 0; startPin < pinMap.NumberOfPins; startPin++)
            {
                for (int endPin = startPin + 1; endPin < pinMap.NumberOfPins; endPin++)
                {
                    (int, byte)[] data = pinMap.GetMap(startPin, endPin);

                    Bitmap image = new Bitmap(2 * pinMap.Radius, 2 * pinMap.Radius);
                    Graphics g = Graphics.FromImage(image);
                    g.FillRectangle(Brushes.White, 0, 0, 2 * pinMap.Radius, 2 * pinMap.Radius);

                    for (int i = 0; i < data.Length; i++)
                    {
                        (int index, byte delta) = data[i];
                        int x = index % (pinMap.Radius * 2);
                        int y = (index - x) / (pinMap.Radius * 2);
                        image.SetPixel(x, y, Color.FromArgb(255, 255 - delta, 255 - delta, 255 - delta));
                    }

                    {
                        double angle = startPin * 2 * Math.PI / pinMap.NumberOfPins;
                        Vector2D start = new Vector2D(pinMap.Radius * (1 + Math.Cos(angle)), pinMap.Radius * (1 + Math.Sin(angle)));
                        g.DrawEllipse(Pens.Red, (float)(start.X - 2), (float)(start.Y - 2), 4, 4);
                    }
                    {
                        double angle = endPin * 2 * Math.PI / pinMap.NumberOfPins;
                        Vector2D position = new Vector2D(pinMap.Radius * (1 + Math.Cos(angle)), pinMap.Radius * (1 + Math.Sin(angle)));
                        g.DrawEllipse(Pens.Blue, (float)(position.X - 2), (float)(position.Y - 2), 4, 4);
                    }

                    image.Save($"{folderPath}/{startPin}-{endPin}.png");
                }
            }
        }

        private static (int, int) getPinCoordinates(int pinA, int pinB) => pinA < pinB
            ? (pinA, pinB)
            : (pinB, pinA);
    }
}