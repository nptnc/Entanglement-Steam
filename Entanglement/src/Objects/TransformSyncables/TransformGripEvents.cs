using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using UnityEngine;

using Entanglement.Network;
using Entanglement.Data;
using Entanglement.Extensions;
using Entanglement.Representation;
using Entanglement.Patching;

using StressLevelZero.Pool;
using StressLevelZero.Interaction;
using StressLevelZero.Props.Weapons;
using StressLevelZero.Props;

using Utilties;

using MelonLoader;

namespace Entanglement.Objects
{
    // NOTE: This is temporary until the syncable rewrite!
    public partial class TransformSyncable : Syncable
    {
        public enum GripEventType : byte {
            AttachedEvent = 0,
            DetachEvent = 1,
            PrimaryButtonEventDown = 2,
            PrimaryButtonEvent = 3,
            PrimaryButtonEventUp = 4,
        }

        public GripEvents[] events;

        bool ignoreThisFrame = false;

        public void SetupEvents() {
            for (byte i = 0; i < events.Length; i++) {
                GripEvents grip = events[i];

#if DEBUG
                EntangleLogger.Log($"Found event at {i} with name {grip.name}");
#endif

                var attached = new Action(() => { AttachedEvent(grip); });
                var detached = new Action(() => { DetachEvent(grip); });
                var down = new Action(() => { PrimaryButtonEventDown(grip); });
                var normal = new Action(() => { PrimaryButtonEvent(grip); });
                var up = new Action(() => { PrimaryButtonEventUp(grip); });

                grip.AttachedEvent?.AddListener(attached);
                grip.DetachEvent?.AddListener(detached);
                grip.PrimaryButtonEventDown?.AddListener(down);
                grip.PrimaryButtonEvent?.AddListener(normal);
                grip.PrimaryButtonEventUp?.AddListener(up);
            }
        }

        public void CallEvent(GripEventType type, byte idx) {
#if DEBUG
            EntangleLogger.Log($"Received event of type {type} at index {idx}");
#endif

            if (events.Length <= idx) {
#if DEBUG
                EntangleLogger.Log($"Skipping event out of range (events length: {events.Length}, index: {idx})");
#endif
                return;
            }

            GripEvents grip = events[idx];

            ignoreThisFrame = true;

            try {
                switch (type)
                {
                    case GripEventType.AttachedEvent:
                    default:
                        grip.AttachedEvent?.Invoke();
                        break;
                    case GripEventType.DetachEvent:
                        grip.DetachEvent?.Invoke();
                        break;
                    case GripEventType.PrimaryButtonEvent:
                        grip.PrimaryButtonEvent?.Invoke();
                        break;
                    case GripEventType.PrimaryButtonEventDown:
                        grip.PrimaryButtonEventDown?.Invoke();
                        break;
                    case GripEventType.PrimaryButtonEventUp:
                        grip.PrimaryButtonEventUp?.Invoke();
                        break;
                }
            } 
            catch { }

            ignoreThisFrame = false;
        }

        public void AttachedEvent(GripEvents grip) => SendEvent(GripEventType.AttachedEvent, grip);

        public void DetachEvent(GripEvents grip) => SendEvent(GripEventType.DetachEvent, grip);

        public void PrimaryButtonEventDown(GripEvents grip) => SendEvent(GripEventType.PrimaryButtonEventDown, grip);

        public void PrimaryButtonEvent(GripEvents grip) => SendEvent(GripEventType.PrimaryButtonEvent, grip);

        public void PrimaryButtonEventUp(GripEvents grip) => SendEvent(GripEventType.PrimaryButtonEventUp, grip);

        public void SendEvent(GripEventType type, GripEvents grip) {
            if (ignoreThisFrame)
                return;

            // This is a really shitty workaround but unity events don't like to store bytes properly for some reason
            byte idx = 0;
            bool valid = false;

            for (byte i = 0; i < events.Length; i++) {
                if (grip == events[i])
                {
                    idx = i;
                    valid = true;
                    break;
                }
            }

            if (!valid)
                return;

#if DEBUG
            EntangleLogger.Log($"Sending Grip Event of type {type} and index {idx}.");
#endif

            GripEventMessageData data = new GripEventMessageData()
            {
                objectId = objectId,
                index = idx,
                type = type,
            };

            NetworkMessage gripEventMessage = NetworkMessage.CreateMessage(BuiltInMessageType.GripEvent, data);
            Node.activeNode.BroadcastMessage(NetworkChannel.Reliable, gripEventMessage.GetBytes());
        }
    }
}
