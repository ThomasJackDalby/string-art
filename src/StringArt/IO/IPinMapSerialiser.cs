using StringArt.Model;

namespace StringArt.IO
{
    public interface IPinMapSerialiser
    {
        void Serialize(string filePath, PinMapSet pinMapSet);
        PinMapSet Deserialize(string filePath);
    }
}