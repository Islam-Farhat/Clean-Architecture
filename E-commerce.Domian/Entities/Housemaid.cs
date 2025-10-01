using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian.Entities
{
    public class Housemaid
    {
        private Housemaid() { }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }

        public static Result<Housemaid> Instance(string name, string address, string phoneNumber, string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(name)) return Result.Failure<Housemaid>("Name empty!");
            if (string.IsNullOrWhiteSpace(address)) return Result.Failure<Housemaid>("Address empty!");
            if (string.IsNullOrWhiteSpace(phoneNumber)) return Result.Failure<Housemaid>("PhoneNumber empty!");
            if (string.IsNullOrWhiteSpace(imageUrl)) return Result.Failure<Housemaid>("Image empty!");

            var housemaid = new Housemaid()
            {
                Address = address,
                PhoneNumber = phoneNumber,
                ImageUrl = imageUrl,
                Name = name,
            };

            return housemaid;
        }

        public Result Update(string name, string address, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(name)) return Result.Failure<Housemaid>("Name empty!");
            if (string.IsNullOrWhiteSpace(address)) return Result.Failure<Housemaid>("Address empty!");
            if (string.IsNullOrWhiteSpace(phoneNumber)) return Result.Failure<Housemaid>("PhoneNumber empty!");

            this.Name = name;
            this.Address = address;
            this.PhoneNumber = phoneNumber;

            return Result.Success();
        }
        public void UpdateImage(string imageUrl)
        {
            this.ImageUrl = imageUrl;
        }
    }
}
