using System;

namespace AzureTools
{
    class MenuItem : IMenuItem
    {

        public string Code { get; set; }



        public ModuleItem Module { get; set; }

        public string Name { get; set; }

        public byte[] Icon { get; set; }
        public Type ViewModelType { get; set; }

        public int Order { get; set; }

        IModuleItem IMenuItem.Module => Module;


    }
}
