using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;
using iText.Layout;
using iText.Layout.Element;
using System.Security.Authentication;
using Org.BouncyCastle.Utilities.Encoders;

namespace ByteDesApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //List<string> cipher_text = new List<string>();
        private List<string> roundKeyBinary = new List<string>();
        private List<string> roundKeyHex = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void encryptBtn_Click(object sender, RoutedEventArgs e)
        {
            string cleanedText = string.Empty;

            // Open a file dialog to select a PDF file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            openFileDialog.Title = "Select a PDF file";

            if (openFileDialog.ShowDialog() == true)
            {
                string text;
                string pdfFilePath = openFileDialog.FileName;

                // Extract text from the selected PDF
                try
                {
                    StringBuilder extractedText = new StringBuilder();
                    using (PdfReader pdfReader = new PdfReader(pdfFilePath))
                    using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                    {
                        for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                        {
                            extractedText.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page)));
                            extractedText.Append("\n"); // Add newline between pages for better formatting
                        }
                    }

                    // Convert StringBuilder to a string
                    text = extractedText.ToString();

                    // Remove unnecessary spaces using Regex
                    // This will replace multiple spaces, newlines, and tabs with a single space
                    cleanedText = Regex.Replace(text, @"\s+", " ").Trim();

                    Console.WriteLine("Cleaned Text extracted from PDF: " + cleanedText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Modify this to use the first 8 characters of the user's name
                string keyString = "0E329232EA6D0D73";
                string binaryString = DesUtils.Hex2Bin(keyString);

                Console.WriteLine("Key in binary is: " + binaryString);

                string key = DesUtils.Permute(binaryString, DesTables.keyp, 56);
                Console.WriteLine($"Key after permutation: {key} and it's size is {key.Length}");

                string left = key.Substring(0, 28);
                string right = key.Substring(28, 28);

                for (int i = 0; i < 16; i++)
                {
                    left = DesUtils.ShiftLeft(left, DesTables.ShiftTable[i]);
                    right = DesUtils.ShiftLeft(right, DesTables.ShiftTable[i]);

                    string combinedString = left + right;
                    string roundKey = DesUtils.Permute(combinedString, DesTables.KeyComp, 48);
                    roundKeyBinary.Add(roundKey);
                    roundKeyHex.Add(DesUtils.Bin2Hex(roundKey));
                }

                Console.WriteLine("Encryption: ");

                int blockSize = 8;

                // Split the text into 8-byte blocks
                List<string> blocks = new List<string>();
                for (int i = 0; i < cleanedText.Length; i += blockSize)
                {
                    string block = cleanedText.Substring(i, Math.Min(blockSize, cleanedText.Length - i));
                    blocks.Add(block);
                }

                // Add padding to the last block if necessary
                int lastBlockSize = blocks[^1].Length; // ^1 is the last index in C#
                                                       // If the last block isn't 8 bytes, pad it
                if (lastBlockSize < blockSize)
                {
                    int paddingNeeded = blockSize - lastBlockSize;
                    blocks[^1] = blocks[^1].PadRight(blockSize, (char)paddingNeeded);
                }

                List<string> hexBlocks = new List<string>();
                foreach (var block in blocks)
                {
                    StringBuilder hexBlock = new StringBuilder();
                    foreach (char c in block)
                    {
                        hexBlock.AppendFormat("{0:x2} ", (int)c); // Convert char to hex (2 digits)
                    }
                    hexBlocks.Add(hexBlock.ToString().Trim()); // Add the hex block, remove trailing space
                }

                Console.WriteLine("Hex blocks are: ");

                foreach (var hex in hexBlocks)
                {
                    Console.WriteLine(hex);
                }

                List<string> cipher_text = new List<string>();
                foreach (var block in hexBlocks)
                {
                    // Remove spaces in the block and prepare it for encryption
                    string pt = string.Join("", block.Split(' '));

                    // Encrypt the block and get the result as a binary string
                    List<string> encryptedBinary = DesUtils.Encrypt(pt, roundKeyBinary, roundKeyHex);

                    // Convert the encrypted binary string to hexadecimal and add it to cipher_text
                    foreach (var binaryBlock in encryptedBinary)
                    {
                        string hexCipher = DesUtils.Bin2Hex(binaryBlock);
                        cipher_text.Add(hexCipher);
                    }
                }

                //MessageBox.Show("CipherText list filled with hexadecimal encryption values");
                //Console.WriteLine("CipherText list filled with hexadecimal encryption values");

                Console.WriteLine("Cipher text content below: ");
                foreach (var cipher in cipher_text)
                {
                    Console.WriteLine(cipher);
                }

                string keyFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pdfFilePath) ?? ".", "Keys.txt");

                // Save the keys to a file
                using (StreamWriter writer = new StreamWriter(keyFilePath))
                {
                    foreach (string k in roundKeyHex)
                    {
                        writer.WriteLine(k); // Save each round key as a separate line
                    }
                }

                MessageBox.Show($"Encryption completed! Round keys saved to: {keyFilePath}");


                WritePdfClass.WritePdf(cipher_text, pdfFilePath);
            }
        }


        private void decryptBtn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Decryption:");
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select a PDF file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string keyFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(openFileDialog.FileName) ?? ".", "Keys.txt");

                if (File.Exists(keyFilePath))
                {
                    roundKeyHex.Clear();
                    roundKeyBinary.Clear();

                    // Load the round keys from the file
                    using (StreamReader reader = new StreamReader(keyFilePath))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            roundKeyHex.Add(line);
                            roundKeyBinary.Add(DesUtils.Hex2Bin(line)); // Convert the hex key to binary and add to roundKeyBinary
                        }
                    }

                    Console.WriteLine("Round keys successfully loaded for decryption.");
                }
                else
                {
                    MessageBox.Show($"Round key file not found: {keyFilePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Reverse the round keys for decryption
                List<string> roundKeyBinaryRev = new List<string>(roundKeyBinary);
                roundKeyBinaryRev.Reverse();
                List<string> roundKeyHexRev = new List<string>(roundKeyHex);
                roundKeyHexRev.Reverse();
                List<string> decryptedBlocks = new List<string>();

                // List to store 64-bit (8-byte) blocks of cleaned text in original notation
                List<string> cipher_text = new List<string>();

                try
                {
                    string text;
                    string pdfFilePath = openFileDialog.FileName;

                    StringBuilder extractedText = new StringBuilder();
                    using (PdfReader pdfReader = new PdfReader(pdfFilePath))
                    using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                    {
                        for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                        {
                            extractedText.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page)));
                        }
                    }

                    text = extractedText.ToString();

                    // Step 1: Remove all spaces, newlines, and tabs
                    string cleanedText = Regex.Replace(text, @"\s+", "");

                    Console.WriteLine("Cleaned Text extracted from PDF:");
                    Console.WriteLine(cleanedText);

                    // Step 2: Split the cleaned text into 8-byte (64-bit) blocks
                    for (int i = 0; i < cleanedText.Length; i += 16)
                    {
                        string block = cleanedText.Substring(i, Math.Min(16, cleanedText.Length - i));

                        // Ensure block is 16 characters long (64 bits).
                        if (block.Length < 16)
                        {
                            block = block.PadRight(16, '0'); // Pad with zeroes.
                        }

                        cipher_text.Add(block);
                    }

                    Console.WriteLine("Cipher Text Blocks:");
                    foreach (var block in cipher_text)
                    {
                        Console.WriteLine(block);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                foreach (var cipher in cipher_text)
                {
                    if (cipher.Length != 16)
                    {
                        Console.WriteLine($"Skipping invalid block: {cipher} (length {cipher.Length})");
                        continue;
                    }

                    try
                    {
                        List<string> decryptedBinaryList = DesUtils.Encrypt(cipher, roundKeyBinaryRev, roundKeyHexRev);
                        foreach (var decryptedBinary in decryptedBinaryList)
                        {
                            string decryptedHex = DesUtils.Bin2Hex(decryptedBinary);
                            decryptedBlocks.Add(decryptedHex);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error decrypting block {cipher}: {ex.Message}");
                    }
                }

                // Convert hex values back to readable plaintext
                StringBuilder plaintext = new StringBuilder();
                foreach (var hexValue in decryptedBlocks)
                {
                    for (int i = 0; i < hexValue.Length; i += 2)
                    {
                        // Convert each hex pair to a character
                        string hexChar = hexValue.Substring(i, 2);
                        int charCode = Convert.ToInt32(hexChar, 16);
                        plaintext.Append((char)charCode);
                    }
                }

                // Remove padding from the last block
                int lastChar = plaintext.Length > 0 ? plaintext[plaintext.Length - 1] : 0;
                if (lastChar > 0 && lastChar <= 8) // Assuming block size is 8 bytes
                {
                    int paddingLength = lastChar;
                    plaintext.Remove(plaintext.Length - paddingLength, paddingLength);
                }

                // Display the result
                Console.WriteLine("PlainText string representation: ");
                Console.Write(plaintext.ToString());

                // Save the decrypted content to a PDF file
                string decryptedFilePath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(openFileDialog.FileName) ?? ".",
                    "DecryptedContent.pdf"
                );
                WriteDecryptedPdf(new List<string> { plaintext.ToString() }, decryptedFilePath);
                MessageBox.Show($"Decrypted content saved to PDF at: {decryptedFilePath}");
            }
        }

        public static void WriteDecryptedPdf(List<string> decryptedContent, string filePath)
        {
            try
            {
                // Create a PdfWriter object
                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    // Create a PdfDocument object
                    using (PdfDocument pdfDoc = new PdfDocument(writer))
                    {
                        Document document = new Document(pdfDoc);

                        // Write the decrypted content to the PDF
                        foreach (var line in decryptedContent)
                        {
                            // Add the text to the PDF
                            document.Add(new iText.Layout.Element.Paragraph(line));
                        }
                    }
                }
                Console.WriteLine("Decrypted content successfully written to PDF.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing decrypted PDF: {ex.Message}");
            }
        }

    }
}