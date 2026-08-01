// <copyright file="ISecretProtector.cs" company="BiglerNet">
// Copyright (c) BiglerNet. All rights reserved.
// </copyright>

namespace TheGrid.Services.Security
{
    /// <summary>
    /// Encrypts and decrypts secret values for storage at rest.
    /// </summary>
    public interface ISecretProtector
    {
        /// <summary>
        /// Encrypts a plaintext value for storage.
        /// </summary>
        /// <param name="plaintext">Value to encrypt.</param>
        /// <returns>The encrypted representation of <paramref name="plaintext"/>.</returns>
        string Protect(string plaintext);

        /// <summary>
        /// Decrypts a value previously produced by <see cref="Protect(string)"/>.
        /// </summary>
        /// <param name="protectedValue">Encrypted value to decrypt.</param>
        /// <returns>The original plaintext value.</returns>
        string Unprotect(string protectedValue);
    }
}
