using System;
using System.ComponentModel.DataAnnotations;

namespace AzureTools.Models
{
    public class ModuleItem
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        [Key]
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        #endregion
    }
}
