using UnityEngine.InputSystem;
namespace ForjaDasMares.Networking
{
    public sealed partial class CoopInventoryHud
    {
        InputAction regionalInventoryKey,regionalCloseKey;
        void BuildRegionalKeys()
        {
            inventoryButton.name="RegionalInventoryButton";
            regionalInventoryKey=new InputAction("RegionalInventory",InputActionType.Button,"<Keyboard>/i");
            regionalCloseKey=new InputAction("RegionalInventoryClose",InputActionType.Button,"<Keyboard>/escape");
            regionalInventoryKey.performed+=OnRegionalInventory;regionalCloseKey.performed+=OnRegionalClose;
            regionalInventoryKey.Enable();regionalCloseKey.Enable();
        }
        void OnRegionalInventory(InputAction.CallbackContext _){if(IsOpen)CloseInventory();else OpenInventory();}
        void OnRegionalClose(InputAction.CallbackContext _){if(IsOpen)CloseInventory();}
        void OnEnable(){regionalInventoryKey?.Enable();regionalCloseKey?.Enable();}
        void OnDestroy(){regionalInventoryKey?.Dispose();regionalCloseKey?.Dispose();}
    }
}

