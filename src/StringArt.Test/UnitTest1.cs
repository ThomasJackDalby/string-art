using StringArt.Model;
using StringArt.Processing;

namespace StringArt.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            GCodeExporter exporter = new GCodeExporter();

            var image = new StringArtImage(300, 300, new[] { 0, 1, 2, 3, 4 });

            exporter.Export("test.gcode", image);
        }
    }
}