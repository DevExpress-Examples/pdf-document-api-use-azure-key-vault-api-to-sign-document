Imports DevExpress.Office.DigitalSignatures
Imports DevExpress.Office.Tsp
Imports DevExpress.Pdf
Imports System
Imports System.Diagnostics

Namespace PdfAPIAzureKeyVaultSample
	Friend Class Program
		Shared Sub Main(ByVal args() As String)
			Using signer = New PdfDocumentSigner("Document.pdf")
				'Create a timestamp:
				Dim tsaClient As ITsaClient = New TsaClient(New Uri("https://freetsa.org/tsr"), HashAlgorithmType.SHA256)

				'Specify the signature's field name and location:
				Dim pageNumber As Integer = 1
				Dim description = New PdfSignatureFieldInfo(pageNumber)
				description.Name = "SignatureField"
				description.SignatureBounds = New PdfRectangle(10, 10, 50, 150)

				Const keyVaultUrl As String = "" 'Please specify your Azure Key Vault URL - (vaultUri)
				Dim certificateId As String = "" 'Please specify the Azure Key Vault Certificate ID (certId)
				'Create a custom signer object:
				Dim client = AzureKeyVaultClient.CreateClient(keyVaultUrl)
				Dim azureSigner As New AzureKeyVaultSigner(client, certificateId, keyVaultUrl, tsaClient)

				'Apply a signature to a new form field:
				Dim signatureBuilder = New PdfSignatureBuilder(azureSigner, description)

				'Specify the image and signer information:
				signatureBuilder.SetImageData(System.IO.File.ReadAllBytes("signature.jpg"))
				signatureBuilder.Location = "LOCATION"

				'Sign and save the document:
				Dim output As String = "SignedDocument.pdf"
				signer.SaveDocument(output, signatureBuilder)
				Process.Start(New ProcessStartInfo(output) With {.UseShellExecute = True})
			End Using
		End Sub
	End Class
End Namespace
