﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

/******************************************************************************
The content of this file includes portions of the AUDIOKINETIC Wwise Technology
released in source code form as part of the SDK installer package.

Apache License Usage

Alternatively, this file may be used under the Apache License, Version 2.0 (the 
"Apache License"); you may not use this file except in compliance with the 
Apache License. You may obtain a copy of the Apache License at 
http://www.apache.org/licenses/LICENSE-2.0.
Unless required by applicable law or agreed to in writing, software distributed
under the Apache License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES
OR CONDITIONS OF ANY KIND, either express or implied. See the Apache License for
the specific language governing permissions and limitations under the License.

  Copyright (c) 2020 Audiokinetic Inc.
*******************************************************************************/

namespace AK.Wwise.Waapi
{
    /// <summary>
    /// The dotNetJsonClient provides an abstraction layer over the base Waapi Client and wraps everything under System.Text.Json.JsonDocument for convenience.
    /// </summary>
    class dotNetJsonClient
    {
        private AK.Wwise.Waapi.Client client = new AK.Wwise.Waapi.Client();
        public delegate void PublishHandler(Newtonsoft.Json.Linq.JObject json);
        //public delegate void PublishHandler(System.Text.Json.JsonDocument json);
        public event Wamp.DisconnectedHandler Disconnected;
        //private readonly System.Text.Json.JsonSerializerOptions serializeOptions = new JsonSerializerOptions { WriteIndented = true };

        public dotNetJsonClient()
        {
            client.Disconnected += Client_Disconnected;
        }

        private void Client_Disconnected()
        {
            if (Disconnected != null)
            {
                Disconnected();
            }
        }

        /// <summary>If the client isn't connected, connect to a running instance of Wwise Authoring.</summary>
        /// <param name="uri">URI to connect. Usually the WebSocket protocol (ws:) followed by the hostname and port, followed by waapi.</param>
        /// <example>Connect("ws://localhost:8080/waapi")</example>
        /// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
        public async Task Connect(
            string uri = "ws://localhost:8080/waapi",
            int timeout = System.Int32.MaxValue)
        {
            if (!client.IsConnected())
            {
                await client.Connect(uri, timeout).ConfigureAwait(false);
            }
        }

        /// <summary>Close the connection.</summary>
        /// <param name="timeout">The maximum timeout in millisecond for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
        public async Task Close()
        {
            await client.Close().ConfigureAwait(false);
        }

        /// <summary>
        /// Return true if the client is connected and ready for operations.
        /// </summary>
        public bool IsConnected()
        {
            return client.IsConnected();
        }

        /// <summary>
        /// Call a WAAPI remote procedure. Refer to WAAPI reference documentation for a list of URIs and their arguments and options.
        /// </summary>
        /// <param name="uri">The URI of the remote procedure.</param>
        /// <param name="args">The arguments of the remote procedure. C# anonymous objects will be automatically serialized to Json.</param>
        /// <param name="options">The options the remote procedure. C# anonymous objects will be automatically serialized to Json.</param>
        /// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
        /// <returns>A System.Text.Json.JsonDocument with the result of the Remote Procedure Call.</returns>
        public async System.Threading.Tasks.Task<Newtonsoft.Json.Linq.JObject> Call(
        //public async System.Threading.Tasks.Task<System.Text.Json.JsonDocument> Call(
            string uri,
            object args = null,
            object options = null,
            int timeout = System.Int32.MaxValue)
        {
            if (args == null)
                args = new { };
            if (options == null)
                options = new { };

            return await Call(uri, Newtonsoft.Json.Linq.JObject.FromObject(args), Newtonsoft.Json.Linq.JObject.FromObject(options), timeout).ConfigureAwait(false);
            //    return await Call(uri, System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(args, serializeOptions)),
            //                           System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(options, serializeOptions)), timeout);
        }

        /// <summary>
        /// Call a WAAPI remote procedure. Refer to WAAPI reference documentation for a list of URIs and their arguments and options.
        /// </summary>
        /// <param name="uri">The URI of the remote procedure.</param>
        /// <param name="args">The arguments of the remote procedure as a Newtonsoft.Json.Linq.JObject</param>
        /// <param name="options">The options the remote procedure as a Newtonsoft.Json.Linq.JObject.</param>
        /// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
        /// <returns>A System.Text.Json.JsonDocument with the result of the Remote Procedure Call.</returns>
        public async System.Threading.Tasks.Task<Newtonsoft.Json.Linq.JObject> Call(
        //public async System.Threading.Tasks.Task<System.Text.Json.JsonDocument> Call(
            string uri,
            Newtonsoft.Json.Linq.JObject args,
            //System.Text.Json.JsonDocument args,
            Newtonsoft.Json.Linq.JObject options,
            //System.Text.Json.JsonDocument options,
            int timeout = System.Int32.MaxValue)
        {
            if (args == null)
                args = new Newtonsoft.Json.Linq.JObject();
            //args = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { }, serializeOptions));
            if (options == null)
                options = new Newtonsoft.Json.Linq.JObject();
            //options = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(new { }, serializeOptions));

            string result = await client.Call(uri,
                                              args.ToString(),
                                              options.ToString(),
                                              timeout).ConfigureAwait(false);
            //string result = await client.Call(uri,
            //                                  args.ToString() != "System.Text.Json.JsonDocument" ? args.ToString() : "{}",
            //                                  options.ToString() != "System.Text.Json.JsonDocument" ? options.ToString() : "{}",
            //                                  timeout).ConfigureAwait(false);

            return Newtonsoft.Json.Linq.JObject.Parse(result);
            //return System.Text.Json.JsonDocument.Parse(result);
        }
        /// <summary>
        /// Subscribe to a topic. Refer to WAAPI reference documentation to obtain the list of topics available.
        /// </summary>
        /// <param name="topic">Topic to subscribe</param>
        /// <param name="options">Option for the subscrition.</param>
        /// <param name="publishHandler">Delegate that will be executed when the topic is pusblished.</param>
        /// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<int> Subscribe(
            string topic, object options,
            PublishHandler publishHandler,
            int timeout = System.Int32.MaxValue)
        {
            if (options == null)
                options = new { };

            return await Subscribe(topic,
                                   Newtonsoft.Json.Linq.JObject.FromObject(options),
                                   publishHandler,
                                   timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to a topic. Refer to WAAPI reference documentation to obtain the list of topics available.
        /// </summary>
        /// <param name="topic">Topic to subscribe</param>
        /// <param name="options">Option for the subscrition.</param>
        /// <param name="publishHandler">Delegate that will be executed when the topic is pusblished.</param>
        /// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
        /// <returns>The subscription id assigned to the subscription. Store the id to call Unsubscribe.</returns>
        public async System.Threading.Tasks.Task<int> Subscribe(
            string topic,
            Newtonsoft.Json.Linq.JObject options,
            PublishHandler publishHandler,
            int timeout = System.Int32.MaxValue)
        {
            if (options == null)
                options = new Newtonsoft.Json.Linq.JObject();

            return await client.Subscribe(
                topic,
                options.ToString(),
                (string json) =>
                {
                    publishHandler(Newtonsoft.Json.Linq.JObject.Parse(json));
                },
                timeout).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from a subscription.
        /// </summary>
        /// <param name="subscriptionId">The subscription id received from the initial subscription.</param>
        /// <param name="timeout">The maximum timeout in milliseconds for the function to execute. Will raise Waapi.TimeoutException when timeout is reached.</param>
        public async Task Unsubscribe(
            int subscriptionId,
            int timeout = System.Int32.MaxValue)
        {
            await client.Unsubscribe(subscriptionId, timeout).ConfigureAwait(false);
        }
    }
}