
using System.Runtime.InteropServices;

namespace CatSystem2.Archive.Int;
public class IntArchiveDecrypter : IntArchive
{
    public IntArchiveDecrypter(uint dataMtSeed, char[] chars) : base(dataMtSeed, chars) { }

    private void DecryptInfo(IndexEntry entry, uint index)
    {
        entry.Offset += index;
        BlowFish.Decrypt(ref entry.Offset, ref entry.Size);
    }

    public void DecryptEntry(IndexEntry entry, uint index)
    {
        DecryptInfo(entry, index);
        TranslationName(entry, index);
    }

    public void DecryptData(byte[] dataBuffer)
    {
        int encryptLength = dataBuffer.Length - (dataBuffer.Length % 8); //dataBuffer.Length & 7 == dataBuffer.Length % 8

        int decryptCount = encryptLength >> 3;

        Span<uint> block = MemoryMarshal.Cast<byte, uint>(dataBuffer);

        for (int i = 0; i < decryptCount; ++i) BlowFish.Decrypt(ref block[i * 2], ref block[i * 2 + 1]);
    }
}


