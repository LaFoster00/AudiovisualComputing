using Unity.Serialization.Json;

public interface IPersistentData
{
    public object Serialize();
    public void Deserialize(object Data);
}