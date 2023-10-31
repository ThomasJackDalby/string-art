namespace StringArt.Tools
{
    public record struct Vector2D(double X, double Y)
    {
        public double GetDistanceTo(Vector2D other)
            => (other - this).Abs();
        public static double Determinant(Vector2D a, Vector2D b)
            => a.X * b.Y - a.Y * b.X;
        public double GetPerpendicularDistanceTo(Line2D line)
        {
            Vector2D ap = line.Origin - this;
            return Determinant(ap, line.Direction) / line.Direction.Abs();
        }
        public double Abs()
            => Math.Sqrt(X * X + Y * Y);
        public static Vector2D operator +(Vector2D self, Vector2D other)
            => new Vector2D(self.X + other.X, self.Y + other.Y);
        public static Vector2D operator -(Vector2D self, Vector2D other)
            => new Vector2D(self.X - other.X, self.Y - other.Y);
    }
}