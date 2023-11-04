using StringArt.Tools;

namespace Utils
{
    public record struct Vector2D(double X, double Y)
    {
        public double GetDistanceTo(Vector2D other)
            => (other - this).Abs();
        public double GetPerpendicularDistanceTo(Line2D line)
        {
            Vector2D ap = line.Origin - this;
            return ap.Determinant(line.Direction) / line.Direction.Abs();
        }
        public double Dot(Vector2D other) => X * other.X + Y * other.Y;
        public double Determinant(Vector2D other) => X * other.Y - Y * other.X;
        public double Abs()
            => Math.Sqrt(X * X + Y * Y);
        public static Vector2D operator +(Vector2D self, Vector2D other)
            => new Vector2D(self.X + other.X, self.Y + other.Y);
        public static Vector2D operator -(Vector2D self, Vector2D other)
            => new Vector2D(self.X - other.X, self.Y - other.Y);
    }
}