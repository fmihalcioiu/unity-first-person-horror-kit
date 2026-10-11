using System;

namespace com.first_person_horror_kit.Core
{
    public static class GameplayState
    {
        public static bool isInputBlocked { get; private set; }

        public static event Action<bool> InputBlockChanged;

        public static void SetInputBlocked(bool isBlocked)
        {
            if (isInputBlocked == isBlocked)
                return;

            isInputBlocked = isBlocked;
            InputBlockChanged?.Invoke(isInputBlocked);
        }
    }
}