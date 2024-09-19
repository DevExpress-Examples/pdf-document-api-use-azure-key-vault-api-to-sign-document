Option Infer On

#Region "using"
Imports DevExpress.Office.DigitalSignatures
Imports DevExpress.Office.Tsp
Imports DevExpress.Pdf
Imports System
Imports System.Diagnostics
#End Region

Namespace PdfAPIAzureKeyVaultSample
	Friend Class Program
		Shared Sub Main(ByVal args() As String)
'			#Region "main"
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

				' Specify the Name of and Azure Key Vault Certificate (certId).
				' This is listed in the "Name" column of your Azure Portal, Certificates page
				Dim certificateName As String = ""

				' Specify the Version for the Azure Key Vault certificate.
				' This is listed in the "Version" column of your Azure Portal, Certificates/[Certificate Name] page.
				' Leave this empty, or null, to auto-select the "Current" version of the given certificate.
				' Warning: Auto-selection will not "fall back" to a non-current certificate should the current certificate
				'          be disabled, meaning that if the chosen certificate is disabled, signing will generate an error.  
				Dim certificateVersion As String = ""

				' Create a custom signer object:
				Dim client = AzureKeyVaultClient.CreateClient(New Uri(keyVaultUrl))
				Dim azureSigner As New AzureKeyVaultSigner(client, certificateName:= certificateName, certificateVersion:= certificateVersion, tsaClient)

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
'			#End Region
		End Sub
	End Class
End Namespace
