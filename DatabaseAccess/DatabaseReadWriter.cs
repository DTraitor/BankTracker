using System.Text.Json;

namespace DatabaseAccess;

public class DatabaseReadWriter<T> : IDisposable {
    public DatabaseReadWriter(string file)
    {
        stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        Read();
    }

    public DatabaseReadWriter(FileStream stream)
    {
        streamOwned = false;
        this.stream = stream;
        Read();
    }

    public void Dispose()
    {
        Save();
        if (streamOwned)
            stream.Dispose();
    }

    private void Read()
    {
        stream.Position = 0;
        using StreamReader reader = new StreamReader(stream, leaveOpen: true);
        try
        {
            BankList = JsonSerializer.Deserialize<List<T>>(reader.ReadToEnd()) ?? new List<T>();
        }
        catch (JsonException e)
        {
            BankList = new List<T>();
        }
    }

    public void Save()
    {
        stream.SetLength(0);
        using StreamWriter writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(JsonSerializer.Serialize(BankList));
    }

    private bool streamOwned = true;
    public List<T> BankList { get; private set; }
    private readonly FileStream stream;
}