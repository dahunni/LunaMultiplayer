﻿using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LunaClient.Systems.ShareScienceSubject
{
    public class ShareScienceSubjectMessageHandler : SubSystem<ShareScienceSubjectSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.ScienceSubjectUpdate) return;

            if (msgData is ShareProgressScienceSubjectMsgData data)
            {
                var subject = new ScienceSubjectInfo(data.ScienceSubject); //create a copy of the tech value so it will not change in the future.
                LunaLog.Log($"Queue Science subject: {subject.Id}");
                System.QueueAction(() =>
                {
                    NewScienceSubject(subject);
                });
            }
        }

        private static void NewScienceSubject(ScienceSubjectInfo subject)
        {
            System.StartIgnoringEvents();

            var currentSubjects = System.ScienceSubjects;
            var receivedSubject = ConvertByteArrayToScienceSubject(subject.Data, subject.NumBytes);

            if (!currentSubjects.TryGetValue(subject.Id, out var existingSubject))
            {
                currentSubjects.Add(receivedSubject.id, receivedSubject);
            }
            else
            {
                existingSubject.dataScale = receivedSubject.dataScale;
                existingSubject.scientificValue = receivedSubject.scientificValue;
                existingSubject.subjectValue = receivedSubject.subjectValue;
                existingSubject.science = receivedSubject.science;
                existingSubject.scienceCap = receivedSubject.scienceCap;
            }

            System.StopIgnoringEvents();
            LunaLog.Log($"Science subject received: {subject.Id}");
        }

        /// <summary>
        /// Convert a byte array to a ConfigNode and then to a ScienceSubject.
        /// If anything goes wrong it will return null.
        /// </summary>
        private static ScienceSubject ConvertByteArrayToScienceSubject(byte[] data, int numBytes)
        {
            ConfigNode node;
            try
            {
                node = ConfigNodeSerializer.Deserialize(data, numBytes);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing science subject configNode: {e}");
                return null;
            }

            if (node == null)
            {
                LunaLog.LogError("[LMP]: Error, the science subject configNode was null.");
                return null;
            }

            return new ScienceSubject(node.GetNode("Science"));
        }
    }
}
