namespace com.first_person_horror_kit.Core
{
    public interface IInteractable
    {
        void Interact();
        string GetPrompt();
        string GetDisplayName();
    }
}