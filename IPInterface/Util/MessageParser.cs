using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCEFTPOS.Util;
using PCEFTPOS.Messaging;

namespace PCEFTPOS.EFTClient.IPInterface
{
    interface IMessageParser
    {
        EFTResponse StringToEFTResponse(string msg);
        string EFTRequestToString(EFTRequest eftRequest);        
    }

    public class MessageParser: IMessageParser
    {
        ReceiptType lastReceiptType;

        enum IPClientResponseType
        {
            Logon = 'G', Transaction = 'M', QueryCard = 'J', Configure = '1', ControlPanel = '5', SetDialog = '2', Settlement = 'P',
            DuplicateReceipt = 'C', GetLastTransaction = 'N', Status = 'K', Receipt = '3', Display = 'S', GenericPOSCommand = 'X', PINRequest = 'W',
            ChequeAuth = 'H', SendKey = 'Y', ClientList = 'Q', CloudLogon = 'A'
        }

        #region StringToEFTResponse
        /// <summary> Parses a string to an EFTResponse message </summary>
        /// <param name="msg">string to parse</param>
        /// <returns>An EFTResponse message</returns>
        /// <exception cref="ArgumentException">An ArgumentException is thrown if the contents of msg is invalid</exception>
        public EFTResponse StringToEFTResponse(string msg)
        {
            if (msg?.Length < 1)
            {
                throw new ArgumentException("msg is null or zero length", nameof(msg));
            }

            EFTResponse eftResponse = null;
            switch ((IPClientResponseType)msg[0])
            {
                case IPClientResponseType.Display:
                    eftResponse = ParseDisplayResponse(msg);
                    break;
                case IPClientResponseType.Receipt:
                    eftResponse = ParseReceiptResponse(msg);
                    break;
                case IPClientResponseType.Logon:
                    eftResponse = ParseEFTLogonResponse(msg);
                    break;
                case IPClientResponseType.Transaction:
                    eftResponse = ParseEFTTransactionResponse(msg);
                    break;
                case IPClientResponseType.SetDialog:
                    eftResponse = ParseSetDialogResponse(msg);
                    break;
                case IPClientResponseType.GetLastTransaction:
                    eftResponse = ParseEFTGetLastTransactionResponse(msg);
                    break;
                case IPClientResponseType.DuplicateReceipt:
                    eftResponse = ParseEFTReprintReceiptResponse(msg);
                    break;
                case IPClientResponseType.ControlPanel:
                    eftResponse = ParseControlPanelResponse(msg);
                    break;
                case IPClientResponseType.Settlement:
                    eftResponse = ParseEFTSettlementResponse(msg);
                    break;
                case IPClientResponseType.Status:
                    eftResponse = ParseEFTStatusResponse(msg);
                    break;
                case IPClientResponseType.ChequeAuth:
                    eftResponse = ParseChequeAuthResponse(msg);
                    break;
                case IPClientResponseType.QueryCard:
                    eftResponse = ParseQueryCardResponse(msg);
                    break;
                case IPClientResponseType.GenericPOSCommand:
                    eftResponse = ParseGenericPOSCommandResponse(msg);
                    break;
                case IPClientResponseType.Configure:
                    eftResponse = ParseConfigMerchantResponse(msg);
                    break;
                case IPClientResponseType.CloudLogon:
                    eftResponse = ParseCloudLogonResponse(msg);
                    break;
                default:
                    throw new ArgumentException($"Unknown message type: {msg}", nameof(msg));
            }

            return eftResponse;
        }

        T TryParse<T>(string input, int length, ref int index)
        {
            return TryParse<T>(input, length, ref index, "");
        }
        T TryParse<T>(string input, int length, ref int index, string format)
        {
            T result = default(T);

            if (input.Length - index >= length)
            {
                if (result is bool && length == 1)
                {
                    result = (T)Convert.ChangeType((input[index] == '1' || input[index] == 'Y'), typeof(T));
                    index += length;
                }
                else
                {
                    object data = input.Substring(index, length);
                    try
                    {
                        if (result is Enum && length == 1)
                            result = (T)Enum.ToObject(typeof(T), ((string)data)[0]);
                        else if (result is DateTime && format.Length > 1)
                            result = (T)(object)DateTime.ParseExact((string)data, format, null);
                        else
                            result = (T)Convert.ChangeType(data, typeof(T));
                    }
                    catch
                    {
                        var idx = index;
                        //Log(LogLevel.Error, tr => tr.Set($"Unable to parse field. Input={input}, Index={idx}, Length={length}"));
                    }
                    finally
                    {
                        index += length;
                    }
                }
            }
            else
                index = length;

            return result;
        }

        EFTResponse ParseEFTTransactionResponse(string msg)
        {
            var index = 1;

            var r = new EFTTransactionResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);
            index += 2; // Skip merchant number.
            r.TxnType = TryParse<TransactionType>(msg, 1, ref index);
            r.CardAccountType = r.CardAccountType.FromString(TryParse<string>(msg, 7, ref index));
            r.AmountCash = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AmountPurchase = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AmountTip = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AuthNumber = TryParse<int>(msg, 6, ref index);
            r.ReferenceNumber = TryParse<string>(msg, 16, ref index);
            r.STAN = TryParse<int>(msg, 6, ref index);
            r.MerchantID = TryParse<string>(msg, 15, ref index);
            r.TerminalID = TryParse<string>(msg, 8, ref index);
            r.ExpiryDate = TryParse<string>(msg, 4, ref index);
            r.SettlementDate = TryParse<DateTime>(msg, 4, ref index, "ddMM");
            r.BankDateTime = TryParse<DateTime>(msg, 12, ref index, "ddMMyyHHmmss");
            r.CardType = TryParse<string>(msg, 20, ref index);
            r.CardPAN = TryParse<string>(msg, 20, ref index);
            r.Track2 = TryParse<string>(msg, 40, ref index);
            r.RRN = TryParse<string>(msg, 12, ref index);
            r.CardBIN = TryParse<int>(msg, 2, ref index);
            r.TxnFlags = new TxnFlags(TryParse<string>(msg, 8, ref index).ToCharArray());
            r.BalanceReceived = TryParse<bool>(msg, 1, ref index);
            r.AvailableBalance = TryParse<decimal>(msg, 9, ref index) / 100;
            r.ClearedFundsBalance = TryParse<decimal>(msg, 9, ref index) / 100;
            r.PurchaseAnalysisData = new PadField(TryParse<string>(msg, msg.Length - index, ref index));

            return r;
        }
        EFTResponse ParseEFTGetLastTransactionResponse(string msg)
        {
            var index = 1;

            var r = new EFTGetLastTransactionResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.LastTransactionSuccess = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);
            index += 2; // Skip merchant number.

            if (char.IsLower(msg[index]))
            {
                r.IsTrainingMode = true;
                msg = msg.Substring(0, index) + char.ToUpper(msg[index]) + msg.Substring(index + 1);
            }
            r.TxnType = TryParse<TransactionType>(msg, 1, ref index);
            string accountType = TryParse<string>(msg, 7, ref index);
            if (accountType == "Credit ") r.CardAccountType = AccountType.Credit;
            else if (accountType == "Savings") r.CardAccountType = AccountType.Savings;
            else if (accountType == "Cheque ") r.CardAccountType = AccountType.Cheque;
            else r.CardAccountType = AccountType.Default;
            r.AmountCash = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AmountPurchase = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AmountTip = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AuthNumber = TryParse<int>(msg, 6, ref index);
            r.ReferenceNumber = TryParse<string>(msg, 16, ref index);
            r.STAN = TryParse<int>(msg, 6, ref index);
            r.MerchantID = TryParse<string>(msg, 15, ref index);
            r.TerminalID = TryParse<string>(msg, 8, ref index);
            r.ExpiryDate = TryParse<string>(msg, 4, ref index);
            r.SettlementDate = TryParse<DateTime>(msg, 4, ref index, "ddMM");
            r.BankDateTime = TryParse<DateTime>(msg, 12, ref index, "ddMMyyHHmmss");
            r.CardType = TryParse<string>(msg, 20, ref index);
            r.CardPAN = TryParse<string>(msg, 20, ref index);
            r.Track2 = TryParse<string>(msg, 40, ref index);
            r.RRN = TryParse<string>(msg, 12, ref index);
            r.CardBIN = TryParse<int>(msg, 2, ref index);
            string txnFlags = TryParse<string>(msg, 8, ref index);
            r.TxnFlags = new TxnFlags(txnFlags.ToCharArray());
            r.BalanceReceived = TryParse<bool>(msg, 1, ref index);
            r.AvailableBalance = TryParse<decimal>(msg, 9, ref index) / 100;
            int padLength = TryParse<int>(msg, 3, ref index);
            r.PurchaseAnalysisData = new PadField(TryParse<string>(msg, padLength, ref index));

            return r;
        }
        EFTResponse ParseSetDialogResponse(string msg)
        {
            var index = 1;

            var r = new SetDialogResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);

            return r;
        }
        EFTResponse ParseEFTLogonResponse(string msg)
        {
            var index = 1;

            var r = new EFTLogonResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            if (msg.Length > 25)
            {
                r.TerminalID = TryParse<string>(msg, 8, ref index);
                r.MerchantID = TryParse<string>(msg, 15, ref index);
                r.BankDateTime = TryParse<DateTime>(msg, 12, ref index, "ddMMyyHHmmss");
                r.STAN = TryParse<int>(msg, 6, ref index);
                r.PinpadVersion = TryParse<string>(msg, 16, ref index);
                r.PurchaseAnalysisData = new PadField(TryParse<string>(msg, msg.Length - index, ref index));
            }
            return r;
        }
        EFTResponse ParseDisplayResponse(string response)
        {
            int index = 1;

            EFTDisplayResponse displayResponse = new EFTDisplayResponse();
            index++; // Skip sub code.
            displayResponse.NumberOfLines = TryParse<int>(response, 2, ref index);
            displayResponse.LineLength = TryParse<int>(response, 2, ref index);
            for (int i = 0; i < displayResponse.NumberOfLines; i++)
                displayResponse.DisplayText[i] = TryParse<string>(response, displayResponse.LineLength, ref index);
            displayResponse.CancelKeyFlag = TryParse<bool>(response, 1, ref index);
            displayResponse.AcceptYesKeyFlag = TryParse<bool>(response, 1, ref index);
            displayResponse.DeclineNoKeyFlag = TryParse<bool>(response, 1, ref index);
            displayResponse.AuthoriseKeyFlag = TryParse<bool>(response, 1, ref index);
            displayResponse.InputType = TryParse<InputType>(response, 1, ref index);
            displayResponse.OKKeyFlag = TryParse<bool>(response, 1, ref index);
            index += 2;
            displayResponse.GraphicCode = TryParse<GraphicCode>(response, 1, ref index);
            int padLength = TryParse<int>(response, 3, ref index);
            displayResponse.PurchaseAnalysisData = new PadField(TryParse<string>(response, padLength, ref index));

            return displayResponse;
        }
        EFTResponse ParseReceiptResponse(string response)
        {
            int index = 1;

            EFTReceiptResponse eftResponse = new EFTReceiptResponse
            {
                Type = TryParse<ReceiptType>(response, 1, ref index)
            };

            if (eftResponse.Type != ReceiptType.ReceiptText)
            {
                lastReceiptType = eftResponse.Type;
                eftResponse.IsPrePrint = true;
            }
            else
            {
                List<string> receiptLines = new List<string>();
                bool done = false;
                while (!done)
                {
                    int lineLength = response.Substring(index).IndexOf("\r\n");
                    if (lineLength > 0)
                    {
                        receiptLines.Add(response.Substring(index, lineLength));
                        index += lineLength + 2;
                        if (index >= response.Length)
                            done = true;
                    }
                    else
                        done = true;
                }

                eftResponse.ReceiptText = receiptLines.ToArray();
                eftResponse.Type = lastReceiptType;
            }

            return eftResponse;
        }
        EFTResponse ParseControlPanelResponse(string msg)
        {
            int index = 1;

            var r = new EFTControlPanelResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            return r;
        }
        EFTResponse ParseEFTReprintReceiptResponse(string response)
        {
            int index = 1;

            EFTReprintReceiptResponse eftResponse = new EFTReprintReceiptResponse();
            index++; // Skip sub code.
            eftResponse.Success = TryParse<bool>(response, 1, ref index);
            eftResponse.ResponseCode = TryParse<string>(response, 2, ref index);
            eftResponse.ResponseText = TryParse<string>(response, 20, ref index);

            List<string> receiptLines = new List<string>();
            bool done = false;
            while (!done)
            {
                int lineLength = response.Substring(index).IndexOf("\r\n");
                if (lineLength > 0)
                {
                    receiptLines.Add(response.Substring(index, lineLength));
                    index += lineLength + 2;
                    if (index >= response.Length)
                        done = true;
                }
                else
                    done = true;
            }

            eftResponse.ReceiptText = receiptLines.ToArray();

            return eftResponse;
        }
        EFTResponse ParseEFTSettlementResponse(string msg)
        {
            var index = 1;

            var r = new EFTSettlementResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            if (msg.Length > 25)
            {
                r.SettlementData = msg.Substring(index);

                //int cardCount = int.Parse( response.Substring( index, 9 ) ); index += 9;
                //for( int i = 0; i < cardCount; i++ )
                //{
                //    int cardTotalsDataLength = int.Parse( response.Substring( index, 3 ) ); index += 3;
                //    if( cardTotalsDataLength >= 69 )
                //    {
                //        SettlementCardTotals cardTotals = new SettlementCardTotals();
                //        cardTotals.CardName = response.Substring( index, 20 ); index += 20;
                //        try { cardTotals.PurchaseAmount = decimal.Parse( response.Substring( index, 9 ) ) / 100; }
                //        catch { cardTotals.PurchaseAmount = 0; }
                //        finally { index += 9; }
                //        try { cardTotals.PurchaseCount = int.Parse( response.Substring( index, 3 ) ); }
                //        catch { cardTotals.PurchaseCount = 0; }
                //        finally { index += 3; }
                //        try { cardTotals.CashOutAmount = decimal.Parse( response.Substring( index, 9 ) ) / 100; }
                //        catch { cardTotals.CashOutAmount = 0; }
                //        finally { index += 9; }
                //        try { cardTotals.CashOutCount = int.Parse( response.Substring( index, 3 ) ); }
                //        catch { cardTotals.CashOutCount = 0; }
                //        finally { index += 3; }
                //        try { cardTotals.RefundAmount = decimal.Parse( response.Substring( index, 9 ) ) / 100; }
                //        catch { cardTotals.RefundAmount = 0; }
                //        finally { index += 9; }
                //        try { cardTotals.RefundCount = int.Parse( response.Substring( index, 3 ) ); }
                //        catch { cardTotals.RefundCount = 0; }
                //        finally { index += 3; }
                //        try { cardTotals.TotalAmount = decimal.Parse( response.Substring( index, 10 ), System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowLeadingWhite ) / 100; }
                //        catch { cardTotals.TotalAmount = 0; }
                //        finally { index += 9; }
                //        try { cardTotals.TotalCount = int.Parse( response.Substring( index, 3 ) ); }
                //        catch { cardTotals.TotalCount = 0; }
                //        finally { index += 3; }
                //        displayResponse.SettlementCardData.Add( cardTotals );
                //        index += cardTotalsDataLength - 69;
                //    }
                //}

                //int totalsDataLength = int.Parse( response.Substring( index, 3 ) ); index += 3;
                //if( totalsDataLength >= 69 )
                //{
                //    SettlementTotals settleTotals = new SettlementTotals();
                //    settleTotals.TotalsDescription = response.Substring( index, 20 ); index += 20;
                //    try { settleTotals.PurchaseAmount = decimal.Parse( response.Substring( index, 9 ) ) / 100; }
                //    catch { settleTotals.PurchaseAmount = 0; }
                //    finally { index += 9; }
                //    try { settleTotals.PurchaseCount = int.Parse( response.Substring( index, 3 ) ); }
                //    catch { settleTotals.PurchaseCount = 0; }
                //    finally { index += 3; }
                //    try { settleTotals.CashOutAmount = decimal.Parse( response.Substring( index, 9 ) ) / 100; }
                //    catch { settleTotals.CashOutAmount = 0; }
                //    finally { index += 9; }
                //    try { settleTotals.CashOutCount = int.Parse( response.Substring( index, 3 ) ); }
                //    catch { settleTotals.CashOutCount = 0; }
                //    finally { index += 3; }
                //    try { settleTotals.RefundAmount = decimal.Parse( response.Substring( index, 9 ) ) / 100; }
                //    catch { settleTotals.RefundAmount = 0; }
                //    finally { index += 9; }
                //    try { settleTotals.RefundCount = int.Parse( response.Substring( index, 3 ) ); }
                //    catch { settleTotals.RefundCount = 0; }
                //    finally { index += 3; }
                //    try { settleTotals.TotalAmount = decimal.Parse( response.Substring( index, 10 ), System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowLeadingWhite ) / 100; }
                //    catch { settleTotals.TotalAmount = 0; }
                //    finally { index += 9; }
                //    try { settleTotals.TotalCount = int.Parse( response.Substring( index, 3 ) ); }
                //    catch { settleTotals.TotalCount = 0; }
                //    finally { index += 3; }
                //    displayResponse.TotalsData = settleTotals;
                //    index += totalsDataLength - 69;
                //}

                //int padLength;
                //try
                //{
                //    padLength = int.Parse( response.Substring( index, 3 ) );
                //    displayResponse.PurchaseAnalysisData = response.Substring( index, padLength ); index += padLength;
                //}
                //catch { padLength = 0; }
                //finally { index += 3; }
            }

            return r;
        }
        EFTResponse ParseQueryCardResponse(string response)
        {
            int index = 1;

            EFTQueryCardResponse eftResponse = new EFTQueryCardResponse
            {
                AccountType = (AccountType)response[index++],
                Success = TryParse<bool>(response, 1, ref index),
                ResponseCode = TryParse<string>(response, 2, ref index),
                ResponseText = TryParse<string>(response, 20, ref index)
            };

            if (response.Length > 25)
            {
                eftResponse.Track2 = response.Substring(index, 40); index += 40;
                string track1or3 = response.Substring(index, 80); index += 80;
                char trackFlag = response[index++];
                switch (trackFlag)
                {
                    case '1':
                        eftResponse.TrackFlags = TrackFlags.Track1;
                        eftResponse.Track1 = track1or3;
                        break;
                    case '2':
                        eftResponse.TrackFlags = TrackFlags.Track2;
                        break;
                    case '3':
                        eftResponse.TrackFlags = TrackFlags.Track1 | TrackFlags.Track2;
                        eftResponse.Track1 = track1or3;
                        break;
                    case '4':
                        eftResponse.TrackFlags = TrackFlags.Track3;
                        eftResponse.Track3 = track1or3;
                        break;
                    case '6':
                        eftResponse.TrackFlags = TrackFlags.Track2 | TrackFlags.Track3;
                        eftResponse.Track3 = track1or3;
                        break;
                }

                eftResponse.CardBin = int.Parse(response.Substring(index, 2)); index += 2;

                int padLength;
                try
                {
                    padLength = int.Parse(response.Substring(index, 3));
                    eftResponse.PurchaseAnalysisData = new PadField(response.Substring(index, padLength));
                    index += padLength;
                }
                catch { padLength = 0; }
                finally { index += 3; }
            }

            return eftResponse;
        }
        EFTResponse ParseEFTConfigureMerchantResponse(string response)
        {
            int index = 1;

            EFTConfigureMerchantResponse eftResponse = new EFTConfigureMerchantResponse();
            index++; // Skip sub code.
            eftResponse.Success = TryParse<bool>(response, 1, ref index);
            eftResponse.ResponseCode = TryParse<string>(response, 2, ref index);
            eftResponse.ResponseText = TryParse<string>(response, 20, ref index);

            return eftResponse;
        }
        EFTResponse ParseEFTStatusResponse(string response)
        {
            int index = 1;

            EFTStatusResponse eftResponse = new EFTStatusResponse();
            index++; // Skip sub code.
            eftResponse.Success = TryParse<bool>(response, 1, ref index);
            eftResponse.ResponseCode = TryParse<string>(response, 2, ref index);
            eftResponse.ResponseText = TryParse<string>(response, 20, ref index);
            if (index >= response.Length) return eftResponse;
            index += 2; // Skip merchant number.
            eftResponse.AIIC = TryParse<string>(response, 11, ref index);
            eftResponse.NII = TryParse<int>(response, 3, ref index);
            eftResponse.MerchantID = TryParse<string>(response, 15, ref index);
            eftResponse.TerminalID = TryParse<string>(response, 8, ref index);
            eftResponse.Timeout = TryParse<int>(response, 3, ref index);
            eftResponse.LoggedOn = TryParse<bool>(response, 1, ref index);
            eftResponse.PINPadSerialNumber = TryParse<string>(response, 16, ref index);
            eftResponse.PINPadVersion = TryParse<string>(response, 16, ref index);
            eftResponse.BankDescription = TryParse<string>(response, 32, ref index);
            int padLength = TryParse<int>(response, 3, ref index);
            if (response.Length - index < padLength)
                return eftResponse;
            eftResponse.SAFCount = TryParse<int>(response, 4, ref index);
            eftResponse.NetworkType = TryParse<NetworkType>(response, 1, ref index);
            eftResponse.HardwareSerial = TryParse<string>(response, 16, ref index);
            eftResponse.RetailerName = TryParse<string>(response, 40, ref index);
            eftResponse.OptionsFlags = ParseStatusOptionFlags(response.Substring(index, 32).ToCharArray()); index += 32;
            eftResponse.SAFCreditLimit = TryParse<int>(response, 9, ref index) / 100;
            eftResponse.SAFDebitLimit = TryParse<int>(response, 9, ref index) / 100;
            eftResponse.MaxSAF = TryParse<int>(response, 3, ref index);
            eftResponse.KeyHandlingScheme = ParseKeyHandlingType(response[index++]);
            eftResponse.CashoutLimit = TryParse<decimal>(response, 9, ref index) / 100;
            eftResponse.RefundLimit = TryParse<decimal>(response, 9, ref index) / 100;
            eftResponse.CPATVersion = TryParse<string>(response, 6, ref index);
            eftResponse.NameTableVersion = TryParse<string>(response, 6, ref index);
            eftResponse.TerminalCommsType = ParseTerminalCommsType(response[index++]);
            eftResponse.CardMisreadCount = TryParse<int>(response, 6, ref index);
            eftResponse.TotalMemoryInTerminal = TryParse<int>(response, 4, ref index);
            eftResponse.FreeMemoryInTerminal = TryParse<int>(response, 4, ref index);
            eftResponse.EFTTerminalType = ParseEFTTerminalType(response.Substring(index, 4)); index += 4;
            eftResponse.NumAppsInTerminal = TryParse<int>(response, 2, ref index);
            eftResponse.NumLinesOnDisplay = TryParse<int>(response, 2, ref index);
            eftResponse.HardwareInceptionDate = TryParse<DateTime>(response, 6, ref index, "ddMMyy");

            return eftResponse;
        }

        TerminalCommsType ParseTerminalCommsType(char CommsType)
        {
            TerminalCommsType commsType = TerminalCommsType.Unknown;

            if (CommsType == '0') commsType = TerminalCommsType.Cable;
            else if (CommsType == '1') commsType = TerminalCommsType.Infrared;

            return commsType;
        }
        KeyHandlingType ParseKeyHandlingType(char KeyHandlingScheme)
        {
            KeyHandlingType keyHandlingType = KeyHandlingType.Unknown;

            if (KeyHandlingScheme == '0') keyHandlingType = KeyHandlingType.SingleDES;
            else if (KeyHandlingScheme == '1') keyHandlingType = KeyHandlingType.TripleDES;

            return keyHandlingType;
        }
        EFTTerminalType ParseEFTTerminalType(string TerminalType)
        {
            EFTTerminalType terminalType = EFTTerminalType.Unknown;

            if (TerminalType == "0062") terminalType = EFTTerminalType.IngenicoNPT710;
            else if (TerminalType == "0069") terminalType = EFTTerminalType.IngenicoPX328;
            else if (TerminalType == "7010") terminalType = EFTTerminalType.Ingenicoi3070;
            else if (TerminalType == "5110") terminalType = EFTTerminalType.Ingenicoi5110;

            return terminalType;
        }
        PINPadOptionFlags ParseStatusOptionFlags(char[] Flags)
        {
            PINPadOptionFlags flags = 0;
            int index = 0;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Tipping;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.PreAuth;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Completions;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.CashOut;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Refund;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Balance;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Deposit;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Voucher;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.MOTO;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.AutoCompletion;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.EFB;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.EMV;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Training;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Withdrawal;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Transfer;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.StartCash;
            return flags;
        }
        EFTResponse ParseChequeAuthResponse(string response)
        {
            int index = 1;

            EFTChequeAuthResponse chqResponse = new EFTChequeAuthResponse();
            index++; // Skip sub code.
            chqResponse.Success = TryParse<bool>(response, 1, ref index);
            chqResponse.ResponseCode = TryParse<string>(response, 2, ref index);
            chqResponse.ResponseText = TryParse<string>(response, 20, ref index);

            if (response.Length > 25)
            {
                index += 2; // Skip merchant number.
                try { chqResponse.Amount = decimal.Parse(response.Substring(index, 9)) / 100; }
                catch { chqResponse.Amount = 0; }
                finally { index += 9; }
                try { chqResponse.AuthNumber = int.Parse(response.Substring(index, 6)); }
                catch { chqResponse.AuthNumber = 0; }
                finally { index += 6; }
                chqResponse.ReferenceNumber = response.Substring(index, 12); index += 12;
            }

            return chqResponse;
        }
        EFTResponse ParseGenericPOSCommandResponse(string response)
        {
            int index = 1;

            CommandType commandType = (CommandType)(char)response[index++];
            switch (commandType)
            {
                case CommandType.GetPassword:
                    EFTGetPasswordResponse pwdResponse = new EFTGetPasswordResponse
                    {
                        ResponseCode = response.Substring(index, 2)
                    };
                    index += 2;
                    pwdResponse.Success = pwdResponse.ResponseCode == "00";
                    pwdResponse.ResponseText = response.Substring(index, 20); index += 20;

                    if (response.Length > 25)
                    {
                        int pwdLength = 0;
                        try { pwdLength = int.Parse(response.Substring(index, 2)); }
                        finally { index += 2; }
                        pwdResponse.Password = response.Substring(index, pwdLength); index += pwdLength;
                    }
                    return pwdResponse;
                case CommandType.Slave:
                    EFTSlaveResponse slaveResponse = new EFTSlaveResponse
                    {
                        ResponseCode = response.Substring(index, 2)
                    };
                    index += 2;
                    slaveResponse.Response = response.Substring(index);
                    return slaveResponse;

                case CommandType.PayAtTable:
                    EFTPayAtTableResponse patResponse = new EFTPayAtTableResponse();
                    index = 22;

                    var headerLength = response.Substring(index, 6); index += 6;
                    int len = 0;
                    int.TryParse(headerLength, out len);

                    patResponse.Header = response.Substring(index, len); index += len;
                    patResponse.Content = response.Substring(index, response.Length - index);

                    return patResponse;

            }

            return null;
        }
        EFTResponse ParseConfigMerchantResponse(string msg)
        {
            int index = 1;

            EFTConfigureMerchantResponse eftResponse = new EFTConfigureMerchantResponse();
            index++; // Skip sub code.
            eftResponse.Success = TryParse<bool>(msg, 1, ref index);
            eftResponse.ResponseCode = TryParse<string>(msg, 2, ref index);
            eftResponse.ResponseText = TryParse<string>(msg, 20, ref index);

            return eftResponse;
        }
        EFTResponse ParseCloudLogonResponse(string msg)
        {
            int index = 1;

            var eftResponse = new EFTCloudLogonResponse();
            index++; // Skip sub code.
            eftResponse.Success = TryParse<bool>(msg, 1, ref index);
            eftResponse.ResponseCode = TryParse<string>(msg, 2, ref index);
            eftResponse.ResponseText = TryParse<string>(msg, 20, ref index);

            return eftResponse;
        }
        
        
        /// <summary>
        /// Convert a PC-EFTPOS message (e.g. #0010K0000) to a human readable debug string
        /// </summary>
        public static string MsgToDebugString(string msg)
        {
            if((msg?.Length ?? 0) < 2)
            {
                return $"Unable to parse msg{Environment.NewLine}ContentLength={msg?.Length ?? 0}{Environment.NewLine}Content={msg}";
            }

            // Remove the header if one exists
            if (msg[0] == '#')
            {
                if (msg.Length < 7)
                    return $"Unable to parse msg{Environment.NewLine}ContentLength={msg?.Length ?? 0}{Environment.NewLine}Content={msg}";

                msg = msg.Substring(5);
            }

            var messageParser = new MessageParser();
            try
            {
                var eftResponse = messageParser.StringToEFTResponse(msg);
                return PrintProperties(eftResponse);
            }
            catch(ArgumentException)
            {
                // Try StringToEFTRequest()
                return $"Unable to parse msg{Environment.NewLine}ContentLength={msg.Length}{Environment.NewLine}Content={msg}";
            }
        }

        static string PrintProperties(object obj)
        {
            if (obj == null)
                return "NULL";

            var sb = new StringBuilder();

            var objType = obj.GetType();
            var properties = objType.GetProperties();
            foreach (var p in properties)
            {
                sb.AppendFormat("{1}: {2}", p.Name, p.ToString());
            }

            return sb.ToString();
        }
        #endregion

        #region EFTResponseToString

        public string EFTRequestToString(EFTRequest eftRequest)
        {
            // Build the request string.
            var request = BuildRequest(eftRequest);
            var len = request.Length + 5;
            request.Insert(0, '#');
            request.Insert(1, len.PadLeft(4));
            return request.ToString();
        }

        StringBuilder BuildRequest(EFTRequest eftRequest)
        {
            if (eftRequest is EFTLogonRequest)
            {
                return BuildEFTLogonRequest((EFTLogonRequest)eftRequest);
            }

            if (eftRequest is EFTTransactionRequest)
            {
                return BuildEFTTransactionRequest((EFTTransactionRequest)eftRequest);
            }

            if (eftRequest is EFTGetLastTransactionRequest)
            {
                return BuildEFTGetLastTransactionRequest((EFTGetLastTransactionRequest)eftRequest);
            }

            if (eftRequest is EFTReprintReceiptRequest)
            {
                return BuildEFTReprintReceiptRequest((EFTReprintReceiptRequest)eftRequest);
            }

            if (eftRequest is SetDialogRequest)
            {
                return BuildSetDialogRequest((SetDialogRequest)eftRequest);
            }

            if (eftRequest is ControlPanelRequest)
            {
                return BuildControlPanelRequest((ControlPanelRequest)eftRequest);
            }

            if (eftRequest is EFTSettlementRequest)
            {
                return BuildSettlementRequest((EFTSettlementRequest)eftRequest);
            }

            if (eftRequest is EFTStatusRequest)
            {
                return BuildStatusRequest((EFTStatusRequest)eftRequest);
            }

            if (eftRequest is ChequeAuthRequest)
            {
                return BuildChequeAuthRequest((ChequeAuthRequest)eftRequest);
            }

            if (eftRequest is QueryCardRequest)
            {
                return BuildQueryCardRequest((QueryCardRequest)eftRequest);
            }

            if (eftRequest is EFTGetPasswordRequest)
            {
                return BuildGetPasswordRequest((EFTGetPasswordRequest)eftRequest);
            }

            if (eftRequest is EFTSlaveRequest)
            {
                return BuildSlaveRequest((EFTSlaveRequest)eftRequest);
            }

            if (eftRequest is EFTConfigureMerchantRequest)
            {
                return BuildConfigMerchantRequest((EFTConfigureMerchantRequest)eftRequest);
            }

            if (eftRequest is EFTCloudLogonRequest)
            {
                return BuildCloudLogonRequest((EFTCloudLogonRequest)eftRequest);
            }

            if (eftRequest is EFTClientListRequest)
            {
                return BuildGetClientListRequest((EFTClientListRequest)eftRequest);
            }

            if (eftRequest is EFTSendKeyRequest)
            {
                return BuildSendKeyRequest((EFTSendKeyRequest)eftRequest);
            }

            if (eftRequest is EFTReceiptRequest)
            {
                return BuildReceiptRequest((EFTReceiptRequest)eftRequest);
            }

            if (eftRequest is EFTPayAtTableRequest)
            {
                return BuildPayAtTableRequest((EFTPayAtTableRequest)eftRequest);
            }

            throw new Exception("Unknown EFTRequest type.");
        }
        StringBuilder BuildEFTTransactionRequest(EFTTransactionRequest v)
        {
            var r = new StringBuilder();
            r.Append("M");
            r.Append("0");
            r.Append(v.Application.ToMerchantString());
            r.Append((char)v.TxnType);
            r.Append(v.TrainingMode ? '1' : '0');
            r.Append(v.EnableTipping ? '1' : '0');
            r.Append(v.AmountCash.PadLeftAsInt(9));
            r.Append(v.AmountPurchase.PadLeftAsInt(9));
            r.Append(v.AuthNumber.PadLeft(6));
            r.Append(v.ReferenceNumber.PadRightAndCut(16));
            r.Append((char)v.ReceiptPrintMode);
            r.Append((char)v.ReceiptCutMode);
            r.Append((char)v.CardPANSource);
            r.Append(v.CardPAN.PadRightAndCut(20));
            r.Append(v.ExpiryDate.PadRightAndCut(4));
            r.Append(v.Track2.PadRightAndCut(40));
            r.Append((char)v.CardAccountType);
            r.Append(v.Application.ToApplicationString());
            r.Append(v.RRN.PadRightAndCut(12));
            r.Append(v.CurrencyCode.PadRightAndCut(3));
            r.Append((char)v.OriginalTxnType);
            r.Append(v.Date != null ? v.Date.Value.ToString("ddMMyy") : "      ");
            r.Append(v.Time != null ? v.Time.Value.ToString("HHmmss") : "      ");
            r.Append(" ".PadRightAndCut(8)); // Reserved
            r.Append(v.PurchaseAnalysisData.GetAsString(true));

            return r;
        }
        StringBuilder BuildEFTLogonRequest(EFTLogonRequest v)
        {
            var r = new StringBuilder();
            r.Append("G");
            r.Append((char)v.LogonType);
            r.Append(v.Application.ToMerchantString());
            r.Append((char)v.ReceiptPrintMode);
            r.Append((char)v.ReceiptCutMode);
            r.Append(v.Application.ToApplicationString());
            r.Append(v.PurchaseAnalysisData.GetAsString(true));
            return r;
        }
        StringBuilder BuildEFTReprintReceiptRequest(EFTReprintReceiptRequest v)
        {
            var r = new StringBuilder();
            r.Append("C");
            r.Append((char)v.ReprintType);
            r.Append(v.Application.ToMerchantString());
            r.Append((char)v.ReceiptCutMode);
            r.Append((char)v.ReceiptPrintMode);
            r.Append(v.Application.ToApplicationString());
            return r;
        }
        StringBuilder BuildEFTGetLastTransactionRequest(EFTGetLastTransactionRequest v)
        {
            var r = new StringBuilder();
            r.Append("N");
            r.Append("0");
            r.Append(v.Application.ToApplicationString());
            return r;
        }
        StringBuilder BuildSetDialogRequest(SetDialogRequest v)
        {
            var r = new StringBuilder();
            r.Append("2");
            r.Append(v.DisableDisplayEvents ? '5' : ' ');
            r.Append((char)v.Type);
            r.Append(v.DialogX.PadLeft(4));
            r.Append(v.DialogY.PadLeft(4));
            r.Append(v.Position.ToString().PadRightAndCut(12));
            r.Append(v.TopMost ? '1' : '0');
            r.Append(v.Title.PadRightAndCut(32));
            return r;
        }
        StringBuilder BuildControlPanelRequest(ControlPanelRequest v)
        {
            var r = new StringBuilder();
            r.Append("5"); // ControlPanel
            r.Append((char)v.ControlPanelType);
            r.Append((char)v.ReceiptPrintMode);
            r.Append((char)v.ReceiptCutMode);
            r.Append((char)v.ReturnType);
            return r;
        }
        StringBuilder BuildSettlementRequest(EFTSettlementRequest v)
        {
            var r = new StringBuilder();
            r.Append("P");
            r.Append((char)v.SettlementType);
            r.Append(v.Application.ToMerchantString());
            r.Append((char)v.ReceiptPrintMode);
            r.Append((char)v.ReceiptCutMode);
            r.Append(v.ResetTotals ? '1' : '0');
            r.Append(v.Application.ToApplicationString());
            r.Append(v.PurchaseAnalysisData.GetAsString(true));
            return r;
        }
        StringBuilder BuildQueryCardRequest(QueryCardRequest v)
        {
            var r = new StringBuilder();
            r.Append("J");
            r.Append((char)v.QueryCardType);
            r.Append(v.Application.ToApplicationString());
            r.Append(v.Application.ToMerchantString());
            r.Append(v.PurchaseAnalysisData.GetAsString(true));
            return r;
        }
        StringBuilder BuildConfigMerchantRequest(EFTConfigureMerchantRequest v)
        {
            var r = new StringBuilder();
            r.Append("10");
            r.Append(v.Application.ToMerchantString());
            r.Append(v.AIIC.PadLeft(11));
            r.Append(v.NII.PadLeft(3));
            r.Append(v.MerchantID.PadRightAndCut(15));
            r.Append(v.TerminalID.PadRightAndCut(8));
            r.Append(v.Timeout.PadLeft(3));
            r.Append(v.Application.ToApplicationString());
            return r;
        }
        StringBuilder BuildStatusRequest(EFTStatusRequest v)
        {
            var r = new StringBuilder();
            r.Append("K");
            r.Append((char)v.StatusType);
            r.Append(v.Application.ToMerchantString());
            r.Append(v.Application.ToApplicationString());
            return r;
        }
        StringBuilder BuildChequeAuthRequest(ChequeAuthRequest v)
        {
            var r = new StringBuilder();
            r.Append("H0");
            r.Append(v.Application.ToApplicationString());
            r.Append(' ');
            r.Append(v.BranchCode.PadRightAndCut(6));
            r.Append(v.AccountNumber.PadRightAndCut(14));
            r.Append(v.SerialNumber.PadRightAndCut(14));
            r.Append(v.Amount.PadLeftAsInt(9));
            r.Append((char)v.ChequeType);
            r.Append(v.ReferenceNumber.PadRightAndCut(12));

            return r;
        }

        StringBuilder BuildGetPasswordRequest(EFTGetPasswordRequest v)
        {
            var r = new StringBuilder();
            r.Append("X");
            r.Append((char)CommandType.GetPassword);
            r.Append(v.MinPasswordLength.PadLeft(2));
            r.Append(v.MaxPassworkLength.PadLeft(2));
            r.Append(v.Timeout.PadLeft(3));
            r.Append("0" + (char)v.PasswordDisplay);
            return r;
        }

        StringBuilder BuildSlaveRequest(EFTSlaveRequest v)
        {
            var r = new StringBuilder();
            r.Append("X");
            r.Append((char)CommandType.Slave);
            r.Append(v.Command);

            return r;
        }

        StringBuilder BuildGetClientListRequest(EFTClientListRequest v)
        {
            var r = new StringBuilder();
            r.Append("Q0");
            return r;
        }

        StringBuilder BuildCloudLogonRequest(EFTCloudLogonRequest v)
        {
            var r = new StringBuilder();
            r.Append("A ");
            r.Append(v.ClientID.PadRightAndCut(16));
            r.Append(v.Password.PadRightAndCut(16));
            r.Append(v.PairingCode.PadRightAndCut(16));
            return r;
        }

        StringBuilder BuildSendKeyRequest(EFTSendKeyRequest v)
        {
            var r = new StringBuilder();
            r.Append("Y0");
            r.Append((char)v.Key);
            if (v.Key == EFTPOSKey.Authorise && v.Data != null)
            {
                r.Append(v.Data.PadRightAndCut(20));
            }

            return r;
        }

        StringBuilder BuildReceiptRequest(EFTReceiptRequest v)
        {
            return new StringBuilder("3 ");
        }

        StringBuilder BuildPayAtTableRequest(EFTPayAtTableRequest request)
        {
            var r = new StringBuilder();
            r.Append("X");
            r.Append((char)CommandType.PayAtTable);
            r.Append(request.Header);
            r.Append(request.Content);

            return r;
        }

        #endregion
    }
}
