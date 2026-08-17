using AzureTools.Models;
using System;

namespace AzureTools.Stores
{
    public interface IAzureToolsStore
    {
        void EnableEncryption();

        AppProfile GetProfile();
        TenantProfile[] GetTenants();



        void SaveProfile(AppProfile profile);

        void AddOrUpdateTenant(TenantProfile tenant);



        void DeleteTenant(TenantProfile tenant);



        bool ExistTenant(string tenantId);

        ProjectInfo[] GetProjects();
        ProjectInfo GetProjectById(Guid id);
        void AddProject(ProjectInfo project);
        void UpdateProject(ProjectInfo project);
        void DeleteProject(ProjectInfo project);
        bool ExistProject(string code);
        bool ExistProject(Guid id, string code);

        void DiscardChanges();
        void Save();
        TenantProfile GetTenant(string tenantId);
    }
}
