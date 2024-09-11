#Region "using"
Imports DevExpress.Office.DigitalSignatures
Imports DevExpress.Office.Tsp
Imports DevExpress.Pdf
Imports System.Diagnostics

#End Region
Namespace PdfAPIAzureKeyVaultSample

    Friend Class Program

        Shared Sub Main(ByVal args As String())
#Region "main"
            Using signer = New PdfDocumentSigner("Document.pdf")
                ' Create a timestamp:
                Dim tsaClient As ITsaClient = New TsaClient(New Uri("https://freetsa.org/tsr"), HashAlgorithmType.SHA256)
                ' Specify the signature's field name and location:
                Dim pageNumber As Integer = 1
                Dim description = New PdfSignatureFieldInfo(pageNumber)
                description.Name = "SignatureField"
                description.SignatureBounds = New PdfRectangle(10, 10, 50, 150)
                ' Specify your Azure Key Vault URL - (vaultUri)
                Const keyVaultUrl As String = ""
                ' Specify the Azure Key Vault Certificate ID (certId)
                Dim certificateId As String = ""
                ' Specify the Azure Key Vault Key ID for a certificate
                Dim keyId As String = ""
                ' Create a custom signer object:
                Dim client = AzureKeyVaultClient.CreateClient(keyVaultUrl)
                Dim azureSigner As AzureKeyVaultSigner = New AzureKeyVaultSigner(client, certificateId, keyId, keyVaultUrl, tsaClient)
                ' Apply a signature to a new form field:
                Dim signatureBuilder = New PdfSignatureBuilder(azureSigner, description)
                ' Specify an image and signer information:
                signatureBuilder.SetImageData(System.IO.File.ReadAllBytes("signature.jpg"))
                signatureBuilder.Location = "LOCATION"
                ' Sign and save the document:
                Dim output As String = "SignedDocument.pdf"
                signer.SaveDocument(output, signatureBuilder)
                Process.Start(New ProcessStartInfo(output) With {.UseShellExecute = True})
            End Using
#End Region
        End Sub
    End Class
End Namespace
