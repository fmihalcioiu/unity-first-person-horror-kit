namespace com.first_person_horror_kit.Core
{
    public interface IConfirmableInteractable
    {
        string GetConfirmationPrompt();
        string GetPrimaryOptionText();
        string GetSecondaryOptionText();
    }
}