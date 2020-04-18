using System;
using System.ComponentModel.DataAnnotations;

namespace QD.EntityFrameworkCore.UnitOfWork.UnitTests.Models
{
	public class Product
	{
		[Key]
		public Guid Id { get; set; }
		public string Name { get; set; }
		public int Price { get; set; }
	}
}
