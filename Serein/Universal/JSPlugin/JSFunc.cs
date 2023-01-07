﻿using Jint.Native;
using Jint.Runtime;
using Serein.Base;
using Serein.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;

namespace Serein.JSPlugin
{
    partial class JSFunc
    {
        public static long ID = 0;

        /// <summary>
        /// 注册插件
        /// </summary>
        /// <param name="namespace">命名空间</param>
        /// <param name="name">名称</param>
        /// <param name="version">版本</param>
        /// <param name="author">作者</param>
        /// <param name="description">介绍</param>
        /// <returns>注册结果</returns>
        public static string Register(
            string @namespace,
            string name,
            string version,
            string author,
            string description
            )
        {
            if (@namespace == null || !JSPluginManager.PluginDict.ContainsKey(@namespace))
            {
                throw new ArgumentException("无法找到对应的命名空间", nameof(@namespace));
            }
            lock (JSPluginManager.PluginDict)
            {
                JSPluginManager.PluginDict[@namespace].Name = name;
                JSPluginManager.PluginDict[@namespace].Version = version;
                JSPluginManager.PluginDict[@namespace].Author = author;
                JSPluginManager.PluginDict[@namespace].Description = description;
            }
            return @namespace;
        }

        /// <summary>
        /// 注册事件监听器
        /// </summary>
        /// <param name="Namespace">命名空间</param>
        /// <param name="EventName">事件名称</param>
        /// <param name="Function">函数</param>
        /// <returns>注册结果</returns>
        public static bool SetListener(string Namespace, string EventName, Delegate Function)
        {
            Logger.Out(LogType.Debug, "Namespace:", Namespace, "EventName:", EventName);
            EventName = System.Text.RegularExpressions.Regex.Replace(EventName ?? string.Empty, "^on", string.Empty);
            if (!Enum.IsDefined(typeof(EventType), EventName))
            {
                throw new Exception("未知的事件：" + EventName);
            }
            lock (JSPluginManager.PluginDict)
                return
                    JSPluginManager.PluginDict.ContainsKey(Namespace) &&
                    JSPluginManager.PluginDict[Namespace].SetListener((EventType)Enum.Parse(typeof(EventType), EventName), Function);
        }

        private static readonly object Lock = new();

        /// <summary>
        /// 触发插件事件
        /// </summary>
        /// <param name="Type">事件名称</param>
        /// <param name="Args">参数</param>
        public static void Trigger(EventType Type, params object[] Args)
        {
            if (JSPluginManager.PluginDict.Count == 0)
            {
                return;
            }
            lock (Lock)
            {
                Logger.Out(LogType.Debug, Type);
                lock (JSPluginManager.PluginDict)
                {
                    JSPluginManager.PluginDict.Keys.ToList().ForEach((Key) => JSPluginManager.PluginDict[Key].Trigger(Type, Args));
                }
                System.Threading.Tasks.Task.Delay(Global.Settings.Serein.DevelopmentTool.JSEventCoolingDownTime).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// 设置定时器
        /// </summary>
        /// <param name="Namespace">命名空间</param>
        /// <param name="Function">函数</param>
        /// <param name="Interval">间隔</param>
        /// <param name="AutoReset"自动重置></param>
        /// <returns>定时器哈希值</returns>
        public static JsValue SetTimer(string Namespace, Delegate Function, JsValue Interval, bool AutoReset)
        {
            if (Namespace == null && !JSPluginManager.PluginDict.ContainsKey(Namespace))
            {
                throw new ArgumentException("无法找到对应的命名空间", nameof(Namespace));
            }
            long TimerID = ID;
            ID++;
            Logger.Out(LogType.Debug, "Interval:", Interval.ToString(), "AutoReset:", AutoReset, "ID:", TimerID);
            Timer _Timer = new((double)Interval.ToObject())
            {
                AutoReset = AutoReset,
            };
            _Timer.Elapsed += (sender, args) =>
            {
                try
                {
                    if (Namespace == null && !JSPluginManager.PluginDict.ContainsKey(Namespace))
                    {
                        throw new ArgumentException("无法找到对应的命名空间", nameof(Namespace));
                    }
                    else if (JSPluginManager.PluginDict[Namespace].Engine == null)
                    {
                        _Timer.Dispose();
                    }
                    else
                    {
                        lock (JSPluginManager.PluginDict[Namespace].Engine)
                        {
                            Function.DynamicInvoke(JsValue.Undefined, new[] { JsValue.Undefined });
                        }
                    }
                }
                catch (Exception e)
                {
                    string Message;
                    if (e.InnerException is JavaScriptException JSe)
                    {
                        Message = $"{JSe.Message}\n{JSe.JavaScriptStackTrace}";
                    }
                    else
                    {
                        Message = (e.InnerException ?? e).Message;
                    }
                    Logger.Out(LogType.Plugin_Error, $"[{Namespace}]", $"触发定时器[ID:{TimerID}]时出现异常：{Message}");
                    Logger.Out(LogType.Debug, $"触发定时器[ID:{TimerID}]时出现异常：", e);
                }
                if (!AutoReset)
                {
                    _Timer.Dispose();
                }
            };
            _Timer.Start();
            JSPluginManager.Timers.Add(TimerID, _Timer);
            return JsValue.FromObject(JSEngine.Converter, TimerID);
        }

        /// <summary>
        /// 清除定时器
        /// </summary>
        /// <param name="ID">哈希值</param>
        /// <returns>清除结果</returns>
        private static bool ClearTimer(long ID)
        {
            if (JSPluginManager.Timers.ContainsKey(ID))
            {
                JSPluginManager.Timers[ID].Stop();
                JSPluginManager.Timers[ID].Dispose();
                return JSPluginManager.Timers.Remove(ID);
            }
            return false;
        }

        /// <summary>
        /// 清除定时器
        /// </summary>
        /// <param name="ID">定时器ID</param>
        /// <returns>清除结果</returns>
        public static bool ClearTimer(JsValue ID) => ClearTimer((long)(double)ID.ToObject());

        /// <summary>
        /// 清除所有定时器
        /// </summary>
        public static void ClearAllTimers()
        {
            IList<long> IDs = JSPluginManager.Timers.Keys.ToArray();
            foreach (long ID in IDs)
            {
                ClearTimer(ID);
            }
        }

        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="Text">文本</param>
        /// <returns>MD5文本</returns>
        public static string GetMD5(string Text)
        {
            byte[] Datas = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Text));
            string Result = string.Empty;
            for (int i = 0; i < Datas.Length; i++)
            {
                Result += Datas[i].ToString("x2");
            }
            return Result;
        }
    }
}
