using CatSystem2.Algorithm;
using System.Text;

namespace CatSystem2.Archive.Int;

public abstract class IntArchive
{

    public static readonly byte[] Header = { 0x4B, 0x49, 0x46, 0x0 }; //KIF

    public static readonly byte[] KeyEntryName = { 0x5F, 0x5F, 0x6B, 0x65, 0x79, 0x5F, 0x5F, 0x2E, 0x64, 0x61, 0x74, 0x0 };   //__key__.dat

    public static readonly Encoding EntryEncoding = Encoding.UTF8;

    protected uint BaseKey { get; init; } = 0x1234;

    protected byte[] CharMap { get; init; } = new byte[0x34];

    internal BlowFish BlowFish { get; init; }

    public IntArchive(uint dataMtSeed, char[] chars)
    {
        BaseKey = (uint)BuildKey(chars);
        InitCharMap();

        BlowFish = new BlowFish(new uint[] { new MTwister(dataMtSeed).Rand() });
    }

    protected int BuildKey(char[] chars)
    {
        int result = -1;

        if (chars.Length == 0) return result;

        foreach (char item in chars)
        {
            int temp = item << 0x18 ^ result;

            for (int i = 0; i < 8; ++i) temp = temp >= 0 ? temp * 2 : temp * 2 ^ 0x4C11DB7;

            result = ~temp;
        }

        return result;
    }

    protected void InitCharMap()
    {
        for (uint i = 0; i < 0x1A; ++i) CharMap[i] = (byte)(i + 0x41);

        for (uint i = 0x1A; i < 0x34; ++i) CharMap[i] = (byte)(i + 0x47);

        Array.Reverse(CharMap);
    }

    protected void TranslationName(IndexEntry entry, uint index)
    {
        uint randNum = new MTwister(BaseKey + index).Rand();

        randNum = randNum + (randNum >> 0x8) + (randNum >> 0x10) + (randNum >> 0x18);

        for (uint c = 0, charMapIndex = (byte)randNum; entry.Name[c] != 0; ++c)
        {
            for (uint i = 0; i < 0x34; ++i)
            {

                if (entry.Name[c] == CharMap[(charMapIndex + i) % 0x34])
                {
                    entry.Name[c] = CharMap[0x33 - i];
                    break;
                }

            }

            charMapIndex++;

        }
    }
}

