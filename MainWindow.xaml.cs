using Microsoft.Win32;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace ProjectInfosecDes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Allow dragging the window by clicking and holding the left mouse button
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void encryptBtn_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to choose a PDF
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Select a PDF"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string pdfContent = ReadPdf(filePath);

                // Define the path for the output text file
                string outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), "ExtractedContent.txt");

                try
                {
                    // Write the content to the text file
                    File.WriteAllText(outputFilePath, pdfContent);
                    MessageBox.Show("PDF content has been saved to: " + outputFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error writing to file: " + ex.Message);
                }

                // Terminate the application
                Environment.Exit(0);
            }
        }

        private string ReadPdf(string filePath)
        {
            string content = string.Empty;

            try
            {
                // Use iTextSharp to extract text
                using (PdfReader reader = new PdfReader(filePath))
                {
                    using (PdfDocument pdfDoc = new PdfDocument(reader))
                    {
                        int numberOfPages = pdfDoc.GetNumberOfPages();

                        for (int i = 1; i <= numberOfPages; i++)
                        {
                            var page = pdfDoc.GetPage(i);
                            var strategy = new LocationTextExtractionStrategy();
                            var contentBytes = PdfTextExtractor.GetTextFromPage(page, strategy);
                            content += contentBytes + " "; // Concatenate text from each page with a single space
                        }
                    }
                }

                // Remove excessive spaces and newline characters between words
                content = Regex.Replace(content, @"\s+", " ").Trim(); // Keeps only single spaces between words
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading PDF: " + ex.Message);
            }

            return content;
        }

        private void decryptBtn_Click(object sender, RoutedEventArgs e)
        {
            // Decrypt button functionality can go here
        }
    }
}
