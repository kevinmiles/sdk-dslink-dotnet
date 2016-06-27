﻿using System;
using DSLink.Connection;
using DSLink.Connection.Serializer;
using DSLink.Container;
using Foundation;
using Square.SocketRocket;

namespace DSLink.iOS
{
    public class iOSWebSocketConnector : Connector
    {
        /// <summary>
        /// WebSocket client instance.
        /// </summary>
        private WebSocket _webSocket;

        public iOSWebSocketConnector(AbstractContainer link, Configuration config, ISerializer serializer) : base(link, config, serializer)
        {
        }

        /// <summary>
        /// Connect to the WebSocket.
        /// </summary>
        public override void Connect()
        {
            base.Connect();

            _webSocket = new WebSocket(new NSUrl(WsUrl));
            _webSocket.Open();

            _webSocket.WebSocketOpened += (object sender, EventArgs e) =>
            {
                EmitOpen();
            };
            _webSocket.WebSocketClosed += (object sender, WebSocketClosedEventArgs e) =>
            {
                if (e.WasClean)
                {
                    _link.Logger.Info(string.Format("WebSocket was closed cleanly with code {0}, and reason \"{1}\"", e.Code, e.Reason));
                }
                else
                {
                    _link.Logger.Info(string.Format("WebSocket was closed uncleanly with code {0}, and reason \"{1}\"", e.Code, e.Reason));
                }

                EmitClose();
            };

            _webSocket.WebSocketFailed += (object sender, WebSocketFailedEventArgs e) =>
            {
                _link.Logger.Error(string.Format("WebSocket error: {0}", e.Error));
            };

            _webSocket.ReceivedMessage += (object sender, WebSocketReceivedMessageEventArgs e) =>
            {
                EmitMessage(new MessageEvent(e.Message.ToString()));
            };
        }

        /// <summary>
        /// Disconnect from the WebSocket.
        /// </summary>
        public override void Disconnect()
        {
            base.Disconnect();

            _webSocket.Close();
            _webSocket.Dispose();
        }

        /// <summary>
        /// Returns true if the WebSocket is connected.
        /// </summary>
        public override bool Connected()
        {
            return _webSocket.ReadyState == ReadyState.Open;
        }

        /// <summary>
        /// Writes a string over the WebSocket connection.
        /// </summary>
        /// <param name="data">String data</param>
        public override void WriteString(string data)
        {
            base.WriteString(data);
            _webSocket.Send((NSString)data);
        }

        /// <summary>
        /// Writes binary over the WebSocket connection.
        /// </summary>
        /// <remarks>Not implemented</remarks>
        /// <param name="data">Binary data</param>
        public override void WriteBinary(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
