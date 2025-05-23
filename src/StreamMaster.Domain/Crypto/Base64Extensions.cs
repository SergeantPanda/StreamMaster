﻿using System.Text;
namespace StreamMaster.Domain.Crypto;
public static class Base64Extensions
{
    /// <summary>
    /// Converts the specified byte array to a URL-safe Base64 string and appends an HMAC for integrity.
    /// </summary>
    /// <param name="data">The data to convert.</param>
    /// <param name="key">The key to use for HMAC calculation.</param>
    /// <returns>A URL-safe Base64 string representing the data with appended HMAC.</returns>
    public static string ConvertToUrlSafeBase64String(this byte[] data, byte[] key)
    {
        byte[] hmacBytes = CryptoHelpers.CalculateHMAC(data, key);
        byte[] combinedData = new byte[data.Length + AesEncryption.HMACSize];
        Buffer.BlockCopy(hmacBytes, 0, combinedData, 0, AesEncryption.HMACSize);
        Buffer.BlockCopy(data, 0, combinedData, AesEncryption.HMACSize, data.Length);

        string base64String = Convert.ToBase64String(combinedData);
        return base64String.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    /// <summary>
    /// Converts the specified URL-safe Base64 string back to the original byte array.
    /// </summary>
    /// <param name="urlSafeBase64String">The URL-safe Base64 string to convert.</param>
    /// <returns>A byte array representing the original data.</returns>
    public static byte[] ConvertFromUrlSafeBase64String(this string urlSafeBase64String)
    {
        string base64String = urlSafeBase64String.Replace('-', '+').Replace('_', '/');
        switch (urlSafeBase64String.Length % 4)
        {
            case 2: base64String += "=="; break;
            case 3: base64String += "="; break;
        }
        return Convert.FromBase64String(base64String);
    }

    /// <summary>
    /// Converts the specified string to a URL-safe Base64 string.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A URL-safe Base64 string representing the input string.</returns>
    public static string ToUrlSafeBase64String(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));
        }

        byte[] bytes = Encoding.UTF8.GetBytes(input);
        string base64String = Convert.ToBase64String(bytes);
        return base64String.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    /// <summary>
    /// Converts the specified URL-safe Base64 string back to the original string.
    /// </summary>
    /// <param name="urlSafeBase64String">The URL-safe Base64 string to convert.</param>
    /// <returns>The original string.</returns>
    public static string FromUrlSafeBase64String(this string urlSafeBase64String)
    {
        string base64String = urlSafeBase64String.Replace('-', '+').Replace('_', '/');
        switch (urlSafeBase64String.Length % 4)
        {
            case 2: base64String += "=="; break;
            case 3: base64String += "="; break;
        }
        byte[] bytes = Convert.FromBase64String(base64String);
        return Encoding.UTF8.GetString(bytes);
    }
}
