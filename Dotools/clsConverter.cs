using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Dotools
{
    /// <summary>
    /// A static class dedicated to general-purpose data type conversions.
    /// It is intended for use wherever conversion between different data types is required within the application.
    /// </summary>
    public static class clsConverter
    {
        /// <summary>
        /// Computes the SHA-256 hash of the input string and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="Input">The input string to hash.</param>
        /// <returns>A hexadecimal string representing the SHA-256 hash of the input.</returns>
        public static string ComputeHash(string Input)
        {
            using (SHA256 SHA = SHA256.Create())
            {
                byte[] HashByte = SHA.ComputeHash(Encoding.UTF8.GetBytes(Input));

                return BitConverter.ToString(HashByte).Replace("-", "");
            }
        }

        /// <summary>
        /// Converts an Image object to a byte array in PNG format.
        /// </summary>
        /// <param name="Img">The Image object to convert.</param>
        /// <returns>A byte array representing the image in PNG format.</returns>
        public static byte[] ToBytes(Image Img)
        {
            using (Img)
            {
                using (MemoryStream memorystream = new MemoryStream())
                {
                    Img.Save(memorystream, ImageFormat.Png);

                    return memorystream.ToArray();
                }
            }
        }

        /// <summary>
        /// Converts a byte array to an Image object.
        /// </summary>
        /// <param name="Input">The byte array representing the image data.</param>
        /// <returns>An Image object created from the byte array.</returns>
        public static Image ToImage(byte[] Input)
        {
            using (MemoryStream InputStream = new MemoryStream(Input))
            {
                Image Img = Image.FromStream(InputStream);

                return new Bitmap(Img);
            }
        }
    }
}
