using System;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>A PC-EFTPOS cloud logon request object.</summary>
    public class EFTCloudTokenLogonRequest : EFTRequest
    {
        /// <summary>Constructs a default cloud logon request object.</summary>
        public EFTCloudTokenLogonRequest() : base(true, typeof(EFTCloudTokenLogonResponse))
        {
        }

        /// <summary>The cloud logon token provided by a Cloud Pairing Request</summary>
        /// <value>Type: <see cref="System.String" /><para>The default is ""</para></value>
        public string Token { get; set; } = "";
    }

    public class EFTCloudTokenLogonResponse : EFTResponse
    {
        /// <summary>Constructs a default terminal logon response object.</summary>
        public EFTCloudTokenLogonResponse() : base(typeof(EFTCloudTokenLogonRequest))
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
    }

}