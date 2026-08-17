namespace AzureTools.ViewModels
{
    public abstract class EditorOption<TOption> : IViewModelOption
    {
        public TOption Option { get; set; }
        public bool IsEditMode { get; set; }
    }
}
