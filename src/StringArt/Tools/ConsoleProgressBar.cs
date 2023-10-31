namespace StringArt.Tools
{
    public class ConsoleProgressBar
    {
        private readonly char cursor = '|';
        private double percentagePerCharUpdate = 2;
        private double percentagePerTextUpdate = 0.05;
        private double nextPercentage = 0;
        private double nextCharPercentage = 0;
        private int numberOfChars = 0;

        private readonly object key = new object();

        public ConsoleProgressBar()
        {
            nextPercentage = percentagePerTextUpdate;
            nextCharPercentage = percentagePerCharUpdate;
            Console.Write("0.00%");
        }

        public void Update(int current, int total)
        {
            lock (key)
            {
                double percentage = ((double)current / total) * 100;
                if (percentage >= nextCharPercentage)
                {
                    nextCharPercentage += percentagePerCharUpdate;
                    numberOfChars++;
                }
                if (percentage >= nextPercentage)
                {
                    nextPercentage += percentagePerTextUpdate;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{percentage:N2}% ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(new string(cursor, numberOfChars));
                }
            }
        }
    }
}