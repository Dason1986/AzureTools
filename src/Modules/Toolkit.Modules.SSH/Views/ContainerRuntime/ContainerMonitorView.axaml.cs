using AzureTools.ViewModels;
using AzureTools.Views;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using Toolkit.Modules.SSH.ViewModels.ContainerRuntime;

namespace Toolkit.Modules.SSH.Views.ContainerRuntime
{
    public partial class ContainerMonitorView : View
    {
        private const int MaxStatsPoints = 60;
        private readonly ContainerMonitorViewModel _vm;
        private DataStreamer _cpuStreamer;
        private DataStreamer _memStreamer;
        private DataStreamer _diskReadStreamer;
        private DataStreamer _diskWriteStreamer;
        private DataStreamer _netRxStreamer;
        private DataStreamer _netTxStreamer;

        public ContainerMonitorView(ContainerMonitorViewModel viewModel, ViewModelContext context) : base(viewModel, context)
        {
            InitializeComponent(true);
            _vm = viewModel;
            _vm.StatsDataUpdated += OnStatsDataUpdated;
            Loaded += OnLoaded;
            Closed += OnClosed;
        }



        private void OnLoaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_cpuStreamer != null) return;
            _cpuStreamer = InitPercentPlot(CpuPlot, Colors.Red);
            _memStreamer = InitPercentPlot(MemPlot, Colors.Blue);
            _diskReadStreamer = InitRatePlot(DiskPlot, Colors.Orange);
            _diskWriteStreamer = AddStreamer(DiskPlot, Colors.Green);
            _netRxStreamer = InitRatePlot(NetPlot, Colors.Cyan);
            _netTxStreamer = AddStreamer(NetPlot, Colors.Purple);
        }

        private void OnClosed(object sender, System.EventArgs e)
        {
            _vm.StatsDataUpdated -= OnStatsDataUpdated;
        }

        private static DataStreamer InitPercentPlot(AvaPlot plot, Color color)
        {
            InitializePlot(plot);
            plot.Plot.Axes.SetLimitsX(0, MaxStatsPoints);
            plot.Plot.Axes.SetLimitsY(0, 100);
            return AddStreamer(plot, color);
        }

        private static DataStreamer InitRatePlot(AvaPlot plot, Color color)
        {
            InitializePlot(plot);
            plot.Plot.Axes.ContinuouslyAutoscale = true;
            return AddStreamer(plot, color);
        }

        private static void InitializePlot(AvaPlot plot)
        {
            plot.Plot.Axes.Title.IsVisible = false;
            plot.Plot.Axes.Bottom.IsVisible = false;
            plot.Plot.Grid.IsVisible = false;
        }

        private static DataStreamer AddStreamer(AvaPlot plot, Color color)
        {
            DataStreamer streamer = plot.Plot.Add.DataStreamer(MaxStatsPoints);
            streamer.LineColor = color;
            streamer.LineWidth = 2;
            streamer.ManageAxisLimits = false;
            return streamer;
        }

        private void OnStatsDataUpdated()
        {
            if (_cpuStreamer == null) return;
            _cpuStreamer.Add(_vm.LatestCpuPercent);
            _memStreamer.Add(_vm.LatestMemoryPercent);
            _diskReadStreamer.Add(_vm.LatestDiskReadRate);
            _diskWriteStreamer.Add(_vm.LatestDiskWriteRate);
            _netRxStreamer.Add(_vm.LatestNetworkRxRate);
            _netTxStreamer.Add(_vm.LatestNetworkTxRate);
            CpuPlot.Refresh();
            MemPlot.Refresh();
            DiskPlot.Refresh();
            NetPlot.Refresh();
        }
    }
}
