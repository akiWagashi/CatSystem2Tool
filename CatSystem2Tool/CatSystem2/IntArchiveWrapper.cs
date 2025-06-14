using System.Diagnostics;
using System.Text.RegularExpressions;
using CatSystem2.Archive.Int;

namespace CatSystem2.Wrapper;
public class IntArchiveWrapper
{

    public static void ExtractArchiveResource(string targetArchive, string extractDirectory, string nameMappingStr, Regex filter)
    {
        Stopwatch watch = new Stopwatch();

        watch.Start();

        using FileStream archiveStream = File.OpenRead(targetArchive);

        using BinaryReader archiveReader = new BinaryReader(archiveStream);

        byte[] header = archiveReader.ReadBytes(4);

        if(!header.SequenceEqual(IntArchive.Header))
        {
            Console.WriteLine("ERROR : the file not CatSystem2 int format archive");
            return;
        }

        uint entryCount = archiveReader.ReadUInt32() - 1;

        IndexEntry keyEntry = new IndexEntry(archiveReader.ReadBytes(0x48));

        if (!keyEntry.Name.AsSpan(0,IntArchive.KeyEntryName.Length).SequenceEqual(IntArchive.KeyEntryName)) 
        {
            Console.WriteLine("ERROR : archive first entry is not __key__.dat");
            return;
        }

        uint mtSeed = keyEntry.Size;

        IntArchiveDecrypter archiveDecrypter = new IntArchiveDecrypter(mtSeed, nameMappingStr.ToArray());

        List<IndexEntry> entries = new List<IndexEntry>((int)entryCount);

        //decrypt index entries
        for (uint i = 1; i <= entryCount; ++i) 
        {
            IndexEntry entry = new IndexEntry(archiveReader.ReadBytes(IndexEntry.EntrySize));

            archiveDecrypter.DecryptEntry(entry, i);

            entries.Add(entry);
        }

        if (Path.GetDirectoryName(extractDirectory) is string && !Directory.Exists(extractDirectory)) Directory.CreateDirectory(extractDirectory);

        uint extractCount = 0;

        List<Task> tasks = new List<Task>(entries.Count);

        //extract resources by filter
        foreach (var entry in entries) 
        {

            string fileName = IntArchive.EntryEncoding.GetString(entry.Name).TrimEnd('\x0');

            if (!filter.IsMatch(fileName)) continue;

            archiveReader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

            byte[] data = archiveReader.ReadBytes((int)entry.Size);

            var task = Task.Run(() =>
            {

                archiveDecrypter.DecryptData(data);

                using FileStream entryStream = File.Create(Path.Combine(extractDirectory, fileName));

                entryStream.Write(data);

                Interlocked.Increment(ref extractCount);

            }).ContinueWith(task =>
            {

                if (task.IsFaulted)
                {
                    Console.WriteLine($"ERROR : Extract resource {fileName}  failed , because {task.Exception.Message}");
                }
                else
                {
                    Console.WriteLine($"SUCCESS :Extract resource {fileName}");
                }

            });

            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());

        watch.Stop();

        Console.WriteLine($"INFO : extract files {extractCount} / {entryCount} in {watch.ElapsedMilliseconds} ms");

    }

    public static void CreateResourceArchive(string resourceDirectory, string createArchivePath, string nameMappingStr, string seedStr)  
    {
        FileInfo[] fileInfos = new DirectoryInfo(resourceDirectory).GetFiles();

        if(fileInfos.Length <= 0) 
        {
            Console.WriteLine("ERROR : the directory haven't file");
            return;
        }

        Stopwatch watch = new Stopwatch();

        watch.Start();

        string archiveDirectory = Path.GetDirectoryName(createArchivePath) ?? throw new ArgumentException("Invalid path format");

        if(!Directory.Exists(archiveDirectory)) Directory.CreateDirectory(archiveDirectory);

        using FileStream archiveStream = File.Create(createArchivePath);

        using BinaryWriter archiveWriter = new BinaryWriter(archiveStream);

        archiveWriter.Write(IntArchive.Header);     //write archive header
        archiveWriter.Write(fileInfos.Length + 1);     //write index entry count

        IndexEntry keyEntry = new IndexEntry();

        IntArchive.KeyEntryName.CopyTo(keyEntry.Name, 0);

        //i don't know what it means at 0x40 in the key entry, maybe some type of checksum?
        uint dataMtSeed = 0;
        if(!string.IsNullOrEmpty(seedStr) && Util.TryParseInt(seedStr, out dataMtSeed)) 
        {
            keyEntry.Size = dataMtSeed;
        }
        else 
        {
            keyEntry.Size = (uint)new Random().Next();
            dataMtSeed = keyEntry.Size;
        }

        

        //write key entry
        archiveWriter.Write(keyEntry.Name);
        archiveWriter.Write(keyEntry.Offset);
        archiveWriter.Write(keyEntry.Size);

        IntArchiveEncrypter archiveEncrypter = new IntArchiveEncrypter(dataMtSeed, nameMappingStr.ToArray());

        List<IndexEntry> entries = new List<IndexEntry>(fileInfos.Length + 1);
        entries.Add(keyEntry);

        uint importCount = 0;
        uint offset = (uint)(((fileInfos.Length + 2) * IndexEntry.EntrySize) + 0x8);      //+2 means two key entries, the second key entry maybe an end flag for the archive index?

        //encrypt and write index entry
        foreach (FileInfo fileInfo in fileInfos)  
        {
            IndexEntry entry = new IndexEntry();
            IntArchive.EntryEncoding.GetBytes(fileInfo.Name).CopyTo(entry.Name, 0);
            entry.Offset = offset;
            entry.Size = (uint)fileInfo.Length;

            offset = (uint)(offset + fileInfo.Length + IndexEntry.EntrySize);

            archiveEncrypter.EncryptEntry(entry, (uint)entries.Count);

            archiveWriter.Write(entry.Name);
            archiveWriter.Write(entry.Offset);
            archiveWriter.Write(entry.Size);

            entries.Add(entry);
        }

        //write encrypted index entry (maybe an end flag?)
        //encrypt and write file data
        for(int i = 0; i< fileInfos.Length; ++i) 
        {
            archiveWriter.Write(entries[i].Name);
            archiveWriter.Write(entries[i].Offset);
            archiveWriter.Write(entries[i].Size);

            byte[] entryData = File.ReadAllBytes(fileInfos[i].FullName);

            archiveEncrypter.EncryptData(entryData);
            archiveWriter.Write(entryData);

            importCount++;
        }

        watch.Stop();

        Console.WriteLine($"INFO : import files {importCount}/{fileInfos.Length} to {Path.GetFileName(createArchivePath)} in {watch.ElapsedMilliseconds} ms ");
    } 
}
