using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] bool Interpolated = true;
    [SerializeField] [Range(0.001F, 200F)] float MouseSensitivity = 100F;
    [SerializeField] [Range(0.001F, 10F)] float MoveFactor = 10F;

    float lastFixedTime;
    float lastFixedTimestep;

    Inputs internalInputs;
    Vector3 internalPosition;
    Vector3 internalEulers;

    Quaternion fixedStartRotation;
    Vector3 fixedStartPosition;

    // WARNING:
    // I SUGGEST YOU NEVER USE EULER ANGLES FOR COMPLICATED ROTATION CALCULATIONS
    // IM ONLY USING THEM HERE AS THEY'RE IN A SIMPLE SCRIPT.
    // They have a tendency of being incredibly unpredictable at times.

    // In other words : (use quaternions instead if you can)
    struct Inputs
    {
        public float Mouse_X, Mouse_Y;
        public float Move_X, Move_Y;
    }

    void Start()
    {
        lastFixedTime = Time.time;
        lastFixedTimestep = Time.fixedDeltaTime;

        internalInputs = new Inputs();
        internalEulers = transform.eulerAngles;

        internalPosition = transform.position;
    }

    //Always write inputs in Update(), act upon them in Fixed or regular Update depending on the context, though.
    void Update()
    {
        WriteInputs(ref internalInputs);

        if (lastFixedTimestep <= 0F || !Interpolated)
            return;

        //Take the amount of time that's passed from out last FixedUpdate timestep, and divide it by the last fixed delta step.

        //This allows us to 'correctly' smooth our position from the last update to the next.

        //Note that the visual representation of our transform will not always be where the actual position is, but nobody will notice
        // if your framerate and fixedrate are high enough.

        //TLDR; interpolation makes movement look nice so use it :)

        float interpolationFactor = (Time.time - lastFixedTime) / lastFixedTimestep;

        fixedStartRotation = Quaternion.Slerp(
            fixedStartRotation,
            Quaternion.Euler(internalEulers),
            interpolationFactor);

        fixedStartPosition = Vector3.Lerp(
            fixedStartPosition,
            internalPosition,
            interpolationFactor);

        transform.position = fixedStartPosition;
        transform.rotation = fixedStartRotation;
    }

    void WriteInputs(ref Inputs localInputs)
    {
        localInputs.Mouse_X = Input.GetAxisRaw("Mouse X");
        localInputs.Mouse_Y = Input.GetAxisRaw("Mouse Y");

        localInputs.Move_X = Input.GetAxisRaw("Horizontal");
        localInputs.Move_Y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        lastFixedTime = Time.fixedTime;
        lastFixedTimestep = Time.fixedDeltaTime;

        /* Handle Camera Inputs */
        HandleCameraInputs(in internalInputs, lastFixedTimestep);
    }

    void HandleCameraInputs(in Inputs localInputs, float fdt)
    {
        /* Setting the start transforms before anything is done */

        fixedStartPosition = internalPosition;
        fixedStartRotation = Quaternion.Euler(internalEulers);

        /* Camera */

        // Switching different lock states
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            switch (Cursor.lockState)
            {
                case CursorLockMode.Locked:
                    Cursor.lockState = CursorLockMode.None;
                    break;
                default:
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
            }
        }

        // Only rotate camera if we're in the cursor's lock state to prevent annoying UI/Simulation switching
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            //the x-axis of your mouse plane is the y-axis of your camera
            float yDeltaAngle = MouseSensitivity * fdt * localInputs.Mouse_X;
            // the y-axis of your mouse plane is actually the x-axis of your camera
            //Confused? Yeah I know me too

            float xDeltaAngle = MouseSensitivity * fdt * localInputs.Mouse_Y;
            internalEulers.y += yDeltaAngle;
            internalEulers.x -= xDeltaAngle;

            //Prevents you from spinning and seeing the world from an upside down perspective
            internalEulers.x = Mathf.Clamp(internalEulers.x, -89.5F, 89.5F);

        }

        /* Movement */
        float rawMoveSpeed = MoveFactor;

        if (Input.GetKey(KeyCode.LeftShift))
            rawMoveSpeed *= 5.0F;

        Vector3 _move = (fixedStartRotation * new Vector3(internalInputs.Move_X, 0F, internalInputs.Move_Y));
        internalPosition += rawMoveSpeed * _move * fdt;

        /* Assigning */
        transform.position = internalPosition;
        transform.rotation = Quaternion.Euler(internalEulers);
    }

}
