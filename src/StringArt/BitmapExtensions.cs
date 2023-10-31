using System.Drawing;
using System.Drawing.Imaging;

namespace StringArt
{
    public static class BitmapExtensions
    {
        private static readonly ColorMatrix colorMatrix = new ColorMatrix(
            new float[][]
            {
                new float[] {.3f, .3f, .3f, 0, 0},
                new float[] {.59f, .59f, .59f, 0, 0},
                new float[] {.11f, .11f, .11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });

        // https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale
        public static Bitmap MakeGrayscale3(this Bitmap image)
        {
            Bitmap greyImage = new(image.Width, image.Height);
            using Graphics g = Graphics.FromImage(greyImage);
            using ImageAttributes attributes = new();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(
                image,
                new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            return greyImage;
        }

        public static Bitmap Crop(this Bitmap original, int width, int height, int xOffset = 0, int yOffset = 0)
        {
            width = Math.Min(original.Width - xOffset, width);
            height = Math.Min(original.Height - yOffset, height);
            Bitmap bitmap = new(width, height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.DrawImage(original,
                new Rectangle(0, 0, width, height),
                xOffset, yOffset, width, height, GraphicsUnit.Pixel);
            return bitmap;
        }

        public static byte[] ExtractByteArray(this Bitmap image)
        {
            int index = 0;
            byte[] data = new byte[image.Width * image.Height];
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    data[index++] = image.GetPixel(x, y).R;
                }
            }
            return data;
        }
    }
}