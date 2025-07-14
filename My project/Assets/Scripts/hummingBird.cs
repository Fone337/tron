using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DroneMechanics : MonoBehaviour
{
    public InputActionProperty jetpackAction;
    public InputActionProperty hoverAction;
    public TMP_Text debugText;

    private CharacterController characterController;

    [Header("Drone Movement")]
    public float maxVerticalSpeed = 10f;
    public float hoverForce = 2f;
    public float gravity = -9.81f;
    public float hoverTransitionSpeed = 5f;

    [Header("Energy Management")]
    public float fuelConsumptionRate = 10f;
    public float hoverFuelConsumptionRate = 1f;
    public float maxFuel = 100f;
    public float currentFuel;
    public float decelerationTime = 0.5f;

    [Header("Temperature Management")]
    public bool enableTemperature = true;
    public float maxTemperature = 100f;
    public float heatIncreaseRate = 10f;
    public float coolingRate = 5f;
    private float currentTemperature = 0f;
    private bool isOverheated = false;

    [Header("Ground Detection")]
    public float groundCheckDistance = 0.2f; // Distance to check for ground
    public LayerMask groundLayer; // LayerMask for ground layers
    private bool isGrounded = false; // Indicates whether the drone is grounded

    private bool isHovering = false;
    private Vector3 verticalVelocity = Vector3.zero;
    private float jetpackThrust = 0f;
    private bool hasLiftoff = false;
    private Vector3 currentMoveSpeed = Vector3.zero;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentFuel = maxFuel;
        if (debugText == null)
        {
            Debug.LogError("Debug Text not assigned in Inspector!");
        }
        Debug.Log("Jetpack initialized. Current Fuel: " + currentFuel);
    }

    private void OnEnable()
    {
        jetpackAction.action.Enable();
        hoverAction.action.Enable();
        Debug.Log("Input actions enabled.");
    }

    private void OnDisable()
    {
        jetpackAction.action.Disable();
        hoverAction.action.Disable();
        Debug.Log("Input actions disabled.");
    }

    private void Update()
    {
        // Check for ground detection
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (hoverAction.action.ReadValue<float>() > 0.1f)
        {
            TriggerHover();
        }
        else
        {
            isHovering = false;
        }

        if (!isHovering)
        {
            float triggerValue = jetpackAction.action.ReadValue<float>();
            if (triggerValue > 0.1f && (!isOverheated || !enableTemperature) && currentFuel > 0)
            {
                TriggerJetpack(triggerValue);
            }
            else
            {
                ResetJetpack();
            }
        }
        else
        {
            CoolDownJetpack();
        }

        HandleMovement();
        UpdateDebugText();
    }

    private void TriggerJetpack(float triggerValue)
    {
        if (!hasLiftoff)
        {
            hasLiftoff = true;  // Only mark liftoff, no initial force applied
        }
        else
        {
            jetpackThrust = Mathf.Lerp(jetpackThrust, maxVerticalSpeed * triggerValue, Time.deltaTime * 5f);
        }

        verticalVelocity.y = jetpackThrust;

        if (enableTemperature)
        {
            currentTemperature += heatIncreaseRate * Time.deltaTime;

            if (currentTemperature >= maxTemperature)
            {
                isOverheated = true;
                verticalVelocity.y = 0f;
                currentTemperature = maxTemperature;
            }
        }

        currentFuel -= fuelConsumptionRate * triggerValue * Time.deltaTime;

        if (currentFuel < 0)
        {
            currentFuel = 0;
            ResetJetpack();
        }

        UpdateDebugText();
    }

    private void ResetJetpack()
    {
        jetpackThrust = 0f;
        hasLiftoff = false;
        if (enableTemperature) CoolDownJetpack();
    }

    private void CoolDownJetpack()
    {
        currentTemperature -= coolingRate * Time.deltaTime;
        if (currentTemperature < 0f)
        {
            currentTemperature = 0f;
        }

        if (currentTemperature < maxTemperature * 0.5f)
        {
            isOverheated = false;
        }

        UpdateDebugText();
    }

    private void TriggerHover()
    {
        isHovering = true;

        verticalVelocity.y = Mathf.Lerp(verticalVelocity.y, hoverForce, Time.deltaTime * hoverTransitionSpeed);

        currentFuel -= hoverFuelConsumptionRate * Time.deltaTime;

        if (currentFuel < 0)
        {
            currentFuel = 0;
            ResetJetpack();
        }

        UpdateDebugText();
    }

    private void HandleMovement()
    {
        Vector3 targetMoveDirection = Vector3.zero;

        // Handle movement direction based on user input (you may want to add input handling here)
        // For example: targetMoveDirection = new Vector3(moveInput.x, 0, moveInput.y);

        currentMoveSpeed = Vector3.Lerp(currentMoveSpeed, targetMoveDirection, Time.deltaTime / decelerationTime);

        if (isGrounded)
        {
            currentMoveSpeed.y = 0f;
            verticalVelocity.y = 0f; // Reset vertical velocity if grounded and not hovering
        }
        else
        {
            if (!isHovering && jetpackThrust == 0f)
            {
                verticalVelocity.y += gravity * Time.deltaTime;
                // Clamp the vertical velocity to a maximum downward speed
                verticalVelocity.y = Mathf.Max(verticalVelocity.y, gravity);
            }
        }

        characterController.Move((currentMoveSpeed + verticalVelocity) * Time.deltaTime);
    }

    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            debugText.text = $"Fuel: {currentFuel:F1}\n" +
                             $"Temperature: {currentTemperature:F1}°C\n" +
                             $"Is Overheated: {isOverheated}\n" +
                             $"Vertical Velocity: {verticalVelocity.y:F1}";
        }
    }
}
