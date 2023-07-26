using Mono.Options;
using Xbox360Toolkit;

internal class Program
{
    private static bool shouldShowHelp = false;
    private static string input = string.Empty;
    private static string output = string.Empty;

    public static void LogLine(string line)
    {
        Console.WriteLine(line);
    }

    public static void Process()
    {
        var ccis = Directory.EnumerateFiles(input, "*.cci", SearchOption.AllDirectories).ToArray();
        for (int i = 0; i < ccis.Length; i++)
        {
            var file = ccis[i];
            LogLine($"Processing {Path.GetFileNameWithoutExtension(file)} - {i + 1} of {ccis.Length}");

            using (var containerReader = new CCIContainerReader(file))
            {
                if (containerReader.TryMount() == false)
                {
                    LogLine($"Failed to mount {Path.GetFileName(file)}");
                    continue;
                }

                if (containerReader.TryGetDefault(out var defaultData, out var containerType) == false)
                {
                    LogLine($"Failed to extract xex {Path.GetFileName(file)}");
                    continue;
                }

                var fileName = Path.GetFileNameWithoutExtension(file) + ".xex";
                var savePath = Path.Combine(output, fileName);
                File.WriteAllBytes(savePath, defaultData);
                LogLine("Extracted xex");
            }
        }

        LogLine("Done!");
    }

    private static void Main(string[] args)
    {
        var options = new OptionSet {
            { "i|input=", "the source path of CCI's.", i => input = i },
            { "o|output=", "the destination path for XEX's.", i => output = i },
            { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
        };

        try
        {
            List<string> extra = options.Parse(args);

            if (shouldShowHelp || args.Length == 0)
            {
                Console.WriteLine("MassXexExtractor: ");
                options.WriteOptionDescriptions(System.Console.Out);
                return;
            }

            if (string.IsNullOrEmpty(input) == true)
            {
                throw new OptionException("input path is invalid", "input");
            }

            input = Path.GetFullPath(input);
            if (Directory.Exists(input) == false)
            {
                throw new OptionException("input path does not exist.", "input");
            }

            if (string.IsNullOrEmpty(output) == true)
            {
                throw new OptionException("output path is invalid", "output");
            }

            output = Path.GetFullPath(output);
            if (Directory.Exists(output) == false)
            {
                throw new OptionException("output path does not exist.", "output");
            }

            if (input.Equals(output, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new OptionException("output path should not be same as input.", "output");
            }

            Process();
        }
        catch (OptionException e)
        {
            Console.Write("MassXexExtractor: ");
            Console.WriteLine(e.Message);
            Console.WriteLine("Try `MassXexExtractor --help' for more information.");
            return;
        }
    }
}