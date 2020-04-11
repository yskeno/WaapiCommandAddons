using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;

namespace AK.Wwise.Waapi
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "<Pending>")]
    static class WaapiCommandFunction
    {
        /// <summary>
        /// Check whether Wwise already connect to any host.
        /// </summary>
        /// <param name="client">AK.Wwise.Waapi.dotNetJsonClient instance.</param>
        /// <returns>true: already connected to, false: not connected to</returns>
        private static async Task<bool> HasRemoteConnected(dotNetJsonClient client)
        {
            System.Diagnostics.Debug.WriteLine("--- HasRemoteConnected() ---");

            var ConnectionStatus = await client.Call(ak.wwise.core.remote.getConnectionStatus).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("ConnectionStatus:\n" + ConnectionStatus);

            if (ConnectionStatus.Value<bool>("isConnected") == true)
            {
                return true;
            }
            return false;

        }

        /// <summary>
        /// Connect to host(if no available console found or already connected to, show warning log).
        /// </summary>
        /// <param name="client">AK.Wwise.Waapi.dotNetJsonClient instance.</param>
        /// <param name="ExcludeEditor">Exclude application including string "Editor".</param>
        /// <param name="IpAddress">Specific IP address you want to connect. If null, connect to Localhost.</param>
        /// <returns>Error: -1, Success: Other int</returns>
        public static async Task<int> ConnectToHost(dotNetJsonClient client, bool ExcludeEditor = false, string IpAddress = null)
        {
            System.Diagnostics.Debug.WriteLine("*** ConnectToHost() ***");

            // Check has already connected to some application
            if (await HasRemoteConnected(client).ConfigureAwait(false))
                return -1;

            var AvailableConsoles = await client.Call(ak.wwise.core.remote.getAvailableConsoles).ConfigureAwait(false);

            if (AvailableConsoles == null)
            {
                // TODO: Show Error window for none of available consoles.
                System.Console.WriteLine("ERROR: Not found any available console!");
                return -1;
            }

            System.Diagnostics.Debug.WriteLine("\nAvailableConsoles:\n" + AvailableConsoles);

            JObject ArgConnect;
            if (ExcludeEditor)
            {
                ArgConnect = (from acs in AvailableConsoles["consoles"]
                              where acs["host"].ToString() == (IpAddress ?? "127.0.0.1")
                              where !(acs["appName"].Contains("Editor"))
                              select new JObject { { "host", acs["host"] }, { "commandPort ", acs["commandPort "] } }).FirstOrDefault();
            }
            else
            {
                ArgConnect = (from acs in AvailableConsoles["consoles"]
                              where acs["host"].ToString() == (IpAddress ?? "127.0.0.1")
                              select new JObject { { "host", acs["host"] }, { "commandPort", acs["commandPort"] } }).FirstOrDefault();
            }

            System.Diagnostics.Debug.WriteLine("\nArgConnect:\n" + ArgConnect);

            if (ArgConnect == null)
            {
                System.Console.WriteLine("ERROR: Not found Console on Localhost!");
                return -1;
            }

            await client.Call(ak.wwise.core.remote.connect, ArgConnect).ConfigureAwait(false);

            return 0;
        }

        /// <summary>
        /// Disconnect from Local remote host(if no connection found, nothing to do).
        /// </summary>
        /// <param name="client">AK.Wwise.Waapi.dotNetJsonClient instance.</param>
        /// <param name="ExcludeEditor">Exclude application including string "Editor".</param>
        /// <param name="IpAddress">Specific IP address you want to connect. If null, connect to Localhost.</param>
        /// <returns>Error: -1, Success: Other int</returns>
        public static async Task<int> DisconnectFromHost(dotNetJsonClient client)
        {
            System.Diagnostics.Debug.WriteLine("*** DisconnectFromHost() ***");

            if (!(await HasRemoteConnected(client).ConfigureAwait(false)))
            {
                System.Console.WriteLine("No Connection Found.\n");
                return -1;
            }

            await client.Call(ak.wwise.core.remote.disconnect).ConfigureAwait(false);

            return 0;
        }

        /// <summary>
        /// Get informatino about Wwise project.
        /// </summary>
        /// <param name="client">AK.Wwise.Waapi.dotNetJsonClient instance.</param>
        /// <param name="ArgSwitch">Information type which you need.</param>
        /// <returns>Wwise information as string.</returns>
        private static async Task<string> GetWwiseInfo(dotNetJsonClient client, string ArgSwitch)
        {
            System.Diagnostics.Debug.WriteLine("--- GetWwiseInfo() ---");

            switch (ArgSwitch)
            {
                case ("-pn"):
                case ("--projname"):
                    System.Diagnostics.Debug.WriteLine("- Get Project Name -");

                    //JObject ArgGetProjName = JObject.Parse(@"{ from : { ofType : [ 'Project' ] } }");
                    JObject ArgGetProjName = new JObject{
                                                        {"from",new JObject{
                                                                {"ofType",new JArray{
                                                                                    "Project"}}}}};
                    System.Diagnostics.Debug.WriteLine("\nArgGetProjName:\n" + ArgGetProjName);

                    //JObject OptGetProjName = JObject.Parse(@"{ return : [ 'name' ] }");
                    JObject OptGetProjName = new JObject { { "return", new JArray { "name" } } };
                    System.Diagnostics.Debug.WriteLine("\nOptGetProjName:\n" + OptGetProjName);

                    var ProjInfo = await client.Call(ak.wwise.core.@object.get, ArgGetProjName, OptGetProjName).ConfigureAwait(false);

                    System.Diagnostics.Debug.WriteLine("\nProjectName:\n" + (string)ProjInfo["return"][0]["name"]);
                    return (string)ProjInfo["return"][0]["name"];

                default:
                    return null;
            }
        }
        /// <summary>
        /// Execute external application with Wwise info as argument.
        /// </summary>
        /// <param name="client">AK.Wwise.Waapi.dotNetJsonClient instance.</param>
        /// <param name="ExecAppPath">Application path to execute.</param>
        /// <param name="ArgSwitch">Information type which you need.</param>
        /// <returns>Error: -1, Success: Other int</returns>
        public static async Task<int> ExecuteExternalApplication(dotNetJsonClient client, string ExecAppPath = null, string ArgSwitch = null)
        {
            System.Diagnostics.Debug.WriteLine("*** ExecuteExternalApplication() ***");

            if (ExecAppPath == null)
            {
                System.Console.WriteLine("ERROR: Not found application to execute!");
                return -1;
            }

            if (ArgSwitch == null)
            {
                System.Console.WriteLine("ERROR: Invalid <Wwise Info> argument switch.");
                return -1;
            }

            ArgSwitch = await GetWwiseInfo(client, ArgSwitch).ConfigureAwait(false);

            var ExecuteCommand = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = ExecAppPath,
                Arguments = ArgSwitch
            };

            Process.Start(ExecuteCommand);
            return 0;
        }

        public static async Task<int> ConvertContainerType(dotNetJsonClient client, string ConvertTo = null)
        {
            System.Diagnostics.Debug.WriteLine("*** ConvertContainerType() ***");

            // Get Selected Object Info
            var OptSelectedObjInfo = JObject.Parse("{ \"return\": [ \"" +
                                                        (String.Join("\", \"", typeof(WwiseObjectsReference).GetProperties().Select(f => f.Name).ToArray())).Replace("\"_", "\"@") +
                                                                    "\" ] }");
            System.Diagnostics.Debug.WriteLine("\nOptSelectedObjInfo:\n" + OptSelectedObjInfo);

            var SelectedObjInfo = (JObject)(await client.Call(ak.wwise.ui.getSelectedObjects,
                                                              null,
                                                              OptSelectedObjInfo).ConfigureAwait(false))["objects"][0];
            System.Diagnostics.Debug.WriteLine("\nSelectedObjInfo:\n" + SelectedObjInfo);

            var NestedProperties = new[] { "parent", "@OutputBus", "@UserAuxSend0", "@UserAuxSend1", "@UserAuxSend2", "@UserAuxSend3", "@ReflectionsAuxSend",
                                           "@Conversion", "@Effect0", "@Effect1", "@Effect2", "@Effect3", "@Attenuation", "@AttachableMixerInput",
                                           /* "@DefaultSwitchOrState", "@SwitchGroupOrStateGroup" */};

            foreach (var NestedProperty in NestedProperties)
            {
                SelectedObjInfo[NestedProperty] = SelectedObjInfo[NestedProperty]["id"];
            }

            // Remove '@' and ignore DefaultValue items.
            // And add '@' again to use argments of object creation.
            // Make Call argument as JObject
            var ArgConvertTo = JObject.Parse(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<WwiseObjectsReference>(SelectedObjInfo.ToString().Replace("\"@", "\"_")),
                                                                          Formatting.Indented,
                                                                          new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }).Replace("\"_", "\"@"));

            // Modify info to use args for object creation
            ArgConvertTo.Add("onNameConflict", "rename");

            switch (ConvertTo)
            {
                case ("ActorMixer"):
                    if ((ArgConvertTo["type"]?.ToString() ?? "None") == "ActorMixer")
                        return 0;

                    ArgConvertTo["@RandomOrSequence"]?.Parent.Remove();
                    ArgConvertTo["type"] = "ActorMixer";
                    break;

                case ("BlendContainer"):
                    if ((ArgConvertTo["type"]?.ToString() ?? "None") == "BlendContainer")
                        return 0;

                    ArgConvertTo["@RandomOrSequence"]?.Parent.Remove();
                    ArgConvertTo["type"] = "BlendContainer";
                    break;

                case ("RandomContainer"):

                    if ((ArgConvertTo["type"]?.ToString() ?? "None") == "RandomSequenceContainer" &&
                        (ArgConvertTo["@RandomOrSequence"]?.ToString() ?? "None") == "1")
                        return 0;

                    if (ArgConvertTo["@RandomOrSequence"] == null)
                        ArgConvertTo.Add("@RandomOrSequence", 0);
                    else
                        ArgConvertTo["@RandomOrSequence"] = 0;

                    ArgConvertTo["@RandomOrSequence"] = 1;
                    ArgConvertTo["type"] = "RandomSequenceContainer";
                    break;

                case ("SequenceContainer"):
                    if ((ArgConvertTo["type"]?.ToString() ?? "None") == "RandomSequenceContainer" &&
                        (ArgConvertTo["@RandomOrSequence"]?.ToString() ?? "None") == "0")
                        return 0;

                    if (ArgConvertTo["@RandomOrSequence"] == null)
                        ArgConvertTo.Add("@RandomOrSequence", 0);
                    else
                        ArgConvertTo["@RandomOrSequence"] = 0;

                    ArgConvertTo["type"] = "RandomSequenceContainer";
                    break;

                case ("SwitchContainer"):
                    if ((ArgConvertTo["type"]?.ToString() ?? "None") == "SwitchContainer")
                        return 0;

                    ArgConvertTo["@RandomOrSequence"]?.Parent.Remove();
                    ArgConvertTo["type"] = "SwitchContainer";
                    break;

                default:
                    return 0;

            }
            System.Diagnostics.Debug.WriteLine("\nArgConvertTo:\n" + ArgConvertTo);

            await client.Call(ak.wwise.core.@object.create, ArgConvertTo, null).ConfigureAwait(false);

            return 0;
        }
    }
}
