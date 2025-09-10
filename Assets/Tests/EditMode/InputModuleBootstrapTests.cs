using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif
using RogueLike2D.UI;

public class InputModuleBootstrapTests
{
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    [Test]
    public void EnsureCorrectInputModule_AssignsDefaultActions()
    {
        // Create a bare EventSystem with no input modules attached.
        var go = new GameObject("EventSystem", typeof(EventSystem));
        var es = go.GetComponent<EventSystem>();

        // Invoke the bootstrap to configure the proper input module.
        InputModuleBootstrap.EnsureCorrectInputModule(es, "unit-test");

        // The new Input System module should be added automatically.
        var uim = es.GetComponent<InputSystemUIInputModule>();
        Assert.IsNotNull(uim, "InputSystemUIInputModule should be present");

        // The helper should assign a default actions asset so the UI can process input.
        Assert.IsNotNull(uim.actionsAsset, "Expected a default actions asset to be assigned");
    }
#endif
}
