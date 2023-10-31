using StringArt.Tools;

namespace StringArt.Processing
{
    public interface IStringScoreCalculator
    {
        public byte GetScore(double distance);
        public string GetKey();
    }

    public static class StringScoreCalculatorExtensions
    {
        public static byte GetScore(this IStringScoreCalculator self, Vector2D point, Line2D line)
        {
            double distance = Math.Abs(point.GetPerpendicularDistanceTo(line));
            return self.GetScore(distance);
        }
    }

    public class StepStringScoreCalculator : IStringScoreCalculator
    {
        private readonly double x1;
        private readonly double y1;

        public StepStringScoreCalculator(double x1 = 1.0, double y1 = 25.0)
        {
            this.x1 = x1;
            this.y1 = y1;
        }

        public string GetKey() => $"step-{x1}-{y1}";
        public byte GetScore(double distance)
        {
            if (distance <= x1) return (byte)y1;
            return 0;
        }
    }

    public class RampStringScoreCalculator : IStringScoreCalculator
    {
        private readonly double x2;
        private readonly double x1;
        private readonly double y2;
        private readonly double y1;

        public RampStringScoreCalculator(double x1 = 1.0, double y1 = 25.0, double x2 = 2.0, double y2 = 0.0)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }

        public string GetKey() => $"ramp-{x1}-{y1}-{x2}-{y2}";
        public byte GetScore(double distance)
        {
            if (distance < x1) return (byte)y1;
            if (distance > x2) return (byte)y2;
            byte score = (byte)((y2 - y1) / (x2 - x1) * (distance - x1) + y1);
            return score;
        }
    }
}