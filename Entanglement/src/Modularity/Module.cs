namespace Entanglement.Modularity
{
    // To avoid MelonLoader messing with us, here's what we do instead
    // We have our own modules that act like MelonMod's, but arent!
    public abstract class EntanglementModule
    {
        // First thing to ever be called inside a module
        public virtual void OnModuleLoaded() { }

        // Unity Update Methods
        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }

        // Misc...
        public virtual void OnSceneWasInitialized(int buildIndex, string sceneName) { }
        public virtual void OnLoadingScreen() { }
        public virtual void OnApplicationQuit() { }
    }
}