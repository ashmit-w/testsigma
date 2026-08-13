using Puppet.Cli;

if (args.Length > 0 && args[0] == "dump")
{
    return await DumpCommand.Run(args[1..]);
}

if (args.Length > 0 && args[0] == "model")
{
    return await ModelCommand.Run(args[1..]);
}

if (args.Length > 0 && args[0] == "session")
{
    return await SessionCommand.Run(args[1..]);
}

if (args.Length > 0 && args[0] == "palette")
{
    return await PaletteCommand.Run(args[1..]);
}

Console.Error.WriteLine("Usage:");
Console.Error.WriteLine("  puppet dump --process <name> --out <path.json> [--summary]");
Console.Error.WriteLine("  puppet model --process <name> --out <path.json> [--merge] [--pid <n>]");
Console.Error.WriteLine("  puppet session --exe <path.exe>");
Console.Error.WriteLine("  puppet palette --model <path.json> --out <path.json>");
return 1;
