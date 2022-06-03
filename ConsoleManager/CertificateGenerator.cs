using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// See https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html
partial class ConsoleManager
{
    private void GenerateCertificates()
    {
        Console.Clear();
        Console.WriteLine("\t Certificate Generation \n");
        Console.WriteLine("\t Generate Encryption Certificate ... ");
        GenerateEncryptionCertificate();
        Console.WriteLine("\t Generate Signing Certificate ... ");
        GenerateSigningCertificate();
        Console.Write("\n\t Press any key to go back to Main Menu");
        Console.ReadKey(true);
    }

    private void GenerateEncryptionCertificate()
    {
        using var algorithm = RSA.Create(keySizeInBits: 2048);

        var subject = new X500DistinguishedName("CN=AIS Encryption Certificate");
        var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, critical: true));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(10));

        var file = Path.Combine(configuration["Application:CertificateFolder"], "encryption-certificate.pfx");
        if (File.Exists(file))
        {
            Console.Write("\t\t Certificate file already exists. Do you want to overwrite it? [y|n] ");
            if (Console.ReadLine().ToLower() != "y") return;
        }
        File.WriteAllBytes(file, certificate.Export(X509ContentType.Pfx, string.Empty));
        Console.WriteLine("\t Done!");
    }

    private void GenerateSigningCertificate()
    {
        using var algorithm = RSA.Create(keySizeInBits: 2048);

        var subject = new X500DistinguishedName("CN=AIS Signing Certificate");
        var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(10));

        var file = Path.Combine(configuration["Application:CertificateFolder"], "signing-certificate.pfx");
        if (File.Exists(file))
        {
            Console.Write("\t\t Certificate file already exists. Do you want to overwrite it? [y|n] ");
            if (Console.ReadLine().ToLower() != "y") return;
        }
        File.WriteAllBytes(file, certificate.Export(X509ContentType.Pfx, string.Empty));
        Console.WriteLine("\t Done!");
    }
}
