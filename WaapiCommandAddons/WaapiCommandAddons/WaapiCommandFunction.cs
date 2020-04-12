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
using System.Runtime.CompilerServices;

namespace AK.Wwise.Waapi
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "<Pending>")]
    static class WaapiCommandFunction
    {

#if DEBUG
        static Stopwatch stopwatch = new Stopwatch();
#endif // DEBUG

        [Conditional("DEBUG")]
        public static void DEBUG_STARTTIME(string msg = null)
        {
            if (msg != null)
                msg = msg + ": ";

            WaapiCommandFunction.stopwatch.Restart();
            System.Diagnostics.Debug.WriteLine("====== STOPWATCH: " + msg + WaapiCommandFunction.stopwatch.ElapsedMilliseconds + " msec");
        }
        [Conditional("DEBUG")]
        public static void DEBUG_LAPTIME(string msg = null)
        {
            if (msg != null)
                msg = msg + ": ";
            System.Diagnostics.Debug.WriteLine("====== STOPWATCH: " + msg + WaapiCommandFunction.stopwatch.ElapsedMilliseconds + " msec");
        }
        [Conditional("DEBUG")]
        public static void DEBUG_STOPTIME(string msg = null)
        {
            if (msg != null)
                msg = msg + ": ";
            WaapiCommandFunction.stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine("====== STOPWATCH: " + msg + WaapiCommandFunction.stopwatch.ElapsedMilliseconds + " msec");
        }


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

        /// <summary>
        /// Get all modified properties of selected single object as JSON string.
        /// </summary>
        /// <param name="client">AK.Wwise.Waapi.dotNetJsonClient instance.</param>
        /// <returns>All modified propertiws of selected object as JSON string.</returns>
        private static async Task<string> GetSelectedObjectInfo(dotNetJsonClient client)
        {
            System.Diagnostics.Debug.WriteLine("*** GetSelectedObjectInfo() ***");
            // Get Selected Object Info
            var OptSelectedObjInfo = JObject.Parse("{ \"return\": [ \"" +
                                            (String.Join("\", \"", typeof(WwiseObjectsReference).GetProperties().Select(f => f.Name).ToArray())).Replace("\"_", "\"@") +
                                                        "\" ] }");
            System.Diagnostics.Debug.WriteLine("\nOptSelectedObjInfo:\n" + OptSelectedObjInfo);

            DEBUG_LAPTIME("End OptSelectedObjInfo");

            var SelectedObjInfo = (JObject)(await client.Call(ak.wwise.ui.getSelectedObjects,
                                                              null,
                                                              OptSelectedObjInfo).ConfigureAwait(false))["objects"][0];
            System.Diagnostics.Debug.WriteLine("\nSelectedObjInfo:\n" + SelectedObjInfo);

            DEBUG_LAPTIME("End SelectedObjInfo");

            var NestedProperties = new[] { "parent", "@OutputBus", "@UserAuxSend0", "@UserAuxSend1", "@UserAuxSend2", "@UserAuxSend3", "@ReflectionsAuxSend",
                                           "@Conversion", "@Effect0", "@Effect1", "@Effect2", "@Effect3", "@Attenuation", "@AttachableMixerInput",
                                           /* "@DefaultSwitchOrState", "@SwitchGroupOrStateGroup" */};

            foreach (var NestedProperty in NestedProperties)
            {
                SelectedObjInfo[NestedProperty] = SelectedObjInfo[NestedProperty]["id"];
            }

            DEBUG_LAPTIME("End NestedProperties");

            // Remove '@' and ignore DefaultValue items.
            // Then, add '@' again to use argments of object creation.
            return JsonConvert.SerializeObject(JsonConvert.DeserializeObject<WwiseObjectsReference>(SelectedObjInfo.ToString().Replace("\"@", "\"_")),
                                                                          Formatting.Indented,
                                                                          new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }).Replace("\"_", "\"@");
        }

        private static async Task<string> GetChildrenObject(dotNetJsonClient client, string path)
        {
            var GetChildrenInfo = await client.Call(ak.wwise.core.@object.get, path, null).ConfigureAwait(false);
            return GetChildrenInfo?.ToString();
        }

        /// <summary>
        /// Convert ContainerType.
        /// </summary>
        /// <param name="client">AK.Wwise.Waapi.dotNetJsonClient instance.</param>
        /// <param name="ConvertTo">Container type you want to convert to.</param>
        /// <returns>Error: -1, Success: Other int</returns>
        public static async Task<int> ConvertContainerType(dotNetJsonClient client, string ConvertTo = null)
        {
            DEBUG_STARTTIME("Start ConvertContainerType()");
            System.Diagnostics.Debug.WriteLine("*** ConvertContainerType() ***");

            // Get Selected Object Info
            var SelectedObjInfo = JObject.Parse(await GetSelectedObjectInfo(client).ConfigureAwait(false));
            DEBUG_LAPTIME("Start DeepCopy() SelectedObjInfo->ArgConvertTo");
            JObject ArgConvertTo = (JObject)SelectedObjInfo.DeepClone();
            DEBUG_LAPTIME("End DeepCopy() SelectedObjInfo->ArgConvertTo");
            System.Diagnostics.Debug.WriteLine("*** " + (SelectedObjInfo == ArgConvertTo));

            DEBUG_LAPTIME("End GetSelectedObjectInfo()");

            // Modify info to use args for object creation
            ArgConvertTo.Remove("id");
            ArgConvertTo.Remove("childrenCount");
            ArgConvertTo.Add("onNameConflict", "rename");

            switch (ConvertTo)
            {
                case ("ActorMixer"):
                case ("BlendContainer"):
                case ("SwitchContainer"):
                    if ((ArgConvertTo["type"]?.ToString() ?? "None") == ConvertTo)
                    {
                        System.Diagnostics.Debug.WriteLine("Convert is stopped. Selected Container type is same as ConvertTo.");
                        return 0;
                    }
                    ArgConvertTo["@RandomOrSequence"]?.Parent.Remove();
                    ArgConvertTo["type"] = ConvertTo;
                    break;

                case ("RandomContainer"):
                case ("SequenceContainer"):
                    if ((ArgConvertTo["type"]?.ToString() ?? "None") == "RandomSequenceContainer")
                        if (((ConvertTo == "SequenceContainer") && (ArgConvertTo["@RandomOrSequence"]?.ToString() == "0")) ||
                            ((ConvertTo == "RandomContainer") && (ArgConvertTo["@RandomOrSequence"]?.ToString() == "1")))
                        {
                            System.Diagnostics.Debug.WriteLine("Convert is stopped. Selected Container type is same as ConvertTo.");
                            return 0;
                        }

                    if (ArgConvertTo["@RandomOrSequence"] == null)
                    {
                        if (ConvertTo == "SequenceContainer")
                            ArgConvertTo.Add("@RandomOrSequence", 0);
                        else
                            ArgConvertTo.Add("@RandomOrSequence", 1);
                    }
                    else
                    {
                        if (ConvertTo == "SequenceContainer")
                            ArgConvertTo["@RandomOrSequence"] = 0;
                        else
                            ArgConvertTo["@RandomOrSequence"] = 1;
                    }
                    ArgConvertTo["type"] = "RandomSequenceContainer";
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine("No Argment of Containert Type for Convert to.");
                    return 0;
            }
            System.Diagnostics.Debug.WriteLine("\nArgConvertTo:\n" + ArgConvertTo);

            DEBUG_LAPTIME("Start ak.wwise.core.@object.create");

            var CreatedContainer = await client.Call(ak.wwise.core.@object.create, ArgConvertTo, null).ConfigureAwait(false);

            DEBUG_LAPTIME("End ak.wwise.core.@object.create");

            // Move children to new created container
            if (SelectedObjInfo["childrenCount"]?.ToString() == "0")
            {
                System.Diagnostics.Debug.WriteLine("No children found. Finish convert container successfully.");
                return 0;
            }

            // Argument & Option for get children's ID
            JObject ArgGetChildrenInfo = new JObject {
                {"from", new JObject{
                    {"id", new JArray{
                        SelectedObjInfo["id"]?.ToString()
                    }}}
                },
                {"transform",new JArray{
                    new JObject{
                        {"select", new JArray{
                            "children"
                        }}}}}
            };
            JObject OptGetChildrenInfo = new JObject{
                { "return", new JArray { "id" } }
            };
            System.Diagnostics.Debug.WriteLine("\nArgGetChildrenInfo:\n" + ArgGetChildrenInfo);
            System.Diagnostics.Debug.WriteLine("\nOptGetChildrenInfo:\n" + OptGetChildrenInfo);

            var ChildrenInfo = await client.Call(ak.wwise.core.@object.get, ArgGetChildrenInfo, OptGetChildrenInfo).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("\nGetChildrenInfo:\n" + ChildrenInfo);

            // Argument for move descendants to new container
            var ArgMoveTo = new JObject{
                { "parent", CreatedContainer["id"] },
                { "onNameConflict", "rename" }
            };
            System.Diagnostics.Debug.WriteLine("\nArgMoveTo:\n" + ArgMoveTo);

            foreach (var ChildInfo in ChildrenInfo["return"].Children<JObject>())
            {
                ArgMoveTo["object"] = ChildInfo["id"];
                System.Diagnostics.Debug.WriteLine("\nArgMoveTo:\n" + ArgMoveTo);
                await client.Call(ak.wwise.core.@object.move, ArgMoveTo, null).ConfigureAwait(false);
            }

            /*

            DEBUG_STOPTIME("End ConvertContainerType()");
            */

            return 0;
        }

        //public static async Task<int> MoveAllDescendants(AK.Wwise.Waapi.dotNetJsonClient client, JObject ParentContainer)
        //{
        //    await client.Call(ak.wwise.core.@object.create, null, null).ConfigureAwait(false);
        //    return 0;
        //}
    }
}
