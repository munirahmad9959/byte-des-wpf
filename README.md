# ByteDesApp

ByteDesApp is a Windows Presentation Foundation (WPF) application designed to demonstrate and work with the Data Encryption Standard (DES) algorithm. It supports data encryption and exports the results into a PDF file.

---

![ByteDesApp Screenshot](./Images/AppUi.png)

---

## Features

- **DES Encryption**: Implements DES encryption using predefined tables (e.g., permutation and S-box tables).
- **PDF Export**: Outputs the encrypted content to a PDF file.
- **Customizable**: DES tables can be modified, and the application can be extended for additional features.

---

## Technologies Used

- **.NET Framework** with WPF
- **iText7 Library** for generating PDF files
- **C#** for backend logic and DES implementation

---

## Setup and Installation

### Prerequisites

- Visual Studio 2019 or later
- .NET Framework
- iText7 NuGet Package

## Steps:

1. Clone the repository:
   ```bash
   git clone https://github.com/your-repository/ByteDesApp.git

2. Open the solution file:
   ```bash
   git clone https://github.com/munirahmad9959/byte-des-wpf.git

3. Restore NuGet packages:
   ```bash
   Navigate to Tools > NuGet Package Manager > Manage NuGet Packages for 
   Solution.
   Search for iText7 and install it.
4. Build the project:
   Use the following shortcut to build:
   ```bash
   Ctrl + Shift + B

5. Usage Instructions:

   - **Encrypt Data**
     1. Open the application after building it.
     2. Input your plaintext and key into the respective fields.
     3. Click the Encrypt button to generate the encrypted result.
   - **Export to PDF**
     1. Use the Export to PDF option to save the encrypted content as a PDF 
        file.
   - **Output Location**
     1. The generated PDF file will be saved in the directory you specify.
## Code Structure

### Key Files and Classes

- **DesTables.cs**  
  Contains static DES tables required for operations:

- **WritePdfClass.cs**  
  A utility class for:
  1. Creating a PDF file using the iText7 library.
  2. Formatting and writing the encrypted content to the PDF.

- **MainWindow.xaml**  
  UI layout for the application.

- **MainWindow.xaml.cs**  
  Logic for encrypting data and triggering PDF generation.

- **DesUtils.cs**  
  Contains encryption functions and other utilities used in the encryption and decryption process.
