using System;

namespace com.first_person_horror_kit.Core
{
    public interface IInteractionPromptSource
    {
        event Action PromptChanged;
    }
}