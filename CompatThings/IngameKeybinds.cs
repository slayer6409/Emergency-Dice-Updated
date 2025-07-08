using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace MysteryDice;

public class IngameKeybinds : LcInputActions
{
    [InputAction("<Keyboard>/numpadMinus", Name = "DebugMenu")]
    public InputAction DebugMenu { get; set; } = null!;
    
    [InputAction("<Keyboard>/space", Name = "FlyButton")]
    public InputAction FlyButton { get; set; } = null!;
    
    [InputAction("<Keyboard>/ctrl", Name = "FlyDownButton")]
    public InputAction FlyDownButton { get; set; } = null!;

}