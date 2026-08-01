// <copyright file="SecretProtectionOptions.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Models.Configuration
{
    /// <summary>
    /// Configuration for encrypting secret connection parameter values at rest.
    /// </summary>
    public class SecretProtectionOptions
    {
        /// <summary>
        /// Base64-encoded 256-bit key used to encrypt and decrypt secret values.
        /// </summary>
        /// <remarks>
        /// Recommended to be sourced from an environment variable. Losing this key makes all
        /// stored secret values permanently unrecoverable.
        /// </remarks>
        public string EncryptionKey { get; set; } = string.Empty;

        /// <summary>
        /// Identifier for <see cref="EncryptionKey"/>, stored alongside each encrypted value so a
        /// future key rotation can identify which key a value was encrypted under.
        /// </summary>
        public string KeyId { get; set; } = "1";
    }
}
