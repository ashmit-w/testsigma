using Puppet.Cli;

if (args.Length > 0 && args[0] == "dump")
{
    return await DumpCommand.Run(args[1..]);
}

Console.Error.WriteLine("Usage: puppet dump --process <name> --out <path.json> [--summary]");
return 1;
