//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.11.2
//     from Assets/InputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Services.Input
{
    public partial class @InputActions: IInputActionCollection2, IDisposable
    {
        public InputActionAsset asset { get; }
        public @InputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputActions"",
    ""maps"": [
        {
            ""name"": ""Combat"",
            ""id"": ""6ca0e278-f7a9-4b98-b0e3-c5170c4d7f48"",
            ""actions"": [
                {
                    ""name"": ""Horizontal"",
                    ""type"": ""Value"",
                    ""id"": ""d7a9888a-8158-486c-a50c-07603ee1fcdd"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Vertical"",
                    ""type"": ""Value"",
                    ""id"": ""fa6a3e23-84b5-4426-9eb1-b17ff9671c08"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""77cecef9-9334-4d1a-9082-8aaf0a719501"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Fire1"",
                    ""type"": ""Button"",
                    ""id"": ""a43d7d3b-5b92-453f-90b1-954041184152"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Fire2"",
                    ""type"": ""Button"",
                    ""id"": ""be8510cd-2016-4f74-b6a6-0961f50f2989"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Fire3"",
                    ""type"": ""Button"",
                    ""id"": ""5578d2ee-b1f6-4462-8b4c-8cbe36123f76"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Fire4"",
                    ""type"": ""Button"",
                    ""id"": ""4256deaf-4ef1-4be6-9d88-9356b7670ba9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Fire5"",
                    ""type"": ""Button"",
                    ""id"": ""868c0119-3ddd-4334-8ad6-af1929931187"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Fire6"",
                    ""type"": ""Button"",
                    ""id"": ""aec4fe01-85d6-4620-830a-0512709c4554"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""21c5a0a1-6b45-439d-b9a7-7e2924107586"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""06878a2d-e052-4769-8af9-9f3f5637e44d"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""56bd031e-9e5f-4ab9-afe2-0509cea35e61"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""7befb896-2c93-424d-b4e0-fec1eb31f933"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""f072ebb8-0efc-4289-88ea-d39313ed6cab"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""dd75185f-6401-4242-af05-41de7930dd78"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""bf20f9e1-7656-4e77-8156-1ff627960716"",
                    ""path"": ""<Gamepad>/dpad/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Vertical"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""18cbd78d-956b-42d3-bc8e-a80e29c3db35"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1e18c8b5-bc66-48b4-8adc-1067e74f81b1"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4545bdc6-8319-45e4-a2f2-55a05fa0dec8"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""29e43dc6-2f0b-4ea0-a655-0aca0ff61ad9"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1e947318-7635-43ca-a484-e8804974e694"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6e10e09f-c567-4fa0-bfd1-b84c1623038a"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bcbd6c71-4550-4857-9afa-467a6db4adeb"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""466b653f-0ec3-4d7b-934a-25af187d9892"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b24bef6f-5de4-4b8f-bce4-def078695b52"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""429a8027-69d5-4e24-9601-1b2ae8dff342"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire5"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""50313649-fc28-4089-b1d1-361c125b5805"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire5"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bf3742e2-5417-41de-b915-420cdefc13b3"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire6"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d5b93e35-826d-4859-9265-6787c0ccfec2"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire6"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f6661653-9695-4ee9-beb7-45c56c947c10"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""2d175525-47ab-4c5b-b3d3-902eabf4f260"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""2430a538-942e-4cd6-890f-1b251693db9f"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""fb8c49dc-f614-401f-adce-161431ee52f4"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""a023d7da-fe43-4bbe-830a-a1a8a3aac0a3"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""6d76f9f3-7baa-4fd4-a27f-471319b19946"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""87a03045-a1e1-4f89-9f13-2f408810ccca"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""e3d1e77e-52b5-4c13-9853-80ab21d52186"",
                    ""path"": ""<Gamepad>/dpad/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Horizontal"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Mouse"",
            ""id"": ""e11966d1-e681-4199-b9d6-ba4e239be991"",
            ""actions"": [
                {
                    ""name"": ""MouseLook"",
                    ""type"": ""PassThrough"",
                    ""id"": ""3eb12b50-1cfd-4427-9052-3880f7c8104b"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Thrust"",
                    ""type"": ""Button"",
                    ""id"": ""3da11a65-4b6a-463c-a456-dbb243f3e197"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Action1"",
                    ""type"": ""Button"",
                    ""id"": ""9ffe58d6-ec06-4c1d-b2de-171abb0c62c0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Action2"",
                    ""type"": ""Button"",
                    ""id"": ""a3898e9c-12d6-467b-89fc-c417c13ae81c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""381ed612-ac24-428f-b42c-b11886c10d9b"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseLook"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9dec5c3f-6b5d-4005-9b2e-4f32a12ee7ff"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Thrust"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""24e52ab1-1d92-45f1-9d75-adbc61052cd1"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Action1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""45597d66-2c98-4458-bd26-0d8b2d83d1eb"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Action2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // Combat
            m_Combat = asset.FindActionMap("Combat", throwIfNotFound: true);
            m_Combat_Horizontal = m_Combat.FindAction("Horizontal", throwIfNotFound: true);
            m_Combat_Vertical = m_Combat.FindAction("Vertical", throwIfNotFound: true);
            m_Combat_Move = m_Combat.FindAction("Move", throwIfNotFound: true);
            m_Combat_Fire1 = m_Combat.FindAction("Fire1", throwIfNotFound: true);
            m_Combat_Fire2 = m_Combat.FindAction("Fire2", throwIfNotFound: true);
            m_Combat_Fire3 = m_Combat.FindAction("Fire3", throwIfNotFound: true);
            m_Combat_Fire4 = m_Combat.FindAction("Fire4", throwIfNotFound: true);
            m_Combat_Fire5 = m_Combat.FindAction("Fire5", throwIfNotFound: true);
            m_Combat_Fire6 = m_Combat.FindAction("Fire6", throwIfNotFound: true);
            // Mouse
            m_Mouse = asset.FindActionMap("Mouse", throwIfNotFound: true);
            m_Mouse_MouseLook = m_Mouse.FindAction("MouseLook", throwIfNotFound: true);
            m_Mouse_Thrust = m_Mouse.FindAction("Thrust", throwIfNotFound: true);
            m_Mouse_Action1 = m_Mouse.FindAction("Action1", throwIfNotFound: true);
            m_Mouse_Action2 = m_Mouse.FindAction("Action2", throwIfNotFound: true);
        }

        ~@InputActions()
        {
            UnityEngine.Debug.Assert(!m_Combat.enabled, "This will cause a leak and performance issues, InputActions.Combat.Disable() has not been called.");
            UnityEngine.Debug.Assert(!m_Mouse.enabled, "This will cause a leak and performance issues, InputActions.Mouse.Disable() has not been called.");
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
        {
            return asset.FindAction(actionNameOrId, throwIfNotFound);
        }

        public int FindBinding(InputBinding bindingMask, out InputAction action)
        {
            return asset.FindBinding(bindingMask, out action);
        }

        // Combat
        private readonly InputActionMap m_Combat;
        private List<ICombatActions> m_CombatActionsCallbackInterfaces = new List<ICombatActions>();
        private readonly InputAction m_Combat_Horizontal;
        private readonly InputAction m_Combat_Vertical;
        private readonly InputAction m_Combat_Move;
        private readonly InputAction m_Combat_Fire1;
        private readonly InputAction m_Combat_Fire2;
        private readonly InputAction m_Combat_Fire3;
        private readonly InputAction m_Combat_Fire4;
        private readonly InputAction m_Combat_Fire5;
        private readonly InputAction m_Combat_Fire6;
        public struct CombatActions
        {
            private @InputActions m_Wrapper;
            public CombatActions(@InputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Horizontal => m_Wrapper.m_Combat_Horizontal;
            public InputAction @Vertical => m_Wrapper.m_Combat_Vertical;
            public InputAction @Move => m_Wrapper.m_Combat_Move;
            public InputAction @Fire1 => m_Wrapper.m_Combat_Fire1;
            public InputAction @Fire2 => m_Wrapper.m_Combat_Fire2;
            public InputAction @Fire3 => m_Wrapper.m_Combat_Fire3;
            public InputAction @Fire4 => m_Wrapper.m_Combat_Fire4;
            public InputAction @Fire5 => m_Wrapper.m_Combat_Fire5;
            public InputAction @Fire6 => m_Wrapper.m_Combat_Fire6;
            public InputActionMap Get() { return m_Wrapper.m_Combat; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(CombatActions set) { return set.Get(); }
            public void AddCallbacks(ICombatActions instance)
            {
                if (instance == null || m_Wrapper.m_CombatActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_CombatActionsCallbackInterfaces.Add(instance);
                @Horizontal.started += instance.OnHorizontal;
                @Horizontal.performed += instance.OnHorizontal;
                @Horizontal.canceled += instance.OnHorizontal;
                @Vertical.started += instance.OnVertical;
                @Vertical.performed += instance.OnVertical;
                @Vertical.canceled += instance.OnVertical;
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Fire1.started += instance.OnFire1;
                @Fire1.performed += instance.OnFire1;
                @Fire1.canceled += instance.OnFire1;
                @Fire2.started += instance.OnFire2;
                @Fire2.performed += instance.OnFire2;
                @Fire2.canceled += instance.OnFire2;
                @Fire3.started += instance.OnFire3;
                @Fire3.performed += instance.OnFire3;
                @Fire3.canceled += instance.OnFire3;
                @Fire4.started += instance.OnFire4;
                @Fire4.performed += instance.OnFire4;
                @Fire4.canceled += instance.OnFire4;
                @Fire5.started += instance.OnFire5;
                @Fire5.performed += instance.OnFire5;
                @Fire5.canceled += instance.OnFire5;
                @Fire6.started += instance.OnFire6;
                @Fire6.performed += instance.OnFire6;
                @Fire6.canceled += instance.OnFire6;
            }

            private void UnregisterCallbacks(ICombatActions instance)
            {
                @Horizontal.started -= instance.OnHorizontal;
                @Horizontal.performed -= instance.OnHorizontal;
                @Horizontal.canceled -= instance.OnHorizontal;
                @Vertical.started -= instance.OnVertical;
                @Vertical.performed -= instance.OnVertical;
                @Vertical.canceled -= instance.OnVertical;
                @Move.started -= instance.OnMove;
                @Move.performed -= instance.OnMove;
                @Move.canceled -= instance.OnMove;
                @Fire1.started -= instance.OnFire1;
                @Fire1.performed -= instance.OnFire1;
                @Fire1.canceled -= instance.OnFire1;
                @Fire2.started -= instance.OnFire2;
                @Fire2.performed -= instance.OnFire2;
                @Fire2.canceled -= instance.OnFire2;
                @Fire3.started -= instance.OnFire3;
                @Fire3.performed -= instance.OnFire3;
                @Fire3.canceled -= instance.OnFire3;
                @Fire4.started -= instance.OnFire4;
                @Fire4.performed -= instance.OnFire4;
                @Fire4.canceled -= instance.OnFire4;
                @Fire5.started -= instance.OnFire5;
                @Fire5.performed -= instance.OnFire5;
                @Fire5.canceled -= instance.OnFire5;
                @Fire6.started -= instance.OnFire6;
                @Fire6.performed -= instance.OnFire6;
                @Fire6.canceled -= instance.OnFire6;
            }

            public void RemoveCallbacks(ICombatActions instance)
            {
                if (m_Wrapper.m_CombatActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(ICombatActions instance)
            {
                foreach (var item in m_Wrapper.m_CombatActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_CombatActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public CombatActions @Combat => new CombatActions(this);

        // Mouse
        private readonly InputActionMap m_Mouse;
        private List<IMouseActions> m_MouseActionsCallbackInterfaces = new List<IMouseActions>();
        private readonly InputAction m_Mouse_MouseLook;
        private readonly InputAction m_Mouse_Thrust;
        private readonly InputAction m_Mouse_Action1;
        private readonly InputAction m_Mouse_Action2;
        public struct MouseActions
        {
            private @InputActions m_Wrapper;
            public MouseActions(@InputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @MouseLook => m_Wrapper.m_Mouse_MouseLook;
            public InputAction @Thrust => m_Wrapper.m_Mouse_Thrust;
            public InputAction @Action1 => m_Wrapper.m_Mouse_Action1;
            public InputAction @Action2 => m_Wrapper.m_Mouse_Action2;
            public InputActionMap Get() { return m_Wrapper.m_Mouse; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(MouseActions set) { return set.Get(); }
            public void AddCallbacks(IMouseActions instance)
            {
                if (instance == null || m_Wrapper.m_MouseActionsCallbackInterfaces.Contains(instance)) return;
                m_Wrapper.m_MouseActionsCallbackInterfaces.Add(instance);
                @MouseLook.started += instance.OnMouseLook;
                @MouseLook.performed += instance.OnMouseLook;
                @MouseLook.canceled += instance.OnMouseLook;
                @Thrust.started += instance.OnThrust;
                @Thrust.performed += instance.OnThrust;
                @Thrust.canceled += instance.OnThrust;
                @Action1.started += instance.OnAction1;
                @Action1.performed += instance.OnAction1;
                @Action1.canceled += instance.OnAction1;
                @Action2.started += instance.OnAction2;
                @Action2.performed += instance.OnAction2;
                @Action2.canceled += instance.OnAction2;
            }

            private void UnregisterCallbacks(IMouseActions instance)
            {
                @MouseLook.started -= instance.OnMouseLook;
                @MouseLook.performed -= instance.OnMouseLook;
                @MouseLook.canceled -= instance.OnMouseLook;
                @Thrust.started -= instance.OnThrust;
                @Thrust.performed -= instance.OnThrust;
                @Thrust.canceled -= instance.OnThrust;
                @Action1.started -= instance.OnAction1;
                @Action1.performed -= instance.OnAction1;
                @Action1.canceled -= instance.OnAction1;
                @Action2.started -= instance.OnAction2;
                @Action2.performed -= instance.OnAction2;
                @Action2.canceled -= instance.OnAction2;
            }

            public void RemoveCallbacks(IMouseActions instance)
            {
                if (m_Wrapper.m_MouseActionsCallbackInterfaces.Remove(instance))
                    UnregisterCallbacks(instance);
            }

            public void SetCallbacks(IMouseActions instance)
            {
                foreach (var item in m_Wrapper.m_MouseActionsCallbackInterfaces)
                    UnregisterCallbacks(item);
                m_Wrapper.m_MouseActionsCallbackInterfaces.Clear();
                AddCallbacks(instance);
            }
        }
        public MouseActions @Mouse => new MouseActions(this);
        public interface ICombatActions
        {
            void OnHorizontal(InputAction.CallbackContext context);
            void OnVertical(InputAction.CallbackContext context);
            void OnMove(InputAction.CallbackContext context);
            void OnFire1(InputAction.CallbackContext context);
            void OnFire2(InputAction.CallbackContext context);
            void OnFire3(InputAction.CallbackContext context);
            void OnFire4(InputAction.CallbackContext context);
            void OnFire5(InputAction.CallbackContext context);
            void OnFire6(InputAction.CallbackContext context);
        }
        public interface IMouseActions
        {
            void OnMouseLook(InputAction.CallbackContext context);
            void OnThrust(InputAction.CallbackContext context);
            void OnAction1(InputAction.CallbackContext context);
            void OnAction2(InputAction.CallbackContext context);
        }
    }
}
