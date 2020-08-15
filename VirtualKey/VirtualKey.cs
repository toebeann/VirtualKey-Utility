using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Straitjacket
{
    internal class VirtualKey : MonoBehaviour
    {
        [DllImport("USER32.dll")]
        private static extern short GetKeyState(VK nVirtKey);
        private const int KEY_PRESSED = 0x8000;

        private static bool user32Failed = false;
        public static bool GetKey(VK vk)
        {
            if (!user32Failed)
            {
                try
                {
                    return Convert.ToBoolean(GetKeyState(vk) & KEY_PRESSED);
                }
                catch (EntryPointNotFoundException e)
                {
                    user32Failed = true;
                    Console.WriteLine($"[VirtualKey] USER32.dll not found:{Environment.NewLine}[VirtualKey] {e.Message}");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool GetKeyUp(VK vk) => Instance.keyStates[vk].Frame == Time.frameCount && !Instance.keyStates[vk].Down;
        public static bool GetKeyDown(VK vk) => Instance.keyStates[vk].Frame == Time.frameCount && Instance.keyStates[vk].Down;

        public static bool GetKey(KeyCode keyCode) => GetKey(keyCode.ToVK());
        public static bool GetKeyUp(KeyCode keyCode) => GetKeyUp(keyCode.ToVK());
        public static bool GetKeyDown(KeyCode keyCode) => GetKeyDown(keyCode.ToVK());

        public static bool GetKeyFromFirstFrame(VK vk) => Instance != null && Instance.firstFrameKeyStates[vk].Down;
        public static bool GetKeyFromFirstFrame(KeyCode keyCode) => GetKeyFromFirstFrame(keyCode.ToVK());

        private static VirtualKey instance = null;
        private static VirtualKey Instance => instance = instance ?? new GameObject("VirtualKey").AddComponent<VirtualKey>();
        private void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(this);
            }
            else
            {
                foreach (var vk in (VK[])Enum.GetValues(typeof(VK)))
                {
                    var down = GetKey(vk);
                    keyStates[vk] = new State { Down = down, Frame = -1 };
                }
                firstFrameKeyStates = new Dictionary<VK, State>(keyStates);
            }
        }
        private struct State
        {
            public bool Down;
            public int Frame;
        }
        private Dictionary<VK, State> firstFrameKeyStates = new Dictionary<VK, State>();
        private Dictionary<VK, State> keyStates = new Dictionary<VK, State>();

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
