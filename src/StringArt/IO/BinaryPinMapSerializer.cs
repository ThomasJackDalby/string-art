using StringArt.Model;

namespace StringArt.IO
{
    public class BinaryPinMapSerializer : IPinMapSerialiser
    {
        private static readonly ILogger logger = LoggingManager.GetLogger<BinaryPinMapSerializer>();

        public PinMapSet Deserialize(string filePath)
        {
            logger.Log($"Deserializing pin map set from [{filePath}].");

            using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new(stream);

            int radius = reader.ReadInt32();
            int numberOfPins = reader.ReadInt32();
            int maxIndex = 4 * radius * radius;

            Func<int> read = maxIndex < short.MaxValue
                ? () => reader.ReadInt16()
                : reader.ReadInt32;

            Dictionary<(int, int), (int, byte)[]> data = new();
            for (int startPin = 0; startPin < numberOfPins; startPin++)
            {
                for (int endPin = startPin + 1; endPin < numberOfPins; endPin++)
                {
                    int length = reader.ReadInt32();
                    (int, byte)[] pinDelta = new (int, byte)[length];
                    for (int i = 0; i < length; i++)
                    {
                        int index = read();
                        byte delta = reader.ReadByte();
                        pinDelta[i] = (index, delta);
                    }
                    data[(startPin, endPin)] = pinDelta;
                }
            }
            return new PinMapSet(radius, numberOfPins, data);
        }
        public void Serialize(string filePath, PinMapSet pinMapSet)
        {
            logger.Log($"Serializing pin map set from [{filePath}].");
            using Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new(stream);
            writer.Write(pinMapSet.Radius);
            writer.Write(pinMapSet.NumberOfPins);

            int maxIndex = 4 * pinMapSet.Radius * pinMapSet.Radius;
            Action<int> write = maxIndex < short.MaxValue
                ? index => writer.Write((short)index)
                : index => writer.Write(index);

            for (int startPin = 0; startPin < pinMapSet.NumberOfPins; startPin++)
            {
                for (int endPin = startPin + 1; endPin < pinMapSet.NumberOfPins; endPin++)
                {
                    (int, byte)[] data = pinMapSet.GetMap(startPin, endPin);
                    writer.Write(data.Length);
                    for (int i = 0; i < data.Length; i++)
                    {
                        (int index, byte delta) = data[i];
                        write(index);
                        writer.Write(delta);
                    }
                }
            }
        }
    }
}