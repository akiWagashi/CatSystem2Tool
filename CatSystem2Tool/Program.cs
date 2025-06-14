
using System.Text.RegularExpressions;
using CatSystem2.Wrapper;
internal class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("Extarct Resources to directory : toolName -e <target archive> <output path> <name mapping string> [regex filter]");
            Console.WriteLine("Create Resources to archive : toolName -c <resources directory> <create archive path> <name mapping string> [data encrypt mtseed](e.g. 114514 or 0x1BF52)");
        }

        switch (args[0])
        {
            case "-e":
                {
                    IntArchiveWrapper.ExtractArchiveResource(args[1], args[2], args[3], new Regex(args.Length >= 5 ? args[4] : string.Empty));
                    break;
                }
            case "-c":
                {
                    IntArchiveWrapper.CreateResourceArchive(args[1], args[2], args[3], args.Length >= 5 ? args[4] : string.Empty);
                    break;
                }
        }
    }


}

