using Avalonia.Threading;
using AzureTools.ViewModels;
using AzureTools.Views;
using System.Collections.Specialized;
using Toolkit.Modules.Comm.ViewModels.CliStreamSession;

namespace Toolkit.Modules.Comm.Views.CliStreamSession
{
    public partial class CliStreamSessionView : View
    {
        public CliStreamSessionView(CliStreamSessionViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent(true);
            viewModel.OutputLines.CollectionChanged += OnOutputLinesChanged;
        }


        private void OnOutputLinesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.UIThread.Post(() => LogScroller.ScrollToEnd(), DispatcherPriority.Background);
        }
    }
}
