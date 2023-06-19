using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Network;

using ModThatIsNotMod.BoneMenu;

using UnityEngine;

namespace Entanglement.UI
{
    public static class StatsUI
    {
        public static IntElement downElem, upElem;

        public static void CreateUI(MenuCategory category) {
            MenuCategory statsCategory = category.CreateSubCategory("Net Stats", Color.white);

            statsCategory.CreateIntElement("Bytes Down", Color.white, 0, null);
            statsCategory.CreateIntElement("Bytes Up", Color.white, 0, null);

            downElem = statsCategory.elements[0] as IntElement;
            upElem = statsCategory.elements[1] as IntElement;
        }

        public static void UpdateUI() {
            downElem.SetValue((int)Node.activeNode.recievedByteCount);
            Node.activeNode.recievedByteCount = 0;

            upElem.SetValue((int)Node.activeNode.sentByteCount);
            Node.activeNode.sentByteCount = 0;
        }
    }
}
