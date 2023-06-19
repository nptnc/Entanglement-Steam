using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StressLevelZero.Arena;

using Entanglement.Patching;

using MelonLoader;

using UnityEngine;

namespace Entanglement.Network
{
    public class FantasyDifficultyMessageHandler : NetworkMessageHandler<FantasyDifficultyMessageData>
    {
        public override byte? MessageIndex => BuiltInMessageType.FantasyDiff;

        public override NetworkMessage CreateMessage(FantasyDifficultyMessageData data)
        {
            NetworkMessage message = new NetworkMessage();

            message.messageData = new byte[] { Convert.ToByte(data.difficulty) };

            return message;
        }

        public override void HandleMessage(NetworkMessage message, long sender)
        {
            if (message.messageData.Length <= 0)
                throw new IndexOutOfRangeException();

            Arena_GameManager instance = Arena_GameManager.instance;
            if (instance) {
                FantasyArena_Settings.m_invalidSettings = true;
                switch (message.messageData[0]) {
                    case 0: default:
                        instance.arenaChallengeUI.SetEasyDifficulty();
                        break;
                    case 1:
                        instance.arenaChallengeUI.SetMediumDifficulty();
                        break;
                    case 2:
                        instance.arenaChallengeUI.SetHardDifficulty();
                        break;
                }

                Control_UI_Arena arenaChallengeUI = instance.arenaChallengeUI;
                GameObject diffPage = arenaChallengeUI.difficultyPageObj;
                arenaChallengeUI.ActiveChallengePage(diffPage);
                arenaChallengeUI.homePageObj.SetActive(false);
                arenaChallengeUI.trialsPageObj.SetActive(false);
                arenaChallengeUI.resumeSurvivalButtonObj.transform.parent.gameObject.SetActive(false);
                arenaChallengeUI.challengeDescriptionText.transform.parent.gameObject.SetActive(false);
                arenaChallengeUI.transform.Find("Page_Brawl")?.gameObject?.SetActive(false);
            }

            if (Server.instance != null) {
                byte[] msgBytes = message.GetBytes();
                Server.instance.BroadcastMessageExcept(NetworkChannel.Reliable, msgBytes, sender);
            }
        }
    }

    public class FantasyDifficultyMessageData : NetworkMessageData {
        public byte difficulty;
    }
}
