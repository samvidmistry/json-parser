namespace Parser;

/// <summary>
/// Abstract representation of a Json Object.
/// Object here does not imply the Object type
/// in JSON representation. JsonObject represents
/// any construct withing JSON format.
/// </summary>
public abstract class JsonObject {
    public abstract string Print();

    // trying to make sure all children
    // implement a ToString method
    public override string ToString()
    {
        return this.Print();
    }
}
