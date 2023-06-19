using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Entanglement.Network;

using ModThatIsNotMod.BoneMenu;

using UnityEngine;

using Discord;

using MelonLoader;

namespace Entanglement.UI
{
    public static class LobbiesUI {
        static MenuCategory lobbiesCategory;

        // First long is userid, second long is lobbyid
        static Dictionary<long, long> lobbiesFound = new Dictionary<long, long>();

        const string refreshText = "Refresh";

        public static void CreateUI(MenuCategory category) {
            lobbiesCategory = category.CreateSubCategory("Public Lobbies", Color.white);

            lobbiesCategory.CreateFunctionElement(refreshText, Color.white, Refresh);
        }

        public static void Refresh() {
            ClearMenuItems();

            LobbySearchQuery searchQuery = DiscordIntegration.lobbyManager.GetSearchQuery();
            searchQuery.Distance(LobbySearchDistance.Default);
            DiscordIntegration.lobbyManager.Search(searchQuery, OnDiscordLobbySearch);

            UpdateMenu();
        }

        public static void OnDiscordLobbySearch(Result res) {
            if (res == Result.Ok) {
                int lobbies = DiscordIntegration.lobbyManager.LobbyCount();

                EntangleLogger.Log($"Searched for {lobbies} Public Lobb{(lobbies == 1 ? "y" : "ies")}.");

                for (int i = 0; i < lobbies; i++) {
                    long lobbyId = DiscordIntegration.lobbyManager.GetLobbyId(i);
#if DEBUG
                    EntangleLogger.Log($"Found Lobby with id {lobbyId}.");
#endif
                    AddLobby(lobbyId);
                }
            }
            else {
                EntangleLogger.Log($"Failed to search for Public Lobbies with result Discord {res}.");
            }
        }
        public static void ClearMenuItems() {
            lobbiesFound.Clear();

            List<string> elementsToRemove = new List<string>();
            foreach (MenuElement element in lobbiesCategory.elements) {
                if (element.displayText != refreshText) elementsToRemove.Add(element.displayText);
            }

            foreach (string element in elementsToRemove) lobbiesCategory.RemoveElement(element);
        }

        public static void AddLobby(long lobbyId) {
#if DEBUG
            EntangleLogger.Log($"Trying to add lobby with id {lobbyId}.");
#endif
            Lobby lobby = DiscordIntegration.lobbyManager.GetLobby(lobbyId);
            lobbiesFound.Add(lobby.OwnerId, lobbyId);
            DiscordIntegration.userManager.GetUser(lobby.OwnerId, DiscordUserGetCallback);
        }
        
        public static void DiscordUserGetCallback(Result res, ref User user) {
#if DEBUG
            EntangleLogger.Log($"Creating BoneMenu option.");
#endif

            long lobbyId = lobbiesFound[user.Id];
            Lobby lobby = DiscordIntegration.lobbyManager.GetLobby(lobbyId);
            string lobbySecret = DiscordIntegration.lobbyManager.GetLobbyActivitySecret(lobbyId);
            IEnumerable<User> users = DiscordIntegration.lobbyManager.GetMemberUsers(lobbyId);

            CreateLobbyItem($"{user.Username}#{user.Discriminator}'s Game ({users.Count()}/{lobby.Capacity})", lobbyId, lobbySecret);
        }

        public static void CreateLobbyItem(string name, long lobbyId, string secret) {
            lobbiesCategory.CreateFunctionElement(name, Color.white, () => {
                if (DiscordIntegration.hasLobby) {
                    EntangleLogger.Error("Already in a server!");
                    return;
                }

                Lobby serverLobby = DiscordIntegration.lobbyManager.GetLobby(lobbyId);
                if (serverLobby.Id == 0) return;

                if (serverLobby.Type != LobbyType.Public) return;
                
                DiscordIntegration.lobbyManager.ConnectLobbyWithActivitySecret(secret, Client.instance.DiscordJoinLobby);
            });

            UpdateMenu();
        }

        public static void UpdateMenu() => MenuManager.OpenCategory(lobbiesCategory);
    }
}
