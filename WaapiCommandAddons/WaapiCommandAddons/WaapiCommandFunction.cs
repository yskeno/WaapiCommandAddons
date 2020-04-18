using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

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
            var ConnectionStatus = await client.Call(ak.wwise.core.remote.getConnectionStatus).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("ConnectionStatus:\n" + ConnectionStatus);

            if (ConnectionStatus.Value<bool>("isConnected") == true)
                return true;
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
            // Check has already connected to some application
            if (await HasRemoteConnected(client).ConfigureAwait(false))
                return -1;

            var AvailableConsoles = await client.Call(ak.wwise.core.remote.getAvailableConsoles).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("\nAvailableConsoles:\n" + AvailableConsoles);

            if (AvailableConsoles == null)
            {
                // TODO: Show Error window for none of available consoles.
                System.Console.WriteLine("ERROR: Not found any available console!");
                return -1;
            }

            JObject Arguments;
            if (ExcludeEditor)
            {
                Arguments = (from acs in AvailableConsoles["consoles"]
                             where acs["host"].ToString() == (IpAddress ?? "127.0.0.1")
                             where !(acs["appName"].Contains("Editor"))
                             select new JObject { { "host", acs["host"] }, { "commandPort ", acs["commandPort "] } }).FirstOrDefault();
            }
            else
            {
                Arguments = (from acs in AvailableConsoles["consoles"]
                             where acs["host"].ToString() == (IpAddress ?? "127.0.0.1")
                             select new JObject { { "host", acs["host"] }, { "commandPort", acs["commandPort"] } }).FirstOrDefault();
            }
            System.Diagnostics.Debug.WriteLine("\nArgConnect:\n" + Arguments);

            if (Arguments == null)
            {
                System.Console.WriteLine("ERROR: Not found Console on Localhost!");
                return -1;
            }

            await client.Call(ak.wwise.core.remote.connect, Arguments).ConfigureAwait(false);
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
            switch (ArgSwitch)
            {
                case ("-pn"):
                case ("--projname"):
                    JObject Arguments = JObject.Parse(@"{ from : { ofType : [ 'Project' ] } }");
                    System.Diagnostics.Debug.WriteLine("\nArguments:\n" + Arguments);

                    //JObject Options = JObject.Parse(@"{ return : [ 'name' ] }");
                    JObject Options = new JObject { { "return", new JArray { "name" } } };
                    System.Diagnostics.Debug.WriteLine("\nOptions:\n" + Options);

                    var ProjInfo = await client.Call(ak.wwise.core.@object.get, Arguments, Options).ConfigureAwait(false);
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
        public static async Task<int> ExecuteExternalApplication(dotNetJsonClient client, string FileName = null, string ArgSwitch = null)
        {
            if (FileName == null)
            {
                System.Console.WriteLine("ERROR: Not found application to execute!");
                return -1;
            }
            if (ArgSwitch == null)
            {
                System.Console.WriteLine("ERROR: Invalid <Wwise Info> argument switch.");
                return -1;
            }

            var Arguments = await GetWwiseInfo(client, ArgSwitch).ConfigureAwait(false);

            var ExecuteCommand = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = FileName,
                Arguments = Arguments
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
            // String for option from WwiseObjectsReference class variables.
            var WwiseObjectsReferenceList = typeof(WwiseObjectsReference).GetProperties().Select(f => f.Name).ToList();
            // Make list to match format of options for cliant.Call
            var ReturnedInfoList = new List<string>();
            // Change characters to fit Wwise call options.
            foreach (var itr in WwiseObjectsReferenceList)
            {
                if (itr.Contains("randomizer"))
                    ReturnedInfoList.Add(Regex.Replace(itr.Replace("_", "@"), @"(randomizer)(.+)(@.+)", "$1(\\\"$2\\\").$3"));
                else
                    ReturnedInfoList.Add(itr.Replace("_", "@"));
            }

            var Options = JObject.Parse("{ \"return\": [ \"" + String.Join("\", \"", ReturnedInfoList) + "\" ] }");
            System.Diagnostics.Debug.WriteLine("\nOptions_getSelectedObjects:\n" + Options);

            var SelectedObjInfo = (JObject)(await client.Call(ak.wwise.ui.getSelectedObjects,
                                                              null,
                                                              Options).ConfigureAwait(false))["objects"][0];
            System.Diagnostics.Debug.WriteLine("\nSelectedObjInfo:\n" + SelectedObjInfo);

            // Extract nested properties.
            var NestedProperties = new[] { "parent", "@OutputBus", "@UserAuxSend0", "@UserAuxSend1", "@UserAuxSend2", "@UserAuxSend3", "@ReflectionsAuxSend",
                                           "@Conversion", "@Effect0", "@Effect1", "@Effect2", "@Effect3", "@Attenuation", "@AttachableMixerInput",
                                           /* "@DefaultSwitchOrState", "@SwitchGroupOrStateGroup" */};

            foreach (var itr in NestedProperties)
            {
                SelectedObjInfo[itr] = SelectedObjInfo[itr]["id"];
            }

            // Remove '@' and ignore DefaultValue items.
            // Then, add '@' again to use argments of object creation.
            var ReturnString = SelectedObjInfo?.ToString();
            // Format to WwiseObjectsReference schema.
            ReturnString = Regex.Replace(ReturnString, @"(randomizer)(\(\\"")(.+?)(\\""\)\.)@(Enable|Max|Min)", "$1$3_$5").Replace("@", "_");
            // Remove default value property.
            ReturnString = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<WwiseObjectsReference>(ReturnString),
                                                                    Formatting.Indented,
                                                                    new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
            // Re-format to WAAPI options schema.
            ReturnString = Regex.Replace(ReturnString, @"(randomizer)(.+?)_(Enable|Max|Min)", "$1(\\\"$2\\\")._$3").Replace("_", "@");
            System.Diagnostics.Debug.WriteLine("\nReturnString:\n" + ReturnString);

            return ReturnString;

            //return JsonConvert.SerializeObject(JsonConvert.DeserializeObject<WwiseObjectsReference>(SelectedObjInfo.ToString().Replace("\"@", "\"_")),
            //                                                              Formatting.Indented,
            //                                                              new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }).Replace("\"_", "\"@");
        }

        /// <summary>
        /// Get children object.
        /// </summary>
        /// <param name="client">AK.Wwise.Waapi.dotNetJsonClient instance.</param>
        /// <param name="path">Parent object path.</param>
        /// <returns></returns>
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
            // Get Selected Object Info
            var SelectedObjInfo = JObject.Parse(await GetSelectedObjectInfo(client).ConfigureAwait(false));
            // Copy object to modify safely.
            JObject ArgumentsCreate = (JObject)SelectedObjInfo.DeepClone();

            // Modify info to use args for object creation
            var RandomizerAccessors = new[] { "randomizer(\"Volume\").@Enabled","randomizer(\"Volume\").@Max","randomizer(\"Volume\").@Min",
                                               "randomizer(\"Lowpass\").@Enabled","randomizer(\"Lowpass\").@Max","randomizer(\"Lowpass\").@Min",
                                               "randomizer(\"Highpass\").@Enabled","randomizer(\"Highpass\").@Max","randomizer(\"Highpass\").@Min",
                                               "randomizer(\"Pitch\").@Enabled","randomizer(\"Pitch\").@Max","randomizer(\"Pitch\").@Min",
                                               "randomizer(\"InitialDelay\").@Enabled","randomizer(\"InitialDelay\").@Max","randomizer(\"InitialDelay\").@Min",};
            JObject SourceRandomizedProps = new JObject();
            foreach (var itr in RandomizerAccessors)
            {
                if (ArgumentsCreate.ContainsKey(itr))
                {
                    SourceRandomizedProps.Add(itr, ArgumentsCreate[itr]);
                    ArgumentsCreate.Remove(itr);
                }
            }
            ArgumentsCreate.Remove("id");
            ArgumentsCreate.Remove("childrenCount");
            ArgumentsCreate.Add("onNameConflict", "rename");

            switch (ConvertTo)
            {
                case ("ActorMixer"):
                case ("BlendContainer"):
                case ("SwitchContainer"):
                    if ((ArgumentsCreate["type"]?.ToString() ?? "None") == ConvertTo)
                    {
                        System.Diagnostics.Debug.WriteLine("Convert is stopped. Selected Container type is same as ConvertTo.");
                        return 0;
                    }
                    ArgumentsCreate["@RandomOrSequence"]?.Parent.Remove();
                    ArgumentsCreate["type"] = ConvertTo;
                    break;

                case ("RandomContainer"):
                case ("SequenceContainer"):
                    if ((ArgumentsCreate["type"]?.ToString() == "RandomSequenceContainer") &&
                        (((ConvertTo == "SequenceContainer") && (ArgumentsCreate["@RandomOrSequence"]?.ToString() == "0")) ||
                         ((ConvertTo == "RandomContainer") && (ArgumentsCreate["@RandomOrSequence"]?.ToString() == "1"))))
                    {
                        System.Diagnostics.Debug.WriteLine("Convert is stopped. Selected Container type is same as ConvertTo.");
                        return 0;
                    }

                    if (ArgumentsCreate["@RandomOrSequence"] == null)
                        ArgumentsCreate.Add("@RandomOrSequence", (ConvertTo == "SequenceContainer") ? 0 : 1);
                    else
                        ArgumentsCreate["@RandomOrSequence"] = (ConvertTo == "SequenceContainer") ? 0 : 1;

                    ArgumentsCreate["type"] = "RandomSequenceContainer";
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine("No Argment of Containert Type for Convert to.");
                    return 0;
            }
            System.Diagnostics.Debug.WriteLine("\nArgments_create:\n" + ArgumentsCreate);

            var CreatedContainer = await client.Call(ak.wwise.core.@object.create, ArgumentsCreate, null).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine("\nCreatedContainer:\n" + CreatedContainer);

            // Make arguments for randomize
            JObject ArgumentsRandomize = new JObject();
            if (SourceRandomizedProps != null)
            {
                ArgumentsRandomize.Add("object", CreatedContainer["id"]);
                ArgumentsRandomize.Add("property", null);
                ArgumentsRandomize.Add("enabled", null);
                ArgumentsRandomize.Add("min", null);
                ArgumentsRandomize.Add("max", null);

                var RandomizedProps = new[] { "Volume", "Lowpass", "Highpass", "Pitch", "InitialDelay" };
                foreach (var itr in RandomizedProps)
                {

                    if (SourceRandomizedProps.ContainsKey("randomizer(\"" + itr + "\").@Enabled") ||
                        SourceRandomizedProps.ContainsKey("randomizer(\"" + itr + "\").@Max") ||
                        SourceRandomizedProps.ContainsKey("randomizer(\"" + itr + "\").@Min"))
                    {
                        ArgumentsRandomize["property"] = itr;
                        ArgumentsRandomize["enabled"] = SourceRandomizedProps["randomizer(\"" + itr + "\").@Enabled"] ?? false;
                        ArgumentsRandomize["min"] = SourceRandomizedProps["randomizer(\"" + itr + "\").@Min"] ?? 0;
                        ArgumentsRandomize["max"] = SourceRandomizedProps["randomizer(\"" + itr + "\").@Max"] ?? 0;
                        System.Diagnostics.Debug.WriteLine("\nArgumentsRandomize(\"" + itr + "\"):\n" + ArgumentsRandomize);
                        await client.Call(ak.wwise.core.@object.setRandomizer, ArgumentsRandomize, null).ConfigureAwait(false);
                    }
                }

            }

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
                            "children",
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

            return 0;
        }

        //public static async Task<int> MoveAllDescendants(AK.Wwise.Waapi.dotNetJsonClient client, JObject ParentContainer)
        //{
        //    await client.Call(ak.wwise.core.@object.create, null, null).ConfigureAwait(false);
        //    return 0;
        //}
    }
}
