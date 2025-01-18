using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;


public class PlayerCamera : MonoBehaviour
{
	public GameObject target;
	public OnScreenStick lookStick;
	public bool useMobileControls;
	// rotates the target object's (player's) Y 
	public bool rotateTargetY;

	public float zoom;
	public float heightAdvantage;

	[Range(0f, 2f)]
	public float sensitivity = 1;

	public bool panToPlayer;
	public float followSpeed;

	public bool smoothLook = false;
	[Range(0f, 1f)]
	public float smoothLookSpeed = 0.1f;

	public bool lookAtPlayer;
	public float lookAtDelay;
	public float lookForwardBy;
	public float lookSpeed;

	public bool lockRotation;

	Vector2 lookDirection;
	float timeSinceLastLook = 0;

	void Start()
	{
		useMobileControls = useMobileControls || Application.isMobilePlatform;	
	}

	void LateUpdate()
	{
		if (useMobileControls)
		{
			GetMobileInput();
		}

		Vector3 targetPosition = target.transform.position - transform.forward * zoom + target.transform.up * heightAdvantage;
		Vector3 lookAtTarget = target.transform.position + target.transform.forward * lookForwardBy;

		if (panToPlayer)
		{
			transform.position = Vector3.Lerp(transform.position, targetPosition, Mathf.Clamp01(followSpeed * Time.deltaTime));
		}
		else
		{
			transform.position = targetPosition;
			transform.LookAt(lookAtTarget);
		}

		if (lockRotation) { }
		else if (lookAtPlayer && timeSinceLastLook > lookAtDelay)
		{
			Quaternion oldRotation = transform.rotation;
			transform.LookAt(lookAtTarget);
			Quaternion newRotation = transform.rotation;
			transform.rotation = Quaternion.Lerp(oldRotation, newRotation, Mathf.Clamp01(lookSpeed * Time.deltaTime));
			lookDirection = new Vector2(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y);
		}
		else if (!smoothLook)
		{
			transform.rotation = Quaternion.Euler(lookDirection.y, lookDirection.x, 0);
			timeSinceLastLook += Time.deltaTime;
		}
		else
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(lookDirection.y, lookDirection.x, 0), smoothLookSpeed);
			timeSinceLastLook += Time.deltaTime;
		}

		if (!lookAtPlayer)
		{
			timeSinceLastLook = 0;
		}

		if (rotateTargetY)
		{
			target.transform.rotation = Quaternion.Euler(target.transform.eulerAngles.x, transform.eulerAngles.y, target.transform.eulerAngles.z);
		}
	}

	public void OnLook(InputAction.CallbackContext context)
	{
		if (useMobileControls || lockRotation) { return; }
		timeSinceLastLook = 0;
		lookDirection += CorrectLook(context.ReadValue<Vector2>()) * sensitivity;
	}

	public void GetMobileInput()
	{
		if (lockRotation) return;
		InputControl control = lookStick.control;

		if (control.IsPressed())
		{
			lookDirection += CorrectLook((Vector2) control.ReadValueAsObject()) * sensitivity;
			timeSinceLastLook = 0;
		}
	}

	public static Vector2 CorrectLook(Vector2 value)
	{
		value.y = -value.y;
		return value;
	}

}

