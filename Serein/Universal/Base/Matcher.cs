﻿using System;
using Newtonsoft.Json.Linq;
using Serein.Extensions;
using Serein.JSPlugin;
using System.Collections.Generic;
using System.Linq;

namespace Serein.Base
{
    internal static class Matcher
    {
        /// <summary>
        /// 统计信息
        /// </summary>
        public static string MessageReceived, MessageSent, SelfId;

        /// <summary>
        /// 处理来自控制台的消息
        /// </summary>
        /// <param name="line">控制台的消息</param>
        public static void Process(string line)
        {
            foreach (Items.Regex regex in Global.RegexList)
            {
                if (string.IsNullOrEmpty(regex.Expression) || regex.Area != 1)
                {
                    continue;
                }
                if (System.Text.RegularExpressions.Regex.IsMatch(line, regex.Expression))
                {
                    Command.Run(2, regex.Command, msgMatch: System.Text.RegularExpressions.Regex.Match(line, regex.Expression));
                }
            }
        }

        /// <summary>
        /// 处理来自机器人的消息
        /// </summary>
        /// <param name="packet">数据包</param>
        public static void Process(JObject packet)
        {
            if (string.IsNullOrEmpty(packet.TryGetString("post_type")))
            {
                return;
            }
            string postType = packet.TryGetString("post_type");
            long result, userID, groupID;
            switch (postType)
            {
                case "message":
                case "message_sent":
                    bool isSelfMessage = postType == "message_sent";
                    string messageType = packet.TryGetString("message_type");
                    string rawMessage = packet.TryGetString("raw_message");
                    userID = long.TryParse(packet.TryGetString("sender", "user_id"), out result) ? result : -1;
                    groupID = messageType == "group" && long.TryParse(packet.TryGetString("group_id"), out result) ? result : -1;
                    Logger.Output(Items.LogType.Bot_Receive, $"{packet.TryGetString("sender", "nickname")}({packet.TryGetString("sender", "user_id")})" + ":" + rawMessage);
                    if (!(messageType == "group" ^ Global.Settings.Bot.GroupList.Contains(groupID)))
                    {
                        foreach (Items.Regex regex in Global.RegexList)
                        {
                            if (
                                string.IsNullOrEmpty(regex.Expression) || // 表达式为空
                                regex.Area <= 1 ||  // 禁用或控制台
                                isSelfMessage ^ regex.Area == 4 || // 自身消息与定义域矛盾
                                !System.Text.RegularExpressions.Regex.IsMatch(rawMessage, regex.Expression) || // 不匹配
                                regex.Area == 2 && regex.Ignored.ToList().Contains(groupID) ||
                                regex.Area == 3 && regex.Ignored.ToList().Contains(userID) // 忽略
                                )
                            {
                                continue;
                            }
                            if (
                                !(
                                Global.Settings.Bot.PermissionList.Contains(userID) ||
                                Global.Settings.Bot.GivePermissionToAllAdmin &&
                                messageType == "group" && (
                                    packet.TryGetString("sender", "role") == "admin" ||
                                    packet.TryGetString("sender", "role") == "owner")
                                ) &&
                                regex.IsAdmin &&
                                !isSelfMessage
                                )
                            {
                                switch (regex.Area)
                                {
                                    case 2:
                                        EventTrigger.Trigger(Items.EventType.PermissionDeniedFromGroupMsg, groupID, userID);
                                        break;
                                    case 3:
                                        EventTrigger.Trigger(Items.EventType.PermissionDeniedFromPrivateMsg, -1, userID);
                                        break;
                                }
                                continue;
                            }
                            if (System.Text.RegularExpressions.Regex.IsMatch(rawMessage, regex.Expression))
                            {
                                if ((regex.Area == 4 || regex.Area == 2) && messageType == "group")
                                {
                                    Command.Run(
                                        1,
                                        regex.Command,
                                        packet,
                                        System.Text.RegularExpressions.Regex.Match(
                                            rawMessage,
                                            regex.Expression
                                        ),
                                        userID,
                                        groupID
                                    );
                                }
                                else if ((regex.Area == 4 || regex.Area == 3) && messageType == "private")
                                {
                                    Command.Run(
                                        1,
                                        regex.Command,
                                        packet,
                                        System.Text.RegularExpressions.Regex.Match(
                                            rawMessage,
                                            regex.Expression
                                            ),
                                        userID
                                    );
                                }
                            }
                        }
                        lock (Global.GroupCache)
                        {
                            if (!Global.GroupCache.ContainsKey(groupID))
                            {
                                Global.GroupCache.Add(groupID, new Dictionary<long, Items.Member>());
                            }
                            if (!Global.GroupCache[groupID].ContainsKey(userID))
                            {
                                Global.GroupCache[groupID].Add(userID, new Items.Member());
                            }
                            Global.GroupCache[groupID][userID].ID = userID;
                            Global.GroupCache[groupID][userID].Nickname = packet.TryGetString("sender", "nickname");
                            Global.GroupCache[groupID][userID].Card = packet.TryGetString("sender", "card");
                            Global.GroupCache[groupID][userID].Role = Array.IndexOf(Command.Roles, packet.TryGetString("sender", "role"));
                            Global.GroupCache[groupID][userID].GameID = Binder.GetGameID(userID);
                        }
                    }
                    if (!isSelfMessage)
                    {
                        if (messageType == "private")
                        {
                            JSFunc.Trigger(Items.EventType.ReceivePrivateMessage, userID, rawMessage, packet.TryGetString("sender", "nickname"));
                        }
                        else if (messageType == "group")
                        {
                            JSFunc.Trigger(Items.EventType.ReceiveGroupMessage, groupID, userID, rawMessage,
                                string.IsNullOrEmpty(packet.TryGetString("sender", "card")) ? packet.TryGetString("sender", "nickname") : packet.TryGetString("sender", "card"));
                        }
                    }
                    break;
                case "meta_event":
                    if (packet.TryGetString("meta_event_type") == "heartbeat")
                    {
                        SelfId = packet.TryGetString("self_id");
                        MessageReceived = (
                            string.IsNullOrEmpty(packet.TryGetString("status", "stat", "message_received")) ?
                            packet.TryGetString("status", "stat", "MessageReceived") : packet.TryGetString("status", "stat", "message_received"));
                        MessageReceived = (
                            string.IsNullOrEmpty(packet.TryGetString("status", "stat", "message_sent")) ?
                            packet.TryGetString("status", "stat", "MessageSent") : packet.TryGetString("status", "stat", "message_sent"));
                        if ((long.TryParse(MessageReceived, out long TempNumber) ? TempNumber : 0) > 10000000)
                        {
                            MessageReceived = (TempNumber / 10000).ToString("N1") + "w";
                        }
                        if ((long.TryParse(MessageSent, out TempNumber) ? TempNumber : 0) > 10000000)
                        {
                            MessageSent = (TempNumber / 10000).ToString("N1") + "w";
                        }
                    }
                    break;
                case "notice":
                    userID = long.TryParse(packet.TryGetString("user_id"), out result) ? result : -1;
                    groupID = long.TryParse(packet.TryGetString("group_id"), out result) ? result : -1;
                    if (Global.Settings.Bot.GroupList.Contains(groupID))
                    {
                        switch (packet.TryGetString("notice_type"))
                        {
                            case "GroupDecrease":
                            case "group_decrease":
                                EventTrigger.Trigger(Items.EventType.GroupDecrease, groupID, userID);
                                JSFunc.Trigger(Items.EventType.GroupDecrease, groupID, userID);
                                break;
                            case "GroupIncrease":
                            case "group_increase":
                                EventTrigger.Trigger(Items.EventType.GroupIncrease, groupID, userID);
                                JSFunc.Trigger(Items.EventType.GroupIncrease, groupID, userID);
                                break;
                            case "notify":
                                if (packet.TryGetString("sub_type") == "poke" &&
                                    packet.TryGetString("target_id") == SelfId)
                                {
                                    EventTrigger.Trigger(Items.EventType.GroupPoke, groupID, userID);
                                    JSFunc.Trigger(Items.EventType.GroupPoke, groupID, userID);
                                }
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
