using System;
using System.ComponentModel.DataAnnotations;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
