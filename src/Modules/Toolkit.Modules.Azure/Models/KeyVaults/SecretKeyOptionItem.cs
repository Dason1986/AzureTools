using System;

namespace Toolkit.Modules.Azure.Models.KeyVaults
{
    public class SecretKeyOptionItem
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        public string Key { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public ValidationOptions Option { get; set; }
        public string[] Tags { get; set; }
        public Guid ProjectId { get; set; }

        #endregion
    }
}
