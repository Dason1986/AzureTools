using System;
using System.ComponentModel.DataAnnotations;

namespace Toolkit.Modules.Models
{
    public class ProjectInfo
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        [Key]
        public Guid Id { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        #endregion
    }
}
