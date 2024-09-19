Option Infer On

#Region "#using"
Imports Azure.Core
Imports Azure.Identity
Imports Azure.Security.KeyVault.Certificates
Imports Azure.Security.KeyVault.Keys
Imports Azure.Security.KeyVault.Keys.Cryptography
Imports DevExpress.Office.DigitalSignatures
Imports DevExpress.Office.Tsp
Imports DevExpress.Pdf
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Security.Cryptography.X509Certificates
#End Region



Namespace PdfAPIAzureKeyVaultSample
	#Region "client"
	Public Class AzureKeyVaultClient
		Public Shared Function CreateClient(ByVal keyVaultUri As Uri) As AzureKeyVaultClient
			Dim tokenCredential = New DefaultAzureCredential(New DefaultAzureCredentialOptions With {
				.ExcludeInteractiveBrowserCredential = False,
				.ExcludeVisualStudioCodeCredential = True
			})

			Return New AzureKeyVaultClient(New KeyClient(keyVaultUri,tokenCredential),tokenCredential)
		End Function

		Private ReadOnly client As KeyClient

		Private clientCredentials As TokenCredential
		Private Sub New(ByVal client As KeyClient, ByVal cryptoClientCredential As TokenCredential)
			Me.client = client
			Me.clientCredentials = cryptoClientCredential
		End Sub
		Public Function Sign(ByVal certificateName As String, ByVal algorithm As SignatureAlgorithm, ByVal digest() As Byte, Optional ByVal version As String = Nothing) As Byte()
			Dim cloudRsaKey As KeyVaultKey = client.GetKey(certificateName,version:= version)
			Dim rsaCryptoClient = New CryptographyClient(cloudRsaKey.Id,clientCredentials)

			Dim rsaSignResult As SignResult = rsaCryptoClient.Sign(algorithm,digest)
			Debug.WriteLine($"Signed digest using the algorithm {rsaSignResult.Algorithm}, " & $"with key {1}. The resulting signature is {2}")
			Return rsaSignResult.Signature
		End Function

		Public Function GetCertificateData(ByVal keyId As String) As KeyVaultCertificateWithPolicy
			Dim certificateClient = New CertificateClient(client.VaultUri,clientCredentials)
			Dim cert As KeyVaultCertificateWithPolicy = certificateClient.GetCertificate(keyId)
			Return cert
		End Function
	End Class
	#End Region

	#Region "signer"
	Public Class AzureKeyVaultSigner
		Inherits Pkcs7SignerBase

		'OID for RSA signing algorithm:
		Private Const PKCS1RsaEncryption As String = "1.2.840.113549.1.1.1"

		Private ReadOnly keyVaultClient As AzureKeyVaultClient
		Private ReadOnly certificate As KeyVaultCertificateWithPolicy

		'Must match with key algorithm (RSA or ECDSA)
		'For RSA PKCS1RsaEncryption(1.2.840.113549.1.1.1) OID can be used with any digest algorithm
		'For ECDSA use OIDs from this family http://oid-info.com/get/1.2.840.10045.4.3 
		'Specified digest algorithm must be same with DigestCalculator algorithm.
		Protected Overrides ReadOnly Property DigestCalculator() As IDigestCalculator 'Digest algorithm
			Get
				Return New DigestCalculator(HashAlgorithmType.SHA256)
			End Get
		End Property
		Protected Overrides ReadOnly Property SigningAlgorithmOID() As String
			Get
				Return PKCS1RsaEncryption
			End Get
		End Property

		''' <summary>
		''' Construct an instance of AzureKeyVaultSigner
		''' </summary>
		''' <param name="keyVaultClient">API client used to communicate with </param>
		''' <param name="certificateName">The name of the Azure Certificate, will not contain any slashes</param>
		''' <param name="certificateVersion">The version of the Azure Certificate, looks like a UUID. Leave empty/null to use the version labelled in Azure Portal as the "Current Version"</param>
		''' <param name="tsaClient"></param>
		''' <param name="ocspClient"></param>
		''' <param name="crlClient"></param>
		''' <param name="profile"></param>
		''' <exception cref="System.ArgumentException"></exception>
		Public Sub New(ByVal keyVaultClient As AzureKeyVaultClient, ByVal certificateName As String, Optional ByVal certificateVersion As String = Nothing, Optional ByVal tsaClient As ITsaClient = Nothing, Optional ByVal ocspClient As IOcspClient = Nothing, Optional ByVal crlClient As ICrlClient = Nothing, Optional ByVal profile As PdfSignatureProfile = PdfSignatureProfile.PAdES_BES)
			MyBase.New(tsaClient,ocspClient,crlClient,profile)
			If String.IsNullOrEmpty(certificateName) Then
				Throw New System.ArgumentException("Certificate name must not be null or empty.")
			End If

			If certificateName.Contains("/"c) Then
				Throw New System.ArgumentException("Invalid certificate name. Certificate name must not contain '/' character.")
			End If

			If certificateVersion IsNot Nothing AndAlso certificateVersion.Contains("/"c) Then
				Throw New System.ArgumentException("Invalid certificate version. Certificate version must not contain '/' character.")
			End If

			Me.keyVaultClient = keyVaultClient
			'Get certificate (without public key) via GetCertificateAsync API
			'You can get the whole certificate chain here
			Me.certificate = keyVaultClient.GetCertificateData($"{certificateName}/{certificateVersion}")
		End Sub

		Protected Overrides Function GetCertificates() As IEnumerable(Of Byte())
			Dim certificateChain As New List(Of Byte())()
			Dim x509 = New X509Certificate2(certificate.Cer)
			Dim chain = New X509Chain()
			If True Then
				chain.Build(x509)

				For Each item In chain.ChainElements
					certificateChain.Add(item.Certificate.GetRawCertData())
				Next item
			End If
			Return certificateChain
		End Function

		Protected Overrides Function SignDigest(ByVal digest() As Byte) As Byte()
			Dim signature = keyVaultClient.Sign(certificate.Name,SignatureAlgorithm.RS256,digest)
			Return signature
		End Function
	End Class
	#End Region
End Namespace
