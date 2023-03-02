using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    // always make extensions static
    public static class DateTimeExtensions
    {
        public static int CalculateAge(this DateOnly dob) {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age)) {
                // this means the person has not had their birthday yet, so minus a year
                age--;
            }
            return age;
        }
    }
}