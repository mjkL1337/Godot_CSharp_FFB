using Godot;
using System;
using System.Collections.Generic;
using SharpDX.DirectInput;
using SharpDX;

public partial class FFB : Node
{
	private DirectInput _directInput = null;
	private Joystick _joystick = null;
	private Effect _constantForceEffect = null;
	private EffectParameters _cachedParameters = null;
	private ConstantForce _cachedForce = null;

	public override void _Ready()
	{
		_directInput = new DirectInput();
		InitializeDevices();
	}

	public void InitializeDevices()
	{
		if (_constantForceEffect != null)
		{
			_constantForceEffect.Stop();
			_constantForceEffect.Dispose();
			_constantForceEffect = null;
		}
		if (_joystick != null)
		{
			_joystick.Unacquire();
			_joystick.Dispose();
			_joystick = null;
		}

		var devices = _directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
		foreach (var device in devices)
		{
			var joystick = new Joystick(_directInput, device.InstanceGuid);
			IntPtr windowHandle = (IntPtr)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle);
			joystick.SetCooperativeLevel(windowHandle, CooperativeLevel.Background | CooperativeLevel.Exclusive);
			joystick.Properties.AxisMode = DeviceAxisMode.Absolute;
			joystick.Properties.AutoCenter = false;
			joystick.Acquire();

			var effect = SetupConstantForce(joystick);
			if (effect != null)
			{
				GD.Print($"[DINPUT] FFB device found: {device.ProductName}");
				_joystick = joystick;
				_constantForceEffect = effect;
				return;
			}

			joystick.Unacquire();
			joystick.Dispose();
		}

		GD.Print("[DINPUT] No FFB device found.");
	}

	private Effect SetupConstantForce(Joystick joystick)
	{
		var axesList = new List<int>();
		foreach (var deviceObject in joystick.GetObjects())
		{
			if (deviceObject.ObjectId.Flags.HasFlag(DeviceObjectTypeFlags.ForceFeedbackActuator))
				axesList.Add((int)deviceObject.ObjectId);
		}
		if (axesList.Count == 0) return null;

		int[] axes = axesList.ToArray();
		int[] directions = new int[axes.Length];

		_cachedForce = new ConstantForce() { Magnitude = 0 };
		_cachedParameters = new EffectParameters()
		{
			Flags = EffectFlags.Cartesian | EffectFlags.ObjectIds,
			Duration = -1,
			SamplePeriod = 0,
			Gain = 10000,
			TriggerButton = -1,
			TriggerRepeatInterval = 0,
			Axes = axes,
			Directions = directions,
			Parameters = _cachedForce
		};
		try
		{
			var effect = new Effect(joystick, EffectGuid.ConstantForce, _cachedParameters);
			effect.Start(1);
			return effect;
		}
		catch (Exception e)
		{
			GD.PrintErr($"[DINPUT] Effect setup failed: {e.Message}");
			return null;
		}
	}

	public void SetForce(float strength)
	{
		if (_constantForceEffect == null) return;
		try
		{
			_cachedForce.Magnitude = (int)Mathf.Clamp(strength * -10000.0f, -10000, 10000);
			_cachedParameters.Parameters = _cachedForce;
			_constantForceEffect.SetParameters(_cachedParameters, EffectParameterFlags.TypeSpecificParameters);
		}
		catch (SharpDXException)
		{
			_constantForceEffect = null;
		}
	}

	public override void _ExitTree()
	{
		if (_constantForceEffect != null)
		{
			_constantForceEffect.Stop();
			_constantForceEffect.Dispose();
		}
		if (_joystick != null)
		{
			_joystick.Unacquire();
			_joystick.Dispose();
		}
		_directInput?.Dispose();
	}
}
