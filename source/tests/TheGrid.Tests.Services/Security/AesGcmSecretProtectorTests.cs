// <copyright file="AesGcmSecretProtectorTests.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using TheGrid.Models.Configuration;
using TheGrid.Services.Security;

namespace TheGrid.Tests.Services.Security
{
    /// <summary>
    /// Tests for the <see cref="AesGcmSecretProtector"/> class.
    /// </summary>
    public class AesGcmSecretProtectorTests
    {
        private readonly AesGcmSecretProtector _protector = new(Options.Create(new SecretProtectionOptions
        {
            EncryptionKey = Convert.ToBase64String(new byte[32]),
            KeyId = "1",
        }));

        /// <summary>
        /// Tests that a value can be round-tripped through <see cref="AesGcmSecretProtector.Protect"/> and <see cref="AesGcmSecretProtector.Unprotect"/>.
        /// </summary>
        [Fact]
        public void RoundTrip_Test()
        {
            // Arrange
            const string plaintext = "super secret password";

            // Act
            var protectedValue = _protector.Protect(plaintext);
            var result = _protector.Unprotect(protectedValue);

            // Assert
            Assert.Equal(plaintext, result);
        }

        /// <summary>
        /// Tests that encrypting the same value twice produces different ciphertext due to distinct nonces.
        /// </summary>
        [Fact]
        public void Protect_UsesDistinctNonces_Test()
        {
            // Arrange
            const string plaintext = "super secret password";

            // Act
            var first = _protector.Protect(plaintext);
            var second = _protector.Protect(plaintext);

            // Assert
            Assert.NotEqual(first, second);
        }

        /// <summary>
        /// Tests that tampering with the ciphertext/tag portion of a protected value causes decryption to fail.
        /// </summary>
        [Fact]
        public void Unprotect_TamperedValue_Throws_Test()
        {
            // Arrange
            var protectedValue = _protector.Protect("super secret password");
            var parts = protectedValue.Split(':', 3);
            var payload = Convert.FromBase64String(parts[2]);
            payload[^1] ^= 0xFF;
            var tampered = $"{parts[0]}:{parts[1]}:{Convert.ToBase64String(payload)}";

            // Act & Assert
            Assert.Throws<AuthenticationTagMismatchException>(() => _protector.Unprotect(tampered));
        }
    }
}
