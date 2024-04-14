//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.8.1
//     from Assets/Codes/HighlightTool/InputController/PlayerControls.inputactions
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
using UnityEngine;

public partial class @PlayerControls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""d9886de1-e17e-4b51-9d9a-6328fd4a235f"",
            ""actions"": [
                {
                    ""name"": ""MouseMove"",
                    ""type"": ""PassThrough"",
                    ""id"": ""914dfaba-2b11-4840-8092-2482efbab5df"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LeftMouseClick"",
                    ""type"": ""Value"",
                    ""id"": ""813e5d1a-8266-43e4-ac20-cf55412871de"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LeftMouseClickAndMove"",
                    ""type"": ""Value"",
                    ""id"": ""6289a388-b327-4c32-8794-f08f3b11c5a3"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""63c0e504-4469-4fab-b95f-97380e1a91ba"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""PositionAt"",
                    ""id"": ""9e0f2f32-8e72-4d73-8625-6d9071d8111e"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClick"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""74eb2836-cc06-4702-acbb-09985e34e460"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""58440a82-9e62-4c6e-a648-49f40e433946"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""StartPosition"",
                    ""id"": ""fd570852-0be0-4ec3-b7d1-753304d1226f"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClickAndMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""0b9fe2e9-79b3-437e-84ed-1d9e34cdbde9"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClickAndMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""6de52760-4dad-4111-8281-415fa22bfab1"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClickAndMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""Placement"",
            ""id"": ""003142fe-1f39-4629-8685-060c1c8e74b7"",
            ""actions"": [
                {
                    ""name"": ""RightMouseClickAndMove"",
                    ""type"": ""Value"",
                    ""id"": ""d32fb7de-7e1c-42e9-9e6c-ad3679670f8c"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""SpaceKey"",
                    ""type"": ""Button"",
                    ""id"": ""1bc57889-f4c6-4152-9195-c755ab2d7615"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""LeftMouseCancel"",
                    ""type"": ""Button"",
                    ""id"": ""a1238fdd-487b-4e27-9f40-b389601bd30a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Placement"",
                    ""id"": ""912a1aba-046d-45bb-9a56-3f53d537542d"",
                    ""path"": ""OneModifier(overrideModifiersNeedToBePressedFirst=true)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightMouseClickAndMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""f1deddef-2b91-4008-8d28-8848a393bbf2"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightMouseClickAndMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""dbaae1af-f839-475a-9d59-e49ebb0c63d9"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightMouseClickAndMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""80136c70-d599-49b4-9e62-fbaa17168c14"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SpaceKey"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""One Modifier"",
                    ""id"": ""47b037eb-9882-473b-84e6-01cff7e66e4b"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseCancel"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""35091024-52c7-44a2-9ce6-4883a62d5468"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseCancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""90b8108a-24b8-4ab1-a66d-ad70514dfd43"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseCancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""Selection"",
            ""id"": ""1e577cc4-e689-41a0-a0b9-415299fceaba"",
            ""actions"": [
                {
                    ""name"": ""MouseMove"",
                    ""type"": ""Value"",
                    ""id"": ""00707b96-636b-4164-a0b8-ea068bc79277"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LeftMouseClickAndMove"",
                    ""type"": ""Value"",
                    ""id"": ""dd6e0d1d-9ad4-4380-af29-8faaac9edddf"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LockSelection"",
                    ""type"": ""Button"",
                    ""id"": ""bf2bbfe6-3c47-4c1d-ac9a-139c41ab3608"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""094885c4-80ef-401c-91ae-8882c846ff01"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""StartPosition"",
                    ""id"": ""321882df-bc12-4673-a7bd-43fd0ca0f2b8"",
                    ""path"": ""OneModifier"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClickAndMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""modifier"",
                    ""id"": ""17b28ba8-6f8f-45bb-9d6e-9f8771b0885c"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClickAndMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""binding"",
                    ""id"": ""1d56e3e3-e479-4289-a620-c7729012974e"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMouseClickAndMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""6495801d-1b3d-47f2-b453-cceea0275bc1"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LockSelection"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""RegimentAbility"",
            ""id"": ""f53388a7-3b8c-4c25-8fa4-64da1ca060b7"",
            ""actions"": [
                {
                    ""name"": ""MarchRun"",
                    ""type"": ""Button"",
                    ""id"": ""7c66b5b7-a5bd-4ee5-be55-71e9566a5bef"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AutoFire"",
                    ""type"": ""Button"",
                    ""id"": ""56c2dd0f-ee79-44ec-a468-506634d72357"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""cb4db4d2-835a-484e-9d58-d86707869bf6"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MarchRun"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7044bb11-f362-4e3c-b8b4-b486531e5dbc"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""AutoFire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_MouseMove = m_Player.FindAction("MouseMove", throwIfNotFound: true);
        m_Player_LeftMouseClick = m_Player.FindAction("LeftMouseClick", throwIfNotFound: true);
        m_Player_LeftMouseClickAndMove = m_Player.FindAction("LeftMouseClickAndMove", throwIfNotFound: true);
        // Placement
        m_Placement = asset.FindActionMap("Placement", throwIfNotFound: true);
        m_Placement_RightMouseClickAndMove = m_Placement.FindAction("RightMouseClickAndMove", throwIfNotFound: true);
        m_Placement_SpaceKey = m_Placement.FindAction("SpaceKey", throwIfNotFound: true);
        m_Placement_LeftMouseCancel = m_Placement.FindAction("LeftMouseCancel", throwIfNotFound: true);
        // Selection
        m_Selection = asset.FindActionMap("Selection", throwIfNotFound: true);
        m_Selection_MouseMove = m_Selection.FindAction("MouseMove", throwIfNotFound: true);
        m_Selection_LeftMouseClickAndMove = m_Selection.FindAction("LeftMouseClickAndMove", throwIfNotFound: true);
        m_Selection_LockSelection = m_Selection.FindAction("LockSelection", throwIfNotFound: true);
        // RegimentAbility
        m_RegimentAbility = asset.FindActionMap("RegimentAbility", throwIfNotFound: true);
        m_RegimentAbility_MarchRun = m_RegimentAbility.FindAction("MarchRun", throwIfNotFound: true);
        m_RegimentAbility_AutoFire = m_RegimentAbility.FindAction("AutoFire", throwIfNotFound: true);
    }

    ~@PlayerControls()
    {
        Debug.Assert(!m_Player.enabled, "This will cause a leak and performance issues, PlayerControls.Player.Disable() has not been called.");
        Debug.Assert(!m_Placement.enabled, "This will cause a leak and performance issues, PlayerControls.Placement.Disable() has not been called.");
        Debug.Assert(!m_Selection.enabled, "This will cause a leak and performance issues, PlayerControls.Selection.Disable() has not been called.");
        Debug.Assert(!m_RegimentAbility.enabled, "This will cause a leak and performance issues, PlayerControls.RegimentAbility.Disable() has not been called.");
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

    // Player
    private readonly InputActionMap m_Player;
    private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
    private readonly InputAction m_Player_MouseMove;
    private readonly InputAction m_Player_LeftMouseClick;
    private readonly InputAction m_Player_LeftMouseClickAndMove;
    public struct PlayerActions
    {
        private @PlayerControls m_Wrapper;
        public PlayerActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseMove => m_Wrapper.m_Player_MouseMove;
        public InputAction @LeftMouseClick => m_Wrapper.m_Player_LeftMouseClick;
        public InputAction @LeftMouseClickAndMove => m_Wrapper.m_Player_LeftMouseClickAndMove;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
            @MouseMove.started += instance.OnMouseMove;
            @MouseMove.performed += instance.OnMouseMove;
            @MouseMove.canceled += instance.OnMouseMove;
            @LeftMouseClick.started += instance.OnLeftMouseClick;
            @LeftMouseClick.performed += instance.OnLeftMouseClick;
            @LeftMouseClick.canceled += instance.OnLeftMouseClick;
            @LeftMouseClickAndMove.started += instance.OnLeftMouseClickAndMove;
            @LeftMouseClickAndMove.performed += instance.OnLeftMouseClickAndMove;
            @LeftMouseClickAndMove.canceled += instance.OnLeftMouseClickAndMove;
        }

        private void UnregisterCallbacks(IPlayerActions instance)
        {
            @MouseMove.started -= instance.OnMouseMove;
            @MouseMove.performed -= instance.OnMouseMove;
            @MouseMove.canceled -= instance.OnMouseMove;
            @LeftMouseClick.started -= instance.OnLeftMouseClick;
            @LeftMouseClick.performed -= instance.OnLeftMouseClick;
            @LeftMouseClick.canceled -= instance.OnLeftMouseClick;
            @LeftMouseClickAndMove.started -= instance.OnLeftMouseClickAndMove;
            @LeftMouseClickAndMove.performed -= instance.OnLeftMouseClickAndMove;
            @LeftMouseClickAndMove.canceled -= instance.OnLeftMouseClickAndMove;
        }

        public void RemoveCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerActions @Player => new PlayerActions(this);

    // Placement
    private readonly InputActionMap m_Placement;
    private List<IPlacementActions> m_PlacementActionsCallbackInterfaces = new List<IPlacementActions>();
    private readonly InputAction m_Placement_RightMouseClickAndMove;
    private readonly InputAction m_Placement_SpaceKey;
    private readonly InputAction m_Placement_LeftMouseCancel;
    public struct PlacementActions
    {
        private @PlayerControls m_Wrapper;
        public PlacementActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @RightMouseClickAndMove => m_Wrapper.m_Placement_RightMouseClickAndMove;
        public InputAction @SpaceKey => m_Wrapper.m_Placement_SpaceKey;
        public InputAction @LeftMouseCancel => m_Wrapper.m_Placement_LeftMouseCancel;
        public InputActionMap Get() { return m_Wrapper.m_Placement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlacementActions set) { return set.Get(); }
        public void AddCallbacks(IPlacementActions instance)
        {
            if (instance == null || m_Wrapper.m_PlacementActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlacementActionsCallbackInterfaces.Add(instance);
            @RightMouseClickAndMove.started += instance.OnRightMouseClickAndMove;
            @RightMouseClickAndMove.performed += instance.OnRightMouseClickAndMove;
            @RightMouseClickAndMove.canceled += instance.OnRightMouseClickAndMove;
            @SpaceKey.started += instance.OnSpaceKey;
            @SpaceKey.performed += instance.OnSpaceKey;
            @SpaceKey.canceled += instance.OnSpaceKey;
            @LeftMouseCancel.started += instance.OnLeftMouseCancel;
            @LeftMouseCancel.performed += instance.OnLeftMouseCancel;
            @LeftMouseCancel.canceled += instance.OnLeftMouseCancel;
        }

        private void UnregisterCallbacks(IPlacementActions instance)
        {
            @RightMouseClickAndMove.started -= instance.OnRightMouseClickAndMove;
            @RightMouseClickAndMove.performed -= instance.OnRightMouseClickAndMove;
            @RightMouseClickAndMove.canceled -= instance.OnRightMouseClickAndMove;
            @SpaceKey.started -= instance.OnSpaceKey;
            @SpaceKey.performed -= instance.OnSpaceKey;
            @SpaceKey.canceled -= instance.OnSpaceKey;
            @LeftMouseCancel.started -= instance.OnLeftMouseCancel;
            @LeftMouseCancel.performed -= instance.OnLeftMouseCancel;
            @LeftMouseCancel.canceled -= instance.OnLeftMouseCancel;
        }

        public void RemoveCallbacks(IPlacementActions instance)
        {
            if (m_Wrapper.m_PlacementActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlacementActions instance)
        {
            foreach (var item in m_Wrapper.m_PlacementActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlacementActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlacementActions @Placement => new PlacementActions(this);

    // Selection
    private readonly InputActionMap m_Selection;
    private List<ISelectionActions> m_SelectionActionsCallbackInterfaces = new List<ISelectionActions>();
    private readonly InputAction m_Selection_MouseMove;
    private readonly InputAction m_Selection_LeftMouseClickAndMove;
    private readonly InputAction m_Selection_LockSelection;
    public struct SelectionActions
    {
        private @PlayerControls m_Wrapper;
        public SelectionActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseMove => m_Wrapper.m_Selection_MouseMove;
        public InputAction @LeftMouseClickAndMove => m_Wrapper.m_Selection_LeftMouseClickAndMove;
        public InputAction @LockSelection => m_Wrapper.m_Selection_LockSelection;
        public InputActionMap Get() { return m_Wrapper.m_Selection; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(SelectionActions set) { return set.Get(); }
        public void AddCallbacks(ISelectionActions instance)
        {
            if (instance == null || m_Wrapper.m_SelectionActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_SelectionActionsCallbackInterfaces.Add(instance);
            @MouseMove.started += instance.OnMouseMove;
            @MouseMove.performed += instance.OnMouseMove;
            @MouseMove.canceled += instance.OnMouseMove;
            @LeftMouseClickAndMove.started += instance.OnLeftMouseClickAndMove;
            @LeftMouseClickAndMove.performed += instance.OnLeftMouseClickAndMove;
            @LeftMouseClickAndMove.canceled += instance.OnLeftMouseClickAndMove;
            @LockSelection.started += instance.OnLockSelection;
            @LockSelection.performed += instance.OnLockSelection;
            @LockSelection.canceled += instance.OnLockSelection;
        }

        private void UnregisterCallbacks(ISelectionActions instance)
        {
            @MouseMove.started -= instance.OnMouseMove;
            @MouseMove.performed -= instance.OnMouseMove;
            @MouseMove.canceled -= instance.OnMouseMove;
            @LeftMouseClickAndMove.started -= instance.OnLeftMouseClickAndMove;
            @LeftMouseClickAndMove.performed -= instance.OnLeftMouseClickAndMove;
            @LeftMouseClickAndMove.canceled -= instance.OnLeftMouseClickAndMove;
            @LockSelection.started -= instance.OnLockSelection;
            @LockSelection.performed -= instance.OnLockSelection;
            @LockSelection.canceled -= instance.OnLockSelection;
        }

        public void RemoveCallbacks(ISelectionActions instance)
        {
            if (m_Wrapper.m_SelectionActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ISelectionActions instance)
        {
            foreach (var item in m_Wrapper.m_SelectionActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_SelectionActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public SelectionActions @Selection => new SelectionActions(this);

    // RegimentAbility
    private readonly InputActionMap m_RegimentAbility;
    private List<IRegimentAbilityActions> m_RegimentAbilityActionsCallbackInterfaces = new List<IRegimentAbilityActions>();
    private readonly InputAction m_RegimentAbility_MarchRun;
    private readonly InputAction m_RegimentAbility_AutoFire;
    public struct RegimentAbilityActions
    {
        private @PlayerControls m_Wrapper;
        public RegimentAbilityActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @MarchRun => m_Wrapper.m_RegimentAbility_MarchRun;
        public InputAction @AutoFire => m_Wrapper.m_RegimentAbility_AutoFire;
        public InputActionMap Get() { return m_Wrapper.m_RegimentAbility; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(RegimentAbilityActions set) { return set.Get(); }
        public void AddCallbacks(IRegimentAbilityActions instance)
        {
            if (instance == null || m_Wrapper.m_RegimentAbilityActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_RegimentAbilityActionsCallbackInterfaces.Add(instance);
            @MarchRun.started += instance.OnMarchRun;
            @MarchRun.performed += instance.OnMarchRun;
            @MarchRun.canceled += instance.OnMarchRun;
            @AutoFire.started += instance.OnAutoFire;
            @AutoFire.performed += instance.OnAutoFire;
            @AutoFire.canceled += instance.OnAutoFire;
        }

        private void UnregisterCallbacks(IRegimentAbilityActions instance)
        {
            @MarchRun.started -= instance.OnMarchRun;
            @MarchRun.performed -= instance.OnMarchRun;
            @MarchRun.canceled -= instance.OnMarchRun;
            @AutoFire.started -= instance.OnAutoFire;
            @AutoFire.performed -= instance.OnAutoFire;
            @AutoFire.canceled -= instance.OnAutoFire;
        }

        public void RemoveCallbacks(IRegimentAbilityActions instance)
        {
            if (m_Wrapper.m_RegimentAbilityActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IRegimentAbilityActions instance)
        {
            foreach (var item in m_Wrapper.m_RegimentAbilityActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_RegimentAbilityActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public RegimentAbilityActions @RegimentAbility => new RegimentAbilityActions(this);
    public interface IPlayerActions
    {
        void OnMouseMove(InputAction.CallbackContext context);
        void OnLeftMouseClick(InputAction.CallbackContext context);
        void OnLeftMouseClickAndMove(InputAction.CallbackContext context);
    }
    public interface IPlacementActions
    {
        void OnRightMouseClickAndMove(InputAction.CallbackContext context);
        void OnSpaceKey(InputAction.CallbackContext context);
        void OnLeftMouseCancel(InputAction.CallbackContext context);
    }
    public interface ISelectionActions
    {
        void OnMouseMove(InputAction.CallbackContext context);
        void OnLeftMouseClickAndMove(InputAction.CallbackContext context);
        void OnLockSelection(InputAction.CallbackContext context);
    }
    public interface IRegimentAbilityActions
    {
        void OnMarchRun(InputAction.CallbackContext context);
        void OnAutoFire(InputAction.CallbackContext context);
    }
}
