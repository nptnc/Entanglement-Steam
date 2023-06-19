using Entanglement.Network;

using ModThatIsNotMod.BoneMenu;

using UnityEngine;

namespace Entanglement.UI
{
    public static class ClientUI {
        public static void CreateUI(MenuCategory category) {
            MenuCategory serverCategory = category.CreateSubCategory("Client Menu", Color.white);

            MenuCategory serverPrefsCategory = serverCategory.CreateSubCategory("Client Settings", Color.white);

            serverPrefsCategory.CreateBoolElement("NameTags", Color.white, true, (value) => { Client.nameTagsVisible = value; });
        }
    }
}
