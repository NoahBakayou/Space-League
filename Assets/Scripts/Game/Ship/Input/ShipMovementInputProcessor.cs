using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;
using Game.Configs.Ship;
using Utils.Maths;
using Utils.Vectors;
using Utils.ScriptableObjects.Variables;
using static UnityEngine.InputSystem.InputAction;
using System.Diagnostics;

namespace Game.Ship.PlayerInput {
    public class ShipMovementInputProcessor : IDisposable {
        public float Throttle { get; private set; }
        public float Strafe { get; private set; }
        public float Rotation { get; private set; }
        public float Roll { get; private set; }
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }
        public bool NitroRequired { get; private set; }
        public Vector3 MouseInputRaw { get; private set; }

        private PID pitchPID = new PID();
        private PID yawPID = new PID();
        private PID rollPID = new PID();
        private Vector2 MouseInputClamped;

        private FloatVariable shipThrottle;
        private ShipConfig config;
        private Transform transform;
        private Camera mainCamera;

        private Mouse mouse;
        private InputAction strafeAction;
        private InputAction rotationAction;
        private InputAction nitroAction;

        // Strafe Dash System
        private float strafeDashValue = 0f;
        private float strafeDashVelocity = 0f;

        // Tuning values
        private const float dashStrength = 6.5f; // How hard the ship kicks sideways
        private const float dashReturnSpeed = 5f; // How fast it returns to zero

        [SerializeField] private float dashNitroCost = 1.5f; // Nitro cost per A/D tap

        private NitroBooster nitroBooster;

        private float aTapTimer = 0f;
        private float dTapTimer = 0f;
        private const float tapThreshold = 0.15f; // 150 ms to classify tap

        public ShipMovementInputProcessor(ShipConfig config, FloatVariable shipThrottle, Transform transform, Camera camera) {
            this.config = config;
            this.shipThrottle = shipThrottle;
            this.transform = transform;
            this.mainCamera = camera;

            mouse = Mouse.current;
            var shipInputActions = new ShipInputActions();
            strafeAction = shipInputActions.Ship.StrafeAxis;
            rotationAction = shipInputActions.Ship.RotationAxis;
            nitroAction = shipInputActions.Ship.Nitro;

            strafeAction.Enable();
            rotationAction.Enable();
            nitroAction.Enable();

            nitroAction.performed += SetNitroRequired;
            shipThrottle.OnValueChanged += OnSliderThrottleChanged;
        }

        public ShipMovementInputProcessor(ShipConfig config, FloatVariable shipThrottle, Transform transform, Camera camera, NitroBooster nitroBooster)
        {
            this.config = config;
            this.shipThrottle = shipThrottle;
            this.transform = transform;
            this.mainCamera = camera;

            mouse = Mouse.current;
            var shipInputActions = new ShipInputActions();
            strafeAction = shipInputActions.Ship.StrafeAxis;
            rotationAction = shipInputActions.Ship.RotationAxis;
            nitroAction = shipInputActions.Ship.Nitro;

            strafeAction.Enable();
            rotationAction.Enable();
            nitroAction.Enable();

            nitroAction.performed += SetNitroRequired;
            shipThrottle.OnValueChanged += OnSliderThrottleChanged;
            this.nitroBooster = nitroBooster;
        }

        public void SetNitroBooster(NitroBooster booster)
        {
            nitroBooster = booster;
        }

        public void Update(float deltaTime) {
            UpdateKeyboardThrottle(deltaTime);
            UpdateInputAxes(deltaTime);
            FindMousePosition(out Vector3 worldPos);
            CalculateTurn(worldPos);
            CalculateRoll(deltaTime);
        }

        public void Dispose() {
            strafeAction.Disable();
            rotationAction.Disable();
            nitroAction.Disable();

            nitroAction.performed -= SetNitroRequired;
            shipThrottle.OnValueChanged -= OnSliderThrottleChanged;
            
            strafeAction.Dispose();
            rotationAction.Dispose();
            nitroAction.Dispose();
        }
        // Original A/D handling 
        /**private void UpdateInputAxes(float deltaTime) {
            float strafe = strafeAction.ReadValue<float>();
            float rotation = rotationAction.ReadValue<float>();
            Strafe = Mathf.MoveTowards(Strafe, strafe, deltaTime * config.strafeSensitivity);
            Rotation = Mathf.MoveTowards(Rotation, rotation, deltaTime * config.rorationSensitivity);
        } */

        // New A/D handling 
        private void UpdateInputAxes(float deltaTime)
        {
            aTapTimer += deltaTime;
            dTapTimer += deltaTime;

            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                aTapTimer = 0f; // reset timer
            }

            if (Keyboard.current.aKey.wasReleasedThisFrame)
            {
                if (aTapTimer < tapThreshold && TryConsumeDashNitro())
                    ApplyDash(-dashStrength);
            }

            if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                dTapTimer = 0f;
            }

            if (Keyboard.current.dKey.wasReleasedThisFrame)
            {
                if (dTapTimer < tapThreshold && TryConsumeDashNitro())
                    ApplyDash(dashStrength);
            }

            strafeDashValue = Mathf.SmoothDamp(strafeDashValue, 0f, ref strafeDashVelocity, 1f / dashReturnSpeed, Mathf.Infinity, deltaTime);

            float analogStrafe = strafeAction.ReadValue<float>();
            float rotation = rotationAction.ReadValue<float>();

            bool userIsHoldingStrafeKey =
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.dKey.isPressed;

            if (!userIsHoldingStrafeKey)
            {
                // Apply dash impulse (decays over time)
                Strafe = strafeDashValue;
            }
            else
            {
                // user is drifting normally
                Strafe = Mathf.MoveTowards(Strafe, analogStrafe, deltaTime * config.strafeSensitivity);
            }
            Rotation = Mathf.MoveTowards(Rotation, rotation, deltaTime * config.rorationSensitivity);
        }

        private void ApplyDash(float strength)
        {
            strafeDashValue = strength;
            strafeDashVelocity = 0f; 
        }

        // Helper function to consume nitro for a dash
        private bool TryConsumeDashNitro()
        {
            if (nitroBooster.CurrentNitro < dashNitroCost)
            {
                return false; 
            }
            nitroBooster.TryConsumeNitro(dashNitroCost);
            return true; 
        }

        private void UpdateKeyboardThrottle(float deltaTime) {
            if (Input.GetKey(KeyCode.W)) {
                Throttle = Mathf.MoveTowards(Throttle, 1f, deltaTime * config.throttleSensitivity);
                shipThrottle.SetValue(Throttle, false);
            }
            if (Input.GetKey(KeyCode.S)) {
                Throttle = Mathf.MoveTowards(Throttle, 0f, deltaTime * config.throttleSensitivity);
                shipThrottle.SetValue(Throttle, false);
            }
        }

        private void FindMousePosition(out Vector3 gotoPos) {
            Vector2 mouseInput = mouse.position.ReadValue();
            MouseInputRaw = mouseInput;
            Vector3 mousePos = new Vector3(mouseInput.x, mouseInput.y, 1000f);
            gotoPos = mainCamera.ScreenToWorldPoint(mousePos);
            
            var inputY = (mouseInput.y - (Screen.height * 0.5f)) / (Screen.height * 0.5f);
            var inputX = (mouseInput.x - (Screen.width * 0.5f)) / (Screen.width * 0.5f);

            this.MouseInputClamped = new Vector2(inputX, inputY).ClampNeg1To1();
        }

        private void CalculateTurn(Vector3 gotoPos) {
            Vector3 localGotoPos = transform.InverseTransformVector(gotoPos - transform.position).normalized;
            float dt = Time.deltaTime;
            
            Pitch = Mathf.Clamp(localGotoPos.x * config.pitchSensitivity, -config.pitchSensitivity, config.pitchSensitivity);
            Pitch = pitchPID.GetOutput(config.pitchYawKp, config.pitchYawKi, config.pitchYawKd, Pitch, dt);
            
            Yaw = Mathf.Clamp(-localGotoPos.y * config.yawSensitivity, -config.yawSensitivity, config.yawSensitivity);
            Yaw = yawPID.GetOutput(config.pitchYawKp, config.pitchYawKi, config.pitchYawKd, Yaw, dt);
        }

        private void CalculateRoll(float deltaTime) {
            float inputRotation = Input.GetAxis("Roll");
            float rollInfluence = -MouseInputClamped.x * Throttle;
            float yInfluence = MathfExtensions.InverseRelationship(3f, MouseInputClamped.y * 10f);
            yInfluence = Mathf.Clamp(Mathf.Abs(yInfluence), float.MinValue, 1f);
            rollInfluence *= yInfluence;
            rollInfluence *= config.autoRollSensitivity;

            if (inputRotation != 0) Roll = inputRotation * config.customRollSensitivity;
            else Roll = rollPID.GetOutput(config.rollKp, config.rollKi, config.rollKd, rollInfluence, deltaTime);
        }

        private void OnSliderThrottleChanged(float value) {
            DOTween.To(() => Throttle, x => Throttle = x, value, config.throttleSensitivity);
        }

        private void SetNitroRequired(CallbackContext callback) {
            NitroRequired = !NitroRequired;
        }
    }
}