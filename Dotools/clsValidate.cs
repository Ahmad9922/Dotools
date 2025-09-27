using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dotools
{
    /// <summary>
    /// Provides static methods for validating common data formats such as email addresses and phone numbers.
    /// </summary>
    public static class clsValidate
    {
        /// <summary>
        /// Validates whether the specified string is in a valid email address format.
        /// </summary>
        /// <param name="Email">The email address string to validate.</param>
        /// <returns>True if the string is a valid email address; otherwise, false.</returns>
        public static bool ValidateEmail(string Email)
        {
            Regex regex = new Regex("[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}");

            return regex.IsMatch(Email);
        }

        /// <summary>
        /// Validates whether the specified string is in a valid phone number format (US style).
        /// </summary>
        /// <param name="Phone">The phone number string to validate.</param>
        /// <returns>True if the string is a valid phone number; otherwise, false.</returns>
        public static bool ValidatePhone(string Phone)
        {
            Regex regex = new Regex("\\(?\\d{3}\\)?[-.\\s]?\\d{3}[-.\\s]?\\d{4}");

            return regex.IsMatch(Phone);
        }
    }
}
