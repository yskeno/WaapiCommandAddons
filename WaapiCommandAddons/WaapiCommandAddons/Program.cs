using System;
using System.Linq;

namespace AK.Wwise.Waapi
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    class Program
    {
        static void usage()
        {
            System.Console.WriteLine(@"
WaapiCommandAddons <command> <option>

Command:
    -h, --help                                              Show help.

    -x, --exe <Wwise Info> <application path>               Execute application with wwise info as argument.
                                                            (Cf.) WaapiCommandAddons -x ExternalApplication.exe -pn
        Wwise Info:
            -pn, --projname                                 WwiseProject Name(.wproj filename)

    -r, --remote <Connect/Disconnect> <option:IP Address>   Remote connection functions(default is ""Connect to Localhost"")
        Connect/Disconnect:
            -c, --connect                                   Connect to Localhost or specific host(need following <ip address> option)
                <option:IP Address>                             IP address to connect.
            -d, --disconnect                                Disconnect from current session.

    -p, --reaper                                            Open Reaper project.
");
            return;
        }

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            if ((args.Length == 0) || args.Contains<string>("-h") || args.Contains<string>("--help"))
            {
                usage();
                return;
            }
            else
            {
                try
                {
                    AK.Wwise.Waapi.dotNetJsonClient client = new AK.Wwise.Waapi.dotNetJsonClient();

                    // Try to connect to running instance of Wwise on localhost, default port
                    await client.Connect().ConfigureAwait(false);
                    // Register for connection lost event
                    client.Disconnected += () => System.Console.WriteLine("We lost connection!");

                    if (args[0] == "-r" || args[0] == "--remote")
                    {
                        if (args[1] == "-d" || args[1] == "--disconnect")
                            await WaapiCommandFunction.DisconnectFromHost(client).ConfigureAwait(false);
                        else
                            await WaapiCommandFunction.ConnectToHost(client).ConfigureAwait(false);
                    }
                    else if (args[0] == "-c" || args[0] == "--convert")
                    {
                        await WaapiCommandFunction.ConvertContainerType(client, args[1]).ConfigureAwait(false);
                    }
                    else if (args[0] == "-x" || args[0] == "--execute")
                    {
                        if (args[1].StartsWith("-"))
                        {
                            string ExecAppPath = String.Join(" ", args[2..]);
                            await WaapiCommandFunction.ExecuteExternalApplication(client, ExecAppPath, args[1]).ConfigureAwait(false);
                        }
                        else
                        {
                            System.Console.WriteLine("ERROR: Invalid <Wwise Info> switch.");
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Console.Error.WriteLine("\n\n***  ERROR: Unhandled Exception ***");
                    System.Console.Error.WriteLine("Message:");
                    System.Console.Error.WriteLine(e.Message + "\n");
                    System.Console.Error.WriteLine("StackTrace:");
                    System.Console.Error.WriteLine(e.StackTrace + "\n");
                }
                finally
                {
                    System.Diagnostics.Debug.WriteLine("\n**** End of Program ***\n");
                }
            }
            return;
        }
    }
}
