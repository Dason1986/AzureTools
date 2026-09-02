namespace AzureTools.Services
{
    public class PromptDialogOption
    {
        public string DefaultValue { get; set; }

        public bool IsPassword { get; set; }

        public string[] Options { get; set; }

        public char PasswordChar { get; set; } = '*';

        public static implicit operator PromptDialogOption(string str)
        {
            return new PromptDialogOption { DefaultValue = str };
        }
        public static readonly PromptDialogOption PasswordOption = new PromptDialogOption { IsPassword = true, PasswordChar = '*' };

    }
}
