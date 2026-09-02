namespace AzureTools.Cli
{
    public static class CliExtensions
    {
        public static IContainerShell CreateShell(this ICliStreamSession session)
        {
            ContainerInteractiveSession interactive = new ContainerInteractiveSession(session);
            return new ContainerShell(interactive);
        }


    }
}
