using System;
using System.Text;

namespace AzureTools.Cli
{
    class AzureCliClientBuilder : DefaultCliClientBuilder, IAzureCliClientBuilder
    {
        public AzureCliClientBuilder(string azPath) : base(azPath)
        {

        }

        public IAppContainerClient GetAppContainerClient(string name, string resourceGroup)
        {

            return new AppContainerClient(FileName, name, resourceGroup);
        }

        public IContainerRegistrieClient GetContainerRegistrieClient(string name, string resourceGroup)
        {
            return new ContainerRegistrieClient(FileName, name);
        }

        public new AzureCliClientBuilder SetArgs(string arguments)
        {
            SetArgs(arguments);
            return this;
        }



        public new AzureCliClientBuilder SetWorkingDirectory(string path)
        {

            base.SetWorkingDirectory(path);
            return this;
        }

        public new AzureCliClientBuilder SetEncoding(Encoding encoding)
        {
            base.SetEncoding(encoding);
            return this;
        }

        public new AzureCliClientBuilder SetStdOut(Action<string> handler)
        {
            base.SetStdOut(handler);
            return this;
        }

        public new AzureCliClientBuilder SetStdErr(Action<string> handler)
        {
            base.SetStdErr(handler);
            return this;
        }

        public new AzureCliClientBuilder EnableStandardInput()
        {
            base.EnableStandardInput();
            return this;
        }
    }
}
