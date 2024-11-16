using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteDesApp
{
    internal class DesUtils
    {
        public static string Hex2Bin(string hex)
        {
            // Convert the hex string to a byte array
            byte[] key = new byte[hex.Length / 2];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            // Convert each byte to a binary string and join them
            StringBuilder binaryString = new StringBuilder();
            foreach (byte b in key)
            {
                binaryString.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return binaryString.ToString();
        }

        public static string Bin2Hex(string binary)
        {
            // Ensure the binary string's length is a multiple of 4
            int remainder = binary.Length % 4;
            if (remainder != 0)
            {
                binary = binary.PadLeft(binary.Length + (4 - remainder), '0');
            }

            // Convert every 4 binary digits to a hexadecimal digit
            string hexString = "";
            for (int i = 0; i < binary.Length; i += 4)
            {
                string fourBits = binary.Substring(i, 4);
                int decimalValue = Convert.ToInt32(fourBits, 2);
                hexString += decimalValue.ToString("X");
            }

            return hexString;
        }

        public static string Permute(string k, int[] arr, int n)
        {
            string permutation = "";
            for (int i = 0; i < n; i++)
            {
                permutation += k[arr[i] - 1];
            }
            return permutation;
        }

        public static string ShiftLeft(string k, int nthShifts)
        {
            string s = "";
            for (int i = 0; i < nthShifts; i++)
            {
                for (int j = 1; j < k.Length; j++)
                {
                    s += k[j];
                }
                s += k[0];
                k = s;
                s = "";
            }
            return k;
        }

        public static string Xor(string a, string b)
        {
            string result = "";
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == b[i])
                {
                    result += "0";
                }
                else
                {
                    result += "1";
                }
            }
            return result;
        }

        public static List<String> Encrypt(string pt, List<string> roundKeyBinary, List<string> roundKeyHex)
        {
            // Remove spaces in pt to ensure proper hex conversion
            pt = Hex2Bin(pt.Replace(" ", ""));

            // Initial Permutation
            pt = Permute(pt, DesTables.initial_permute, 64);
            Console.WriteLine($"After initial permutation: {Bin2Hex(pt)}");

            // Splitting into left and right halves (PlainText)
            string left = pt.Substring(0, 32);
            string right = pt.Substring(32, 32);
            for (int i = 0; i < 16; i++)
            {
                // Expansion D-box: Expanding the 32 bits data into 48 bits
                string rightExpanded = Permute(right, DesTables.exp_d, 48);

                // XOR RoundKey[i] and right_expanded
                string xorX = Xor(rightExpanded, roundKeyBinary[i]);

                // S-boxes: substituting the value from s-box table by calculating row and column
                string sboxStr = "";
                for (int j = 0; j < 8; j++)
                {
                    int row = Convert.ToInt32($"{xorX[j * 6]}{xorX[j * 6 + 5]}", 2);
                    int col = Convert.ToInt32($"{xorX[j * 6 + 1]}{xorX[j * 6 + 2]}{xorX[j * 6 + 3]}{xorX[j * 6 + 4]}", 2);
                    int val = DesTables.sbox[j, row, col];
                    sboxStr += Convert.ToString(val, 2).PadLeft(4, '0');
                }

                // Straight D-box: After substituting rearranging the bits
                sboxStr = Permute(sboxStr, DesTables.per, 32);

                // XOR left and sbox_str
                string result = Xor(left, sboxStr);
                left = result;

                // Swapper
                if (i != 15)
                {
                    string temp = right;
                    right = left;
                    left = temp;
                }
                Console.WriteLine($"Round {i + 1}: {Bin2Hex(left)} {Bin2Hex(right)} {roundKeyHex[i]}");
            }

            // Combination
            string combine = left + right;

            // Final permutation
            string cipher_text = "";
            cipher_text = Permute(combine, DesTables.final_perm, 64);
            return new List<string> { cipher_text };
        }
    }
}
