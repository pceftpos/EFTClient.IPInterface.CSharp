using System;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>A PC-EFTPOS cloud pairing request object.</summary>
    public class EFTCloudPairRequest : EFTRequest
    {
        /// <summary>Constructs a default cloud pairing request object.</summary>
        public EFTCloudPairRequest() : base(true, typeof(EFTCloudLogonResponse))
        {
        }

        /// <summary>The cloud logon username/ClientID</summary>
        /// <value>Type: <see cref="System.String" /><para>The default is ""</para></value>
        public string ClientID { get; set; } = "";

        /// <summary>The cloud logon password</summary>
        /// <value>Type: <see cref="System.String" /><para>The default is ""</para></value>
        public string Password { get; set; } = "";

        /// <summary>The cloud logon pairing code created by the pinpad</summary>
        /// <value>Type: <see cref="System.String" /><para>The default is ""</para></value>
        public string PairingCode { get; set; } = "";
    }

    /// <summary>A PC-EFTPOS cloud logon response object.</summary>
    public class EFTCloudPairResponse : EFTResponse
    {
        /// <summary>Constructs a default terminal pairing response object.</summary>
        public EFTCloudPairResponse() : base(typeof(EFTCloudPairRequest))
        {
        }

        /// <summary>Success flag for the call. TRUE for successful</summary>
        /// <value>Type: <see cref="System.Boolean" /></value>
        public bool Success { get; set; } = false;

        /// <summary>Response code for the call</summary>
        /// <value>Type: <see cref="System.String" /></value>
        public string ResponseCode { get; set; } = "";

        /// <summary>Response text for the call</summary>
        /// <value>Type: <see cref="System.String" /></value>
        public string ResponseText { get; set; } = "";

        /// <summary>Redirect Port that the POS should connect to after making the pairing request</summary>
        /// <value>Type: <see cref="System.String" /></value>
        public int RedirectPort { get; set; } = 443;

        /// <summary>Redirect Address that the POS should connect to after making the pairing request</summary>
        /// <value>Type: <see cref="System.String" /></value>
        public string RedirectAddress { get; set; } = "";

        /// <summary>Response Token used by the POS for subsequent Cloud Calls</summary>
        /// <value>Type: <see cref="System.String" /></value>
        public string Token { get; set; } = "";
    }
}
