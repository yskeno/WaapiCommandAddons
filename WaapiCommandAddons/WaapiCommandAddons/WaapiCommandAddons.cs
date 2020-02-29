using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace AK.Wwise.Waapi
{
    class WaapiCommandAddons
    {
        static void usage()
        {
            System.Console.WriteLine(@"
WwiseMultiFunctionalAddons [options]

General Options:
    -h, --help          Show help.
    -x, --exe           Execute application provided as argument.
                        (Cf.) --exe -pn ExternalApplication.exe
         [Arguments]
         -pn                Project Name
    -c, --connect       Connect to Localhost
    -d, --disconnect    Disconnect from current session.
    -u, --update        Update all sound assets.
");
            return;
        }

        static void Main(string[] args)
        {
            if ((args.Length == 0) || args.Contains<string>("-h") || args.Contains<string>("--help"))
            {
                usage();
            }

            waapiMain(null, args).Wait();

            System.Diagnostics.Debug.WriteLine("**** End of Program ***");
        }

        static async Task waapiMain(AK.Wwise.Waapi.JsonClient client, string[] args)
        {

            try
            {
                if (client == null)
                {
                    client = new AK.Wwise.Waapi.JsonClient();
                }
                if (!client.IsConnected())
                {
                    await client.Connect();
                }

                // Connect to Local remotehost ( if no available console found or already connected to, show warning log)
                if (args.Contains<string>("-c") || args.Contains<string>("--connect"))
                {
                    var connectionStatus = await client.Call(ak.wwise.core.remote.getConnectionStatus);

                    if (connectionStatus.Value<bool>("isConnected") == true)
                    {
                        System.Diagnostics.Debug.WriteLine(connectionStatus);
                        return;
                    }

                    JObject waapiArg_connect = new JObject(new JProperty("host", "127.0.0.1"));

                    var availableConsoles = await client.Call(ak.wwise.core.remote.getAvailableConsoles);

                    if (availableConsoles == null)
                    {
                        System.Console.WriteLine("WARNING: Not found any available console!");
                        return;
                    }

                    if (((JArray)availableConsoles["consoles"]).Count != 1)
                    {
                        var availableAppNames = from cs in availableConsoles["consoles"]
                                                select (string)cs["appName"];

                        foreach (var availableAppName in availableAppNames)
                        {
                            if (availableAppName.IndexOf("Editor", StringComparison.OrdinalIgnoreCase) != 0)
                                continue;

                            waapiArg_connect.Add("appName", availableAppName);
                        }
                    }

                    await client.Call(ak.wwise.core.remote.connect, waapiArg_connect);
                    return;
                }

                // Disconnect from Local remotehost ( if no connection found, nothing to do)
                else if (args.Contains<string>("-d") || args.Contains<string>("--disconnect"))
                {
                    var connectionStatus = await client.Call(ak.wwise.core.remote.getConnectionStatus);

                    if (connectionStatus.Value<bool>("isConnected") == false)
                    {
                        System.Diagnostics.Debug.WriteLine(connectionStatus);
                        return;
                    }

                    await client.Call(ak.wwise.core.remote.disconnect);
                    return;
                }

                if (args.Contains<string>("-u") || args.Contains<string>("--update"))
                {
                    Console.WriteLine("No action defined.");
                    return;
                }

                /*                // Execute external application with Wwise info as argument
                                if (args.Contains<string>("-x") || args.Contains<string>("--exe"))
                                {
                                    var executeCommand = new ProcessStartInfo();
                                    executeCommand.UseShellExecute = true;

                                    if (args.Contains<string>("-pn"))
                                    {
                                        JObject waapiArg_getProjectName = new JObject(
                                                                               new JProperty("from",
                                                                                  new JObject(new JProperty("ofType",
                                                                                      new JArray(new JValue("Project"))))));
                                        JObject waapOpt_getProjectName = new JObject(
                                                                               new JProperty("return",
                                                                                    new JArray(new JValue("name"))));

                                        var projectInfo = await client.Call(ak.wwise.core.@object.get, waapiArg_getProjectName, waapOpt_getProjectName);

                                        executeCommand.Arguments = (string)projectInfo["return"][0]["name"];

                                        //Console.WriteLine("projectName: {0}", exeArg);
                                    }
                                    else
                                    {
                                        Console.WriteLine("ERROR: No argument for -x(--exe) found!");
                                        return;
                                    }

                                    // array range method for C#8.0 or later.
                                    //executeCommand.FileName = String.Join(" ", args[2..]);

                                    if (executeCommand.FileName == null)
                                    {
                                        Console.WriteLine("Not found application to execute!");
                                        return;
                                    }

                                    Process.Start(executeCommand);
                                    return;
                                }*/
            }

            catch (Exception e)
            {
                System.Console.WriteLine("\n\n*** Unexpected Error Occuring ***");
                System.Console.Error.WriteLine(e.Message);
                System.Console.Error.WriteLine(e.StackTrace);
            }
        }
    }
}
