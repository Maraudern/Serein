﻿using Jint;
using Jint.Native;
using System;
using WebSocket4Net;

namespace Serein.Plugin
{
    internal class JSWebSocket : IDisposable
    {
        /// <summary>
        /// 事件函数
        /// </summary>
        public static Delegate onopen = null, onclose = null, onerror = null, onmessage = null;

        /// <summary>
        /// WS客户端
        /// </summary>
        private WebSocket _WebSocket = null;

        /// <summary>
        /// 状态
        /// </summary>
        public int state => _WebSocket != null ? -1 : ((int)_WebSocket.State);

        public void Dispose()
        {
            if (_WebSocket != null && _WebSocket.State == WebSocketState.Open)
                _WebSocket.Close();
            _WebSocket.Dispose();
        }

        /// <summary>
        /// 入口函数
        /// </summary>
        /// <param name="Uri">ws地址</param>
        /// <param name="Token">鉴权Token</param>
        public JSWebSocket(string Uri)
        {
            _WebSocket = new WebSocket(
                Uri,
                "",
                null,
                null
                );
            _WebSocket.Opened += (sender, e) => onopen?.DynamicInvoke(JsValue.Undefined, new[] { JsValue.Undefined });
            _WebSocket.Closed += (sender, e) => onclose?.DynamicInvoke(JsValue.Undefined, new[] { JsValue.Undefined });
            _WebSocket.MessageReceived += (sender, e) =>
            {
                if (onmessage != null)
                {
                    try
                    {
                        onmessage.DynamicInvoke(JsValue.Undefined, new[] { JsValue.FromObject(JSEngine.Converter, e.Message) });
                    }
                    catch { }
                }
            };
            _WebSocket.Error += (sender, e) => onerror?.DynamicInvoke(JsValue.Undefined, new[] { JsValue.FromObject(JSEngine.Converter, e.Exception.Message) });
            Plugins.WebSockets.Add(this);
        }

#pragma warning disable IDE1006

        /// <summary>
        /// 开启ws
        /// </summary>
        public void open() => _WebSocket.Open();

        /// <summary>
        /// 关闭ws
        /// </summary>
        public void close() => _WebSocket.Close();

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Msg"></param>
        public void send(string Msg) => _WebSocket.Send(Msg);

#pragma warning restore IDE1006
    }
}
