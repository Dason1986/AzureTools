using AzureTools.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTools.Stores
{
    public class DefaultAzureToolsStore : IAzureToolsStore
    {
        #region Constructors

        public DefaultAzureToolsStore(IStoreContext storeContext)
        {
            this.storeContext = storeContext;
        }

        #endregion

        #region Fields & Properties

        private readonly IStoreContext storeContext;

        #endregion 




        public AppProfile GetProfile()
        {
            SettingOptioinItem[] settings = storeContext.GetSet<SettingOptioinItem>().Where(x => x.SettingGroup == nameof(AppProfile)).ToArray();
            if (settings.Length == 0) return null;
            AppProfile profile = new AppProfile(settings.ToDictionary(x => x.SettingKey, x => x.SettingValue, StringComparer.OrdinalIgnoreCase));

            return profile;
        }
        public TenantProfile[] GetTenants()
        {
            return storeContext.GetSet<TenantProfile>().ToArray();
        }
        public TenantProfile GetTenant(string tenantId)
        {
            return storeContext.GetSet<TenantProfile>().FirstOrDefault(x => x.TenantId == tenantId);
        }



        public void SaveProfile(AppProfile profile)
        {
            List<SettingOptioinItem> list = storeContext.GetSet<SettingOptioinItem>()
                .Where(x => x.SettingGroup == nameof(AppProfile)).ToList();
            IDictionary<string, string> values = profile.GetValues();

            foreach (KeyValuePair<string, string> value in values)
            {

                SettingOptioinItem exist = list.FirstOrDefault(x => x.SettingKey == value.Key);
                if (exist != null)
                {
                    exist.SettingValue = value.Value;
                    storeContext.Update(exist);
                }
                else
                {
                    storeContext.Add(new SettingOptioinItem
                    {
                        SettingGroup = nameof(AppProfile),
                        SettingKey = value.Key,
                        SettingValue = value.Value
                    });
                }
            }



        }
        public void EnableEncryption()
        {
            storeContext.EnableEncryption();
        }
        public void AddOrUpdateTenant(TenantProfile tenant)
        {
            TenantProfile existing = storeContext.GetSet<TenantProfile>()
                .FirstOrDefault(x => x.TenantId == tenant.TenantId);
            if (existing != null)
            {
                existing.Name = tenant.Name;
                existing.Code = tenant.Code;
                existing.Credential = tenant.Credential;
                existing.ClientId = tenant.ClientId;
                existing.CertificateRawData = tenant.CertificateRawData;
                existing.Password = tenant.Password;

                storeContext.Update(existing);
            }
            else
            {

                storeContext.Add(tenant);
            }

        }

        public void AddTenant(TenantProfile tenant)
        {

            storeContext.Add(tenant);

        }

        public void DeleteTenant(TenantProfile tenant)
        {


            storeContext.Remove(tenant);



        }



        public bool ExistTenant(string tenantId)
        {
            return storeContext.GetSet<TenantProfile>().Any(x => x.TenantId == tenantId);
        }

        public ProjectInfo[] GetProjects()
        {
            return storeContext.GetSet<ProjectInfo>().ToArray();
        }

        public ProjectInfo GetProjectById(Guid id)
        {
            return storeContext.GetSet<ProjectInfo>().FirstOrDefault(x => x.Id == id);
        }

        public void AddProject(ProjectInfo project)
        {
            project.Id = Guid.NewGuid();
            storeContext.Add(project);
        }

        public void UpdateProject(ProjectInfo project)
        {
            storeContext.Update(project);
        }

        public void DeleteProject(ProjectInfo project)
        {
            storeContext.Remove(project);
        }

        public bool ExistProject(string code)
        {
            return storeContext.GetSet<ProjectInfo>().Any(x => x.Code == code);
        }

        public bool ExistProject(Guid id, string code)
        {
            return storeContext.GetSet<ProjectInfo>().Any(x => x.Code == code && x.Id != id);
        }

        public void Save()
        {
            storeContext.SaveChanges();
        }

        public void DiscardChanges()
        {
            storeContext.DiscardChanges();
        }

    }
}
