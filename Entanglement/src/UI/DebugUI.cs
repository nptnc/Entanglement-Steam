#if DEBUG
using UnityEngine;

using ModThatIsNotMod.BoneMenu;

using Entanglement.Representation;
using Entanglement.Objects;

namespace Entanglement.UI {
    public static class DebugUI {
        public static void CreateUI(MenuCategory category) {
            MenuCategory debugCategory = category.CreateSubCategory("--DEBUG--", Color.red);

            debugCategory.CreateFunctionElement("Create Debug Representation", Color.white, () => { PlayerRepresentation.debugRepresentation = new PlayerRepresentation("Dummy", 0); });
        }
    }
}
#endif