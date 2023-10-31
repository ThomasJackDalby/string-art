using StringArt.IO;
using StringArt.Model;
using StringArt.Processing;
using StringArt.Tools;
using System.Drawing;

namespace StringArt
{
    public class StringArtSolver
    {
        private static readonly ILogger logger = LoggingManager.GetLogger<StringArtSolver>();

        public event Action<int, int>? PinsCompletedChanged;

        private readonly int numberOfPins;
        private readonly int size;
        private readonly int radius;
        private readonly int[] currentData;
        private readonly byte[] targetData;
        private readonly IStringScoreCalculator scoreCalculator;
        private readonly IPinMapSerialiser pinMapSerialiser;

        public StringArtSolver(
            int numberOfPins,
            string imageFilePath,
            IStringScoreCalculator? scoreCalculator = null)
        {
            this.numberOfPins = numberOfPins;

            this.scoreCalculator = scoreCalculator ?? new RampStringScoreCalculator(1, 10, 3, 0);
            pinMapSerialiser = new BinaryPinMapSerializer();

            using Bitmap sourceImage = new Bitmap(imageFilePath).MakeGrayscale3();

            radius = Math.Min(sourceImage.Width, sourceImage.Height) / 2;
            size = 4 * radius * radius;

            int xOffset = (sourceImage.Width - 2 * radius) / 2;
            int yOffset = (sourceImage.Height - 2 * radius) / 2;
            using Bitmap croppedImage = sourceImage.Crop(2 * radius, 2 * radius, xOffset, yOffset);

            targetData = croppedImage.ExtractByteArray();
            currentData = new int[size];
        }

        public StringArtImage Evaluate()
        {
            int[] pins = EvaluatePins().ToArray();
            return new StringArtImage(radius, numberOfPins, pins, currentData);
        }
        public IEnumerable<int> EvaluatePins()
        {
            PinMapSet pinMapSet = getPinMapSet(radius, numberOfPins);

            logger.Log("Solving...");
            int startPin = 0;
            yield return startPin;

            for (int i = 0; i < size; i++) currentData[i] = 255; // start with a blank image

            int numberOfStrings = 0;
            while (true)
            {
                int minErrorDelta = Int32.MaxValue;
                int minPin = -1;

                // pre-calculate the delta and error to save doing it each time
                int[] currentDelta = new int[size];
                for (int i = 0; i < size; i++) currentDelta[i] = targetData[i] - currentData[i];

                // find the best string to add, from the fixed start location
                for (int endPin = 0; endPin < numberOfPins; endPin++)
                {
                    if (endPin == startPin) continue; // don't loop to self

                    // work out what the error would be if added
                    (int, byte)[] pinDelta = pinMapSet.GetMap(startPin, endPin);

                    int errorDelta = 0;
                    foreach ((int i, byte delta) in pinDelta)
                    {
                        // only need to sum the error for indexes the pin will change
                        int currentError = Math.Abs(currentDelta[i]);
                        int error = Math.Abs(currentDelta[i] + delta);
                        errorDelta += error - currentError;
                    }

                    // if the error is the current best fit, store it.
                    // the error delta is less than zero if the error for the test pin is less than the current error.
                    if (errorDelta < 0 && (minPin == -1 || errorDelta < minErrorDelta))
                    {
                        minPin = endPin;
                        minErrorDelta = errorDelta;
                    }
                }

                if (minPin != -1)
                {
                    // apply the pin to the data
                    (int, byte)[] pinDelta = pinMapSet.GetMap(startPin, minPin);
                    foreach ((int i, byte delta) in pinDelta) currentData[i] = Math.Max(0, currentData[i] - delta);

                    PinsCompletedChanged?.Invoke(numberOfStrings++, minPin);
                    yield return minPin;
                    startPin = minPin;
                }
                else yield break;
            }
        }
        public IEnumerable<int> EvaluatePinsAsync()
        {
            PinMapSet pinMapSet = getPinMapSet(radius, numberOfPins);

            logger.Log("Solving...");
            int startPin = 0;
            yield return startPin;

            for (int i = 0; i < size; i++) currentData[i] = 255; // start with a blank image

            int numberOfStrings = 0;
            while (true)
            {
                // pre-calculate the delta and error to save doing it each time
                int[] currentDelta = new int[size];
                for (int i = 0; i < size; i++) currentDelta[i] = targetData[i] - currentData[i];

                // find the best string to add, from the fixed start location
                (int minPin, int errorDelta) = Enumerable.Range(0, numberOfPins)
                    .AsParallel()
                    .Select(endPin =>
                    {
                        if (endPin == startPin) return (-1, 0); // don't loop to self

                        // work out what the error would be if added
                        (int, byte)[] pinDelta = pinMapSet.GetMap(startPin, endPin);

                        int errorDelta = 0;
                        foreach ((int i, byte delta) in pinDelta)
                        {
                            // only need to sum the error for indexes the pin will change
                            int currentError = Math.Abs(currentDelta[i]);
                            int error = Math.Abs(currentDelta[i] + delta);
                            errorDelta += error - currentError;
                        }

                        return (endPin, errorDelta);
                    })
                    .Aggregate((a, b) =>
                    {
                        if (a.Item1 == -1) return b;
                        if(b.Item1 == -1) return a;
                        // return most negative error delta 
                        return a.Item2 < b.Item2 ? a : b;
                    });

                if (minPin != -1)
                {
                    // apply the pin to the data
                    (int, byte)[] pinDelta = pinMapSet.GetMap(startPin, minPin);
                    foreach ((int i, byte delta) in pinDelta) currentData[i] = Math.Max(0, currentData[i] - delta);

                    PinsCompletedChanged?.Invoke(numberOfStrings++, minPin);
                    yield return minPin;
                    startPin = minPin;
                }
                else yield break;
            }
        }
        public int[] GetData() => currentData;
        public int GetRadius() => radius;

        private PinMapSet getPinMapSet(int radius, int numberOfPins)
        {
            logger.Log($"Getting pin map set for [{radius}] {numberOfPins}");
            string filePath = $"{radius}_{numberOfPins}_{scoreCalculator.GetKey()}.bin";
            PinMapSet? pinMapSet = null;
            if (File.Exists(filePath))
            {
                logger.Log($"Pin map already evaluated. Loading from file [{filePath}].");
                pinMapSet = pinMapSerialiser.Deserialize(filePath);
            }
            if (pinMapSet is null)
            {
                logger.Log("Pin map does not exist. Evaluating...");

                PinMapEvaluator evaluator = new(scoreCalculator, radius, numberOfPins);
                ConsoleProgressBar progressBar = new();
                evaluator.Progressed += progressBar.Update;

                pinMapSet = evaluator.Evaluate();
                Console.WriteLine();
                pinMapSerialiser.Serialize(filePath, pinMapSet);
            }
            return pinMapSet;
        }
    }
}