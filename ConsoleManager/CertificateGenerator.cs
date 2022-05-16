using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// See https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
partial class ConsoleManager
{
    private void GenerateCertificates()
    {
        Console.Clear();
        Console.WriteLine("\t Certificate Generation \n");
        Console.Write("\t Generate Encryption Certificate ... ");
        GenerateEncryptionCertificate();
        Console.WriteLine("Done!");
        Console.Write("\t Generate Signing Certificate ... ");
        GenerateSigningCertificate();
        Console.WriteLine("Done!");
        Console.Write("\n\t Press any key to go back to Main Menu");
        _ = Console.ReadKey(true);
    }

    private void GenerateEncryptionCertificate()
    {
        using var algorithm = RSA.Create(keySizeInBits: 2048);

        var subject = new X500DistinguishedName("CN=AIS Encryption Certificate");
        var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, critical: true));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(10));

        File.WriteAllBytes("encryption-certificate.pfx", certificate.Export(X509ContentType.Pfx, string.Empty));
    }

    private void GenerateSigningCertificate()
    {
        using var algorithm = RSA.Create(keySizeInBits: 2048);

        var subject = new X500DistinguishedName("CN=AIS Signing Certificate");
        var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(10));

        File.WriteAllBytes("signing-certificate.pfx", certificate.Export(X509ContentType.Pfx, string.Empty));
    }
}
