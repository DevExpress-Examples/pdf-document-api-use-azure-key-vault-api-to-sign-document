#region #using
using DevExpress.Office.DigitalSignatures;
using DevExpress.Office.Tsp;
using DevExpress.Pdf;
using System;
using System.Diagnostics;
#endregion #using

namespace PdfAPIAzureKeyVaultSample
{
    class Program
    {
        static void Main(string[] args)
        {
            #region #main
            using (var signer = new PdfDocumentSigner(@"Document.pdf"))
            {
                // Create a timestamp:
                ITsaClient tsaClient = new TsaClient(new Uri(@"https://freetsa.org/tsr"), HashAlgorithmType.SHA256);

                // Specify the signature's field name and location:
                int pageNumber = 1;
                var description = new PdfSignatureFieldInfo(pageNumber);
                description.Name = "SignatureField";
                description.SignatureBounds = new PdfRectangle(10, 10, 50, 150);

                // Please specify your Azure Key Vault URL - (vaultUri)
                const string keyVaultUrl = "";

                // Please specify the Azure Key Vault Certificate ID (certId)
                string certificateId = "";

                // Please specify the Azure Key Vault Key ID for signing certificate
                string keyId = "";

                // Create a custom signer object:
                var client = AzureKeyVaultClient.CreateClient(keyVaultUrl);
                AzureKeyVaultSigner azureSigner = new AzureKeyVaultSigner(client, certificateId, keyId, keyVaultUrl, tsaClient);

                // Apply a signature to a new form field:
                var signatureBuilder = new PdfSignatureBuilder(azureSigner, description);

                // Specify the image and signer information:
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
