using StringArt.Model;
using StringArt.Processing;
using System.Drawing;

namespace StringArt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Single("johny.jpg", 300);
        }

        public static void Single(string filePath, int numberOfPins)
        {
            StringArtSolver solver = new(numberOfPins, filePath, new RampStringScoreCalculator(1, 15, 2, 0));
            solver.PinsCompletedChanged += (numberOfStrings, minPin) =>
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"{numberOfStrings} ({minPin})");
            };

            string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
            string outputPinsFilePath = $"{fileNameNoExt}.pins";

            StringArtImage result = solver.Evaluate();
            Console.WriteLine("Solved!");
            result.SavePins(outputPinsFilePath);

            string outputImageFilePath = $"{fileNameNoExt}-exact.png";
            result.SaveExact(outputImageFilePath);
        }
    }
}