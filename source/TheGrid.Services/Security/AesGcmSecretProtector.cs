// <copyright file="AesGcmSecretProtector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using TheGrid.Models.Configuration;

namespace TheGrid.Services.Security
{
    /// <summary>
    /// Encrypts and decrypts secret values using AES-GCM.
    /// </summary>
    /// <param name="options">Encryption key configuration.</param>
    public class AesGcmSecretProtector(IOptions<SecretProtectionOptions> options) : ISecretProtector
    {
        private const string FormatVersion = "v1";
        private const int NonceSizeInBytes = 12;
        private const int TagSizeInBytes = 16;

        private readonly byte[] _key = DecodeKey(options.Value);
        private readonly string _keyId = options.Value.KeyId;

        /// <inheritdoc/>
        public string Protect(string plaintext)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var nonce = RandomNumberGenerator.GetBytes(NonceSizeInBytes);
            var ciphertext = new byte[plaintextBytes.Length];
            var tag = new byte[TagSizeInBytes];

            using (var aesGcm = new AesGcm(_key, TagSizeInBytes))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            var payload = new byte[NonceSizeInBytes + ciphertext.Length + TagSizeInBytes];
            Buffer.BlockCopy(nonce, 0, payload, 0, NonceSizeInBytes);
            Buffer.BlockCopy(ciphertext, 0, payload, NonceSizeInBytes, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, payload, NonceSizeInBytes + ciphertext.Length, TagSizeInBytes);

            return $"{FormatVersion}:{_keyId}:{Convert.ToBase64String(payload)}";
        }

        /// <inheritdoc/>
        public string Unprotect(string protectedValue)
        {
            var parts = protectedValue.Split(':', 3);

            if (parts.Length != 3 || parts[0] != FormatVersion)
            {
                throw new FormatException("The protected value is not in a recognized format.");
            }

            var payload = Convert.FromBase64String(parts[2]);

            if (payload.Length < NonceSizeInBytes + TagSizeInBytes)
            {
                throw new FormatException("The protected value is not in a recognized format.");
            }

            var nonce = payload.AsSpan(0, NonceSizeInBytes);
            var ciphertextLength = payload.Length - NonceSizeInBytes - TagSizeInBytes;
            var ciphertext = payload.AsSpan(NonceSizeInBytes, ciphertextLength);
            var tag = payload.AsSpan(NonceSizeInBytes + ciphertextLength, TagSizeInBytes);

            var plaintextBytes = new byte[ciphertextLength];

            using (var aesGcm = new AesGcm(_key, TagSizeInBytes))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        private static byte[] DecodeKey(SecretProtectionOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.EncryptionKey))
            {
                throw new InvalidOperationException($"{nameof(SecretProtectionOptions.EncryptionKey)} must be configured to encrypt or decrypt secret connection properties.");
            }

            return Convert.FromBase64String(options.EncryptionKey);
        }
    }
}
