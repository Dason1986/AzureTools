using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public class SshFeatureMenuItem
    {
        public string Name { get; }

        public Geometry Icon { get; }

        public IAsyncRelayCommand OpenCommand { get; }

        public SshFeatureMenuItem(SshHostFeature feature, Func<string, Task> openFeature)
        {
            Name = feature.Name;
            Icon = StreamGeometry.Parse(feature.Icon);
            OpenCommand = new AsyncRelayCommand(() => openFeature(feature.Code));
        }
    }
}
