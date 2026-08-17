using CommunityToolkit.Mvvm.ComponentModel;

namespace AzureTools.ViewModels
{
    public abstract partial class EditorViewModel<T> : DialogViewModel
    {
        #region Constructors
        protected EditorViewModel(EditorOption<T> editorOption)
        {
            System.ArgumentNullException.ThrowIfNull(editorOption);
            ExistingItem = editorOption.Option;
            IsEditMode = editorOption.IsEditMode;
        }
        #endregion

        #region Fields & Properties
        [ObservableProperty]
        private bool _isEditMode;
        public T ExistingItem { get; }

        #endregion

    }
}
