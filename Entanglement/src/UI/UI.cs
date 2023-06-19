using UnityEngine;

using ModThatIsNotMod.BoneMenu;

using Entanglement.Network;
using Entanglement.Representation;

namespace Entanglement.UI {
    public static class EntanglementUI {
        public static void CreateUI() {
            MenuCategory category = MenuManager.CreateCategory("Entanglement", Color.white);

            ServerUI.CreateUI(category);
            ClientUI.CreateUI(category);
            BanlistUI.CreateUI(category);
            LobbiesUI.CreateUI(category);
            VoiceUI.CreateUI(category);
            StatsUI.CreateUI(category);

#if DEBUG
            DebugUI.CreateUI(category);
#endif
        }
    }
}
