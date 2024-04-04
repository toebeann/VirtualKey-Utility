using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Straitjacket
{
    internal sealed class VirtualKey : MonoBehaviour
    {
        [DllImport("USER32.dll")]
        private static extern short GetKeyState(VK nVirtKey);
        private const int KEY_PRESSED = 0x8000;

        public static bool GetKey(VK vk) => PlatformHelper.Is(Platform.Windows) && GetKeyInternal(vk);
        private static bool GetKeyInternal(VK vk)
        {
            try
            {
                return Convert.ToBoolean(GetKeyState(vk) & KEY_PRESSED);
            }
            catch (EntryPointNotFoundException e)
            {
                Console.WriteLine($"[VirtualKey] USER32.dll not found:{Environment.NewLine}[VirtualKey] {e.Message}");
                return false;
            }
        }

        public static bool GetKeyUp(VK vk) =>
            Instance.enabled &&
            Instance.keyStates[vk].Frame == Time.frameCount &&
            !Instance.keyStates[vk].Down;

        public static bool GetKeyDown(VK vk) =>
            Instance.enabled &&
            Instance.keyStates[vk].Frame == Time.frameCount &&
            Instance.keyStates[vk].Down;

        public static bool GetKey(KeyCode keyCode) => PlatformHelper.Is(Platform.Windows)
            ? GetKeyInternal(keyCode.ToVK())
            : Input.GetKey(keyCode);

        public static bool GetKeyUp(KeyCode keyCode) => PlatformHelper.Is(Platform.Windows)
            ? GetKeyUp(keyCode.ToVK())
            : Input.GetKeyUp(keyCode);

        public static bool GetKeyDown(KeyCode keyCode) => PlatformHelper.Is(Platform.Windows)
            ? GetKeyDown(keyCode.ToVK())
            : Input.GetKeyUp(keyCode);

        public static bool GetKeyFromFirstFrame(VK vk) => Instance.enabled && Instance.firstFrameKeyStates[vk].Down;
        public static bool GetKeyFromFirstFrame(KeyCode keyCode) => GetKeyFromFirstFrame(keyCode.ToVK());

        private static VirtualKey instance;
        private static VirtualKey Instance => instance == null
            ? instance = new GameObject("VirtualKey").AddComponent<VirtualKey>()
            : instance;

        private void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(this);
            }
            else if (PlatformHelper.Is(Platform.Windows))
            {
                foreach (var vk in (VK[])Enum.GetValues(typeof(VK)))
                {
                    var down = GetKey(vk);
                    firstFrameKeyStates[vk] = keyStates[vk] = new State { Down = down, Frame = -1 };
                }
            }
        }
        private struct State
        {
            public bool Down;
            public int Frame;
        }
        private readonly Dictionary<VK, State> firstFrameKeyStates = [];
        private readonly Dictionary<VK, State> keyStates = [];

        private void OnEnable()
        {
            if (!PlatformHelper.Is(Platform.Windows)) enabled = false;
        }

        private void LateUpdate()
        {
            foreach (var vk in (VK[])Enum.GetValues(typeof(VK)))
            {
                var previousState = keyStates[vk];
                var down = GetKey(vk);
                var keyState = new State { Down = down };
                var frame = Time.frameCount + 1;
                if (down != previousState.Down && frame > previousState.Frame)
                {
                    keyState.Frame = frame;
                }
                keyStates[vk] = keyState;
            }
        }
    }
}
