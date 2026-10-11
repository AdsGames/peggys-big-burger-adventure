using UnityEngine.InputSystem;

// Central place for all gameplay input bindings (Input System package).
public static class GameInput
{
  public static readonly InputAction Turn = new InputAction("Turn", InputActionType.Value, expectedControlType: "Axis");
  public static readonly InputAction Handbrake = new InputAction("Handbrake", InputActionType.Button);
  public static readonly InputAction Boost = new InputAction("Boost", InputActionType.Button);
  public static readonly InputAction Jump = new InputAction("Jump", InputActionType.Button);
  public static readonly InputAction DebugAddSegment = new InputAction("DebugAddSegment", InputActionType.Button);
  public static readonly InputAction SkipSplash = new InputAction("SkipSplash", InputActionType.Button);

  static GameInput()
  {
    Turn.AddCompositeBinding("1DAxis")
      .With("Negative", "<Keyboard>/leftArrow")
      .With("Positive", "<Keyboard>/rightArrow");
    Turn.AddCompositeBinding("1DAxis")
      .With("Negative", "<Keyboard>/a")
      .With("Positive", "<Keyboard>/d");
    Turn.AddCompositeBinding("1DAxis")
      .With("Negative", "<Gamepad>/dpad/left")
      .With("Positive", "<Gamepad>/dpad/right");
    // The stick's own deadzone only applies to the whole Vector2, not to leftStick/x,
    // so without this a resting stick that's slightly off centre steers constantly
    Turn.AddBinding("<Gamepad>/leftStick/x").WithProcessor("axisDeadzone");

    // Peggy always runs forward, so up and down are free for speeding up and sliding.
    // No modifier keys: in the browser, Ctrl and Alt combos clash with shortcuts like Ctrl+D.
    Handbrake.AddBinding("<Keyboard>/s");
    Handbrake.AddBinding("<Keyboard>/downArrow");
    Handbrake.AddBinding("<Gamepad>/leftTrigger");
    Handbrake.AddBinding("<Gamepad>/rightShoulder");

    Boost.AddBinding("<Keyboard>/w");
    Boost.AddBinding("<Keyboard>/upArrow");
    Boost.AddBinding("<Keyboard>/leftShift");
    Boost.AddBinding("<Gamepad>/rightTrigger");

    Jump.AddBinding("<Keyboard>/space");
    Jump.AddBinding("<Gamepad>/buttonSouth");

    DebugAddSegment.AddBinding("<Keyboard>/f");

    SkipSplash.AddBinding("<Keyboard>/anyKey");
    SkipSplash.AddBinding("<Mouse>/leftButton");
    SkipSplash.AddBinding("<Gamepad>/start");
    SkipSplash.AddBinding("<Gamepad>/buttonSouth");

    Turn.Enable();
    Handbrake.Enable();
    Boost.Enable();
    Jump.Enable();
    DebugAddSegment.Enable();
    SkipSplash.Enable();
  }
}
