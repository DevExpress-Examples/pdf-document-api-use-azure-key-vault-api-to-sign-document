The DevExpress PDF Document API library allows you to retrieve a certificate and sign the document hash using any external client. This example demonstrates how to use Azure Key Vault API to sign a PDF document. 

### Prerequisites

-   An [Azure subscription](https://azure.microsoft.com/free/).
-   An existing Azure Key Vault. If you need to create an Azure Key Vault, you can use the Azure Portal or [Azure CLI](https://docs.microsoft.com/cli/azure).

### Description
First, you need to obtain a certificate or the whole certificate chain using the **Azure Key Vault API**. Create the [Pkcs7SignerBase](https://docs.devexpress.com/OfficeFileAPI/DevExpress.Pdf.Pkcs7SignerBase) descendant and retrieve a certificate/ certificate chain in the constructor. Calculate the document hash using the [DevExpress.Office.DigitalSignatures.DigestCalculator](https://docs.devexpress.com/OfficeFileAPI/DevExpress.Office.DigitalSignatures.DigestCalculator) class with one of the following hashing algorithms: **SHA1**, **SHA256**, **SHA384**, and **SHA512**.
Then, sign the calculated document hash with a private key using the Azure Key Vault API in the **SignDigest** method of the **Pkcs7SignerBase** class.

  ### Additional information
Note that to run this example, you will need to specify your Azure Key Vault URL, certificate Id and RSA key. If you don't have a configured Key Vault, we recommend you review these articles to learn how to obtain this information:
[Azure Key Vault key client library for .NET](https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/keyvault/Azure.Security.KeyVault.Keys)
[Azure Key Vault Certificate client library for .NET](https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/keyvault/Azure.Security.KeyVault.Certificates)
