namespace CatSystem2.Archive.Int;
public class IndexEntry
{
    public static readonly int EntrySize = 0x48;
    public byte[] Name { get; private set; } = new byte[0x40];

    public uint Offset = 0;

    public uint Size = 0;

    public IndexEntry() { }

    public IndexEntry(byte[] bytes)
    {
        Name = bytes[0..0x40];

        Offset = BitConverter.ToUInt32(bytes, 0x40);

        Size = BitConverter.ToUInt32(bytes, 0x44);

    }


}

