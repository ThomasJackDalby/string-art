using StringArt.Model;
using StringArt.Processing;
using StringArt.Tools;

namespace StringArt
{
    public class PinMapEvaluator
    {
        private static readonly ILogger logger = LoggingManager.GetLogger<PinMapEvaluator>();

        public event Action<int, int>? Progressed;

        private int current = 0;
        private readonly int radius;
        private readonly int numberOfPins;
        private readonly int total;
        private readonly IStringScoreCalculator calculator;
        private readonly Vector2D[] positions;

        public PinMapEvaluator(IStringScoreCalculator calculator, int radius, int numberOfPins)
        {
            this.radius = radius;
            this.numberOfPins = numberOfPins;
            this.calculator = calculator;
            positions = evaluatePinPositions(radius, numberOfPins).ToArray();

            total = numberOfPins * (numberOfPins - 1) / 2;
        }

        public PinMapSet Evaluate()
        {
            Dictionary<(int, int), (int, byte)[]> pinData = Enumerable.Range(0, numberOfPins)
                .AsParallel()
                .SelectMany(startPin => Enumerable.Range(startPin + 1, numberOfPins - startPin - 1)
                .Select(endPin => evaluatePin(startPin, endPin)))
                .ToDictionary(result => (result.Item1, result.Item2), result => result.Item3);

            //Task<(int, int, (int, byte)[])>[] tasks = Enumerable.Range(0, numberOfPins)
            //    .SelectMany(startPin => Enumerable.Range(startPin + 1, numberOfPins - startPin - 1)
            //        .Select(endPin => Task.Run(() => evaluatePin(startPin, endPin))))
            //        .ToArray();

            //Task.WaitAll(tasks);

            //Dictionary<(int, int), (int, byte)[]> pinData = tasks
            //    .Select(task => task.Result)
            //    .ToDictionary(result => (result.Item1, result.Item2), result => result.Item3);

            return new PinMapSet(numberOfPins, radius, pinData);
        }

        private (int, int, (int, byte)[]) evaluatePin(int startPin, int endPin)
        {
            Vector2D start = positions[startPin];
            Vector2D end = positions[endPin];
            Line2D line = new(start, end - start);
            (int minX, int maxX, int minY, int maxY) = getLimits(start, end, 2 * radius, 2 * radius);
            List<(int, byte)> data = new();
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    int index = y * 2 * radius + x;
                    byte score = calculator.GetScore(new Vector2D(x, y), line);
                    if (score != 0) data.Add((index, score));
                }
            }
            if (Progressed is not null) Progressed(current++, total);
            return (startPin, endPin, data.ToArray());
        }

        private static IEnumerable<Vector2D> evaluatePinPositions(int radius, int numberOfPins)
        {
            logger.Log("Caching pin positions.");
            double angle_delta = 2 * Math.PI / numberOfPins;
            for (int pin = 0; pin < numberOfPins; pin++)
            {
                double angle = pin * angle_delta;
                double x = radius * (1 + Math.Cos(angle));
                double y = radius * (1 + Math.Sin(angle));
                yield return new Vector2D(x, y);
            }
        }

        private static (int, int, int, int) getLimits(Vector2D start, Vector2D end, int width, int height, int margin = 5)
        {
            int minX = (int)Math.Max(Math.Min(start.X - margin, end.X - margin), 0);
            int maxX = (int)Math.Min(Math.Max(start.X + margin, end.X + margin), width);
            int minY = (int)Math.Max(Math.Min(start.Y - margin, end.Y - margin), 0);
            int maxY = (int)Math.Min(Math.Max(start.Y + margin, end.Y + margin), height);
            return (minX, maxX, minY, maxY);
        }
    }
}