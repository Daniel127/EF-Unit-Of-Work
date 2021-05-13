using System;
using System.ComponentModel.DataAnnotations;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models
{
    public sealed class Product
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }

        private bool Equals(Product other)
        {
            return Id.Equals(other.Id) && Name == other.Name && Price == other.Price;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Product)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Price);
        }
    }
}
