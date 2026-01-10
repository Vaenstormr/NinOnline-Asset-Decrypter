using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Collections.Generic;

namespace NinOnlineAssetDecrypter
{
    static class Program
    {
        // Encrypt values from .nin files to decrypt into PNG files
        private static readonly string Salt = "yC9*I^~0%d J4k0k4JhfDkwzpi^|of0~*W5I-r0u7T1IY4S^C6O3^RmV-H-B";
        private static readonly byte[] FixedBytes = { 66, 135, 99, 114 };

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select .nin Files to Decrypt";
                openFileDialog.Filter = "Nin Files (*.nin)|*.nin";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // New: Ask the user where to save the files
                    using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.Description = "Select the destination folder for the decrypted PNGs";
                        
                        if (folderDialog.ShowDialog() == DialogResult.OK)
                        {
                            string targetFolder = folderDialog.SelectedPath;
                            int success = 0;

                            foreach (string file in openFileDialog.FileNames)
                            {
                                if (ProcessFile(file, targetFolder)) success++;
                            }

                            MessageBox.Show($"Process completed.\nFiles processed successfully: {success}\nLocation: {targetFolder}", 
                                            "Nin Extractor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        static bool ProcessFile(string inputPath, string outputFolder)
        {
            try
            {
                byte[] input = File.ReadAllBytes(inputPath);
                
                // Construct the new path in the selected folder
                string fileName = Path.GetFileNameWithoutExtension(inputPath) + ".png";
                string outputPath = Path.Combine(outputFolder, fileName);

                using (PasswordDeriveBytes pdb = new PasswordDeriveBytes(Salt, FixedBytes))
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.Key = pdb.GetBytes(32);
                        aes.IV = pdb.GetBytes(16);

                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(input, 0, input.Length);
                            }
                            File.WriteAllBytes(outputPath, ms.ToArray());
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Note: In a GUI app, console output might not be visible unless debugging
                return false;
            }
        }
    }
}