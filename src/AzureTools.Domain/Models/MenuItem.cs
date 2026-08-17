using System;
using System.ComponentModel.DataAnnotations;

namespace AzureTools.Models
{
    public class MenuItem
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        [Key]
        public Guid Id { get; set; }
        public string Code { get; set; }

        public string Name { get; set; }

        public byte[] Icons { get; set; }
        public string Form { get; set; }

        #endregion
    }
}
