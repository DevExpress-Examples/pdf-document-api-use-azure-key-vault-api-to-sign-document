#region using
using DevExpress.Office.DigitalSignatures;
using DevExpress.Office.Tsp;
using DevExpress.Pdf;
using System;
using System.Diagnostics;
#endregion

namespace PdfAPIAzureKeyVaultSample
{
    class Program
    {
        static void Main(string[] args)
        {
            #region main
            using (var signer = new PdfDocumentSigner(@"Document.pdf"))
            {
                // Create a timestamp:
                ITsaClient tsaClient = new TsaClient(new Uri(@"https://freetsa.org/tsr"), HashAlgorithmType.SHA256);

                // Specify the signature's field name and location:
                int pageNumber = 1;
                var description = new PdfSignatureFieldInfo(pageNumber);
                description.Name = "SignatureField";
                description.SignatureBounds = new PdfRectangle(10, 10, 50, 150);

                // Specify your Azure Key Vault URL - (vaultUri)
                const string keyVaultUrl = "";

                // Specify the Name of and Azure Key Vault Certificate (certId).
                // This is listed in the "Name" column of your Azure Portal, Certificates page
                string certificateName = "";

                // Specify the Version for the Azure Key Vault certificate.
                // This is listed in the "Version" column of your Azure Portal, Certificates/[Certificate Name] page.
                // Leave this empty, or null, to auto-select the "Current" version of the given certificate.
                // Warning: Auto-selection will not "fall back" to a non-current certificate should the current certificate
                //          be disabled, meaning that if the chosen certificate is disabled, signing will generate an error.  
                string certificateVersion = "";

                // Create a custom signer object:
                var client = AzureKeyVaultClient.CreateClient(new Uri(keyVaultUrl));
                AzureKeyVaultSigner azureSigner = new AzureKeyVaultSigner(client, 
                    certificateName: certificateName, 
                    certificateVersion: certificateVersion, 
                    tsaClient);

                // Apply a signature to a new form field:
                var signatureBuilder = new PdfSignatureBuilder(azureSigner, description);

                // Specify an image and signer information:
                signatureBuilder.SetImageData(System.IO.File.ReadAllBytes("signature.jpg"));
                signatureBuilder.Location = "LOCATION";

                // Sign and save the document:
                string output = "SignedDocument.pdf";
                signer.SaveDocument(output, signatureBuilder);
                Process.Start(new ProcessStartInfo(output) { UseShellExecute = true });
            }
            #endregion
        }
    }
}
