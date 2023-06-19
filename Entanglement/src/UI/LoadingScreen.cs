using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entanglement.Network;
using Entanglement.Data;

using UnityEngine;
using UnityEngine.UI;

namespace Entanglement.UI {
    public static class LoadingScreen {
        public static AssetBundle assetBundle;

        public static void LoadBundle() {
            assetBundle = EmebeddedAssetBundle.LoadFromAssembly(EntanglementMod.entanglementAssembly, "Entanglement.resources.logo.eres");
        }

        public static void OverrideScreen() {
            Texture2D texture = assetBundle.LoadAsset<Texture2D>("entanglement.png");
            GameObject rawImage = GameObject.Find("Canvas/RawImage (1)");

            rawImage.GetComponent<RawImage>().texture = texture;
        }
    }
}
