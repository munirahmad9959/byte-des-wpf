using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace ByteDesApp
{
    internal class WritePdfClass
    {
        public static void WritePdf(List<string> cipherText, string pdfFilePath)
        {
            try
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(pdfFilePath) ?? ".";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Define the output PDF file path
                string outputFilePath = Path.Combine(directoryPath, "EncryptedContent.pdf");

                // Check if the file already exists and delete it
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                Console.WriteLine($"Starting PDF creation at: {outputFilePath}");

                // Initialize PdfWriter
                using (PdfWriter writer = new PdfWriter(outputFilePath))
                {
                    // Initialize PdfDocument
                    using (PdfDocument pdfDoc = new PdfDocument(writer))
                    {
                        // Initialize Document for layout
                        using (Document document = new Document(pdfDoc))
                        {
                            StringBuilder concatenatedText = new StringBuilder();

                            // Process cipherText to concatenate all the strings
                            int count = 0; // To count how many strings we are processing
                            foreach (string hexString in cipherText)
                            {
                                if (string.IsNullOrWhiteSpace(hexString))
                                {
                                    Console.WriteLine("Skipping empty or null text...");
                                    continue;
                                }

                                string formattedText = FormatHexString(hexString);
                                concatenatedText.Append(formattedText + " ");  // Add a space between each hex string

                                // Debugging: Show progress every 1000 strings
                                count++;
                                if (count % 1000 == 0)
                                {
                                    Console.WriteLine($"Processing string {count}/{cipherText.Count}...");
                                }
                            }

                            // Add concatenated text as a single paragraph
                            document.Add(new Paragraph(concatenatedText.ToString().Trim()));

                            // Debugging: Check the final concatenated length
                            Console.WriteLine($"Concatenated text length: {concatenatedText.Length}");
                        }
                    }
                }

                // Final success message
                MessageBox.Show($"Encryption completed! PDF successfully created at: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating the PDF: {ex.Message}");
                Console.WriteLine("Detailed Stack Trace: ");
                Console.WriteLine(ex.ToString());
            }
        }


        // Helper function to format the hex string as a continuous line (no spaces between characters)
        private static string FormatHexString(string hexString)
        {
            // Remove spaces and return the continuous string
            return hexString.Replace(" ", ""); // Ensures no spaces, and all letters are uppercase
        }

    }
}
