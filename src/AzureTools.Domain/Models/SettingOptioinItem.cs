using System.ComponentModel.DataAnnotations;

namespace AzureTools.Models
{
    public class SettingOptioinItem
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        [Key]
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string SettingGroup { get; set; }

        #endregion
    }
}
