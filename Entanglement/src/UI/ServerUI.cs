using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Network;
using Entanglement.Data;

using ModThatIsNotMod.BoneMenu;

using UnityEngine;

using MelonLoader;

namespace Entanglement.UI {
    public static class ServerUI {
        static MenuCategory playersCategory;

        const string refreshText = "Refresh";

        public static void CreateUI(MenuCategory category) {
            MenuCategory serverCategory = category.CreateSubCategory("Server Menu", Color.white);

            serverCategory.CreateFunctionElement("Start Server", Color.white, () => { Server.StartServer(); });

            serverCategory.CreateFunctionElement("Stop Server", Color.white, () => {
                if (Server.instance != null)
                    Server.instance.Shutdown();
            });

            serverCategory.CreateFunctionElement("Disconnect", Color.white, () => {
                if (Node.activeNode is Client client) {
                    client.DisconnectFromServer();
                }
            });

            MenuCategory serverPrefsCategory = serverCategory.CreateSubCategory("Server Settings", Color.white);

            serverPrefsCategory.CreateIntElement("Max Players", Color.white, 8, (value) => {
                Server.maxPlayers = (byte)value;
                //Server.instance?.UpdateLobbyConfig();
            },
            1, Server.serverMinimum, Server.serverCapacity, true);

            serverPrefsCategory.CreateBoolElement("Locked", Color.white, false, (value) =>
            {
                Server.isLocked = value;
                //Server.instance?.UpdateLobbyConfig();
            });

            /*serverPrefsCategory.CreateEnumElement("Visibility", Color.white, LobbyType.Private, (value) =>
            {
                if (!(value is LobbyType)) return;

                LobbyType type = (LobbyType)value;
                Server.lobbyType = type;
                Server.instance?.UpdateLobbyConfig();
            });*/

            playersCategory = serverCategory.CreateSubCategory("Players", Color.white);

            playersCategory.CreateFunctionElement(refreshText, Color.white, Refresh);
        }

        public static void ClearPlayers() {
            List<string> elementsToRemove = new List<string>();
            foreach (MenuElement element in playersCategory.elements) {
                if (element.displayText != refreshText) elementsToRemove.Add(element.displayText);
            }

            foreach (string element in elementsToRemove) playersCategory.RemoveElement(element);
        }

        public static void Refresh() {
            ClearPlayers();

            if (!SteamIntegration.hasServer) {
                UpdateMenu();
                return;
            }

           
            foreach (PlayerId user in PlayerIds.playerIds) {
                if (user.LargeId == SteamIntegration.localId.LargeId)
                    continue;

                AddUser(user);
            }

            UpdateMenu();
        }

        public static void UpdateMenu() => MenuManager.OpenCategory(playersCategory);

        public static void AddUser(PlayerId player) {
            string playerName = $"{player.Username}";
            Color playerColor = Color.white;
            if (player.SmallId == 0) { 
                playerName += " (Host)";
                playerColor = Color.yellow;
            }

            MenuCategory userItem = playersCategory.CreateSubCategory(playerName, playerColor);
            if (SteamIntegration.isHost) {
                userItem.CreateFunctionElement("Kick", Color.red, () => {
                    if (!SteamIntegration.isHost) return;

                    Server.instance?.KickUser(player.LargeId, playerName);

                    Refresh();
                });

                userItem.CreateFunctionElement("Ban", Color.red, () => {
                    if (!SteamIntegration.isHost) return;

                    BanList.BanUser(player);
                    Server.instance.KickUser(player.LargeId, playerName, DisconnectReason.Banned);

                    Refresh();
                });

                userItem.CreateFunctionElement("Teleport To", Color.yellow, () => { Server.instance?.TeleportTo(player.LargeId); });
            }
            /*userItem.CreateIntElement("Volume", Color.white, DiscordIntegration.voiceManager.GetLocalVolume(player.Id) / 20, (value) => {
                DiscordIntegration.voiceManager.SetLocalVolume(player.Id, (byte)(value * 20));
            }, 1, 0, 10, true);

            userItem.CreateBoolElement("Muted", Color.white, DiscordIntegration.voiceManager.IsLocalMute(player.Id), (value) => {
                DiscordIntegration.voiceManager.SetLocalMute(player.Id, value);
            });*/
        }
    }
}
