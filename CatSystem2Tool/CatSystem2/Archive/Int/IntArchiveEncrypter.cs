

using System.Runtime.InteropServices;

namespace CatSystem2.Archive.Int;
public class IntArchiveEncrypter : IntArchive
{
    public IntArchiveEncrypter(uint dataMtSeed, char[] chars) : base(dataMtSeed, chars) { }

    private void EncryptInfo(IndexEntry entry, uint index) 
    {
        BlowFish.Encrypt(ref entry.Size, ref entry.Offset);
        entry.Offset -= index;
    }

    public void EncryptEntry(IndexEntry entry, uint index)  
    {
        EncryptInfo(entry, index);
        TranslationName(entry, index);
    }

    public void EncryptData(byte[] dataBuffer) 
    {
        int encryptLength = dataBuffer.Length - (dataBuffer.Length % 8);

        int encryptCount  = encryptLength >> 3;

        Span<uint> block = MemoryMarshal.Cast<byte,uint>(dataBuffer);

        for (int i = 0; i < encryptCount; ++i) BlowFish.Encrypt(ref block[i * 2 + 1], ref block[i * 2]);
    }
}

