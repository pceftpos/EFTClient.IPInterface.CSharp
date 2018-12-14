using PCEFTPOS.EFTClient.IPInterface.Slave;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
	public class EftWrapper
	{
		IEFTClientIPAsync _eft = new EFTClientIPAsync();
		ClientData _data = null;
		CancellationTokenSource _ct = null;

		/// <summary>
		/// Set to true when a request is in progress and false when a request ends. Used to limit access to SendRequest
		/// </summary>
		bool _requestInProgress = false;

		//System.Timers.Timer _checkStatusTimer = null;

		public EftWrapper(ClientData data)
		{
			_data = data;

			//_checkStatusTimer = new System.Timers.Timer(15000 /* 15 seconds */);
			//_checkStatusTimer.Elapsed += _checkStatusTimer_Elapsed;

			_eft.OnLog += Eft_OnLog;
		}

		private string _eftLogs = string.Empty;
		private void Eft_OnLog(object sender, LogEventArgs e)
		{
			if (_data.SendKeyEnabled)
			{
				_eftLogs = e.Message;
			}
			else
			{
				var logType = LogType.Info;
				switch (e.LogLevel)
				{
					case LogLevel.Error:
					case LogLevel.Fatal:
						logType = LogType.Error;
						break;
					case LogLevel.Warn:
						logType = LogType.Warning;
						break;
					default:
						logType = LogType.Info;
						break;
				}

				var msg = (logType == LogType.Error && e.Exception != null) ? $"{e.Message} [{e.Exception.Message}]" : e.Message;

				_data.Log(msg, logType);
			}
		}

		#region Common
		private Dictionary<string, string> DictionaryFromType(object atype)
		{
			Dictionary<string, string> d = new Dictionary<string, string>();
			if (atype == null)
				return d;

			Type t = atype.GetType();

			try
			{
				foreach (PropertyInfo prp in t.GetProperties())
				{
					var p = prp.GetValue(atype, new object[] { });
					if (p != null)
					{
						string value = p.ToString();

						Type tt = p.GetType();
						if (p is TransactionType txn)
						{
							value = txn.ToTransactionString();
						}
						else if (tt.IsClass && tt != typeof(string) && !tt.IsArray)
						{
							var e = DictionaryFromType(p);
							if (e.Count > 0)
							{
								value = string.Empty;
								foreach (var i in e)
								{
									value += $"{i.Key}: {i.Value};{Environment.NewLine}";
								}
							}
						}
						else if (tt == typeof(string[]))
						{
							value = string.Empty;
							string[] arr = p as string[];
							foreach (string s in arr)
							{
								value += s + Environment.NewLine;
							}
							_data.Log(value);
						}

						d.Add(prp.Name, value);
					}
				}
			}
			catch
			{
			}
			return d;
		}

		private void ShowError(string hr, string message)
		{
			_data.LastTxnResult.Clear();
			var x = new Dictionary<string, string>
			{
				{ "Result", "Failed" },
				{ "ResponseCode", hr },
				{ "ResponseText", message }
			};
			_data.LastTxnResult = x;
		}

		private async Task<bool> SendRequest<T>(EFTRequest request, bool autoApproveDialogs = false)
		{
			bool result = false;

			if (_requestInProgress)
			{
				_data.Log($"Unable to process {request.ToString()}. There is already a request in progress");
				return false;
			}
			if (!_eft.IsConnected || _data.ConnectedState == ConnectedStatus.Disconnected)
			{
				await _eft.ConnectAsync("127.0.0.1", 2011, false);
				_data.ConnectedState = ConnectedStatus.Connected;
			}

			try
			{
				await _eft.WriteRequestAsync(request);

				_requestInProgress = true;
				do
				{
					var timeoutToken = new CancellationTokenSource(new TimeSpan(0, 5, 0)).Token; // 5 minute timeout
					if (request.GetType().Equals(typeof(EFTSlaveRequest)))
						_requestInProgress = false;
					var r = await _eft.ReadResponseAsync(timeoutToken);

					if (r == null) // stream is busy
					{
						_data.Log($"Unable to process {request.ToString()}. Stream is busy.");
					}
					else if (r is EFTDisplayResponse)
					{
						if (_data.Settings.DemoDialogOption != DemoDialogMode.Hide)
						{
							EFTDisplayResponse res = r as EFTDisplayResponse;
							_data.DisplayDetails = res;
							_data.DisplayDialog(true);
						}

						if (autoApproveDialogs && (r as EFTDisplayResponse).OKKeyFlag)
						{
							await _eft.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
						}
					}
					else if (r is EFTReceiptResponse || r is EFTReprintReceiptResponse)
					{
						// Hacked in some preprint and print response timeouts. It ain't pretty and it may not work, but it's my code.
						#region PrintResponse Timeout
						if (_data.IsPrePrintTimeOut)
						{
							if ((r as EFTReceiptResponse).IsPrePrint)
							{
								Thread.Sleep(59000);
							}
						}
						else if (_data.IsPrintTimeOut)
						{
							if (!(r as EFTReceiptResponse).IsPrePrint)
							{
								Thread.Sleep(59000);
							}
						}
						#endregion

						string[] receiptText = (r is EFTReceiptResponse) ? (r as EFTReceiptResponse).ReceiptText : (r as EFTReprintReceiptResponse).ReceiptText;

						StringBuilder receipt = new StringBuilder();
						foreach (var s in receiptText)
						{
							if (s.Length > 0)
							{
								receipt.AppendLine(s);
							}
						}

						if (!string.IsNullOrEmpty(receipt.ToString()))
						{
							_data.Receipt = receipt.ToString();
						}

						if (request is EFTReceiptRequest || (request is EFTReprintReceiptRequest && request.GetPairedResponseType() == r.GetType()))
						{
							_requestInProgress = false;
							_eft.Dispose();
						}
					}
					else if (r is EFTQueryCardResponse)
					{
						_data.LastTxnResult = DictionaryFromType(r);
						var x = (EFTQueryCardResponse)r;
						_data.SelectedTrack2 = x.Track2;
						_data.DisplayDialog(false);
						_requestInProgress = false;
						_eft.Dispose();
					}
					else if (r is EFTClientListResponse)
					{
						int index = 1;
						var x = (EFTClientListResponse)r;
						Dictionary<string, string> ClientList = new Dictionary<string, string>();
						foreach (var clnt in x.EFTClients)
						{
							ClientList.Add("Client " + index.ToString() + " " + nameof(clnt.Name), clnt.Name);
							ClientList.Add("Client " + index.ToString() + " " + nameof(clnt.IPAddress), clnt.IPAddress);
							ClientList.Add("Client " + index.ToString() + " " + nameof(clnt.Port), clnt.Port.ToString());
							ClientList.Add("Client " + index.ToString() + " " + nameof(clnt.State), clnt.State.ToString());
							index++;
						}

						_data.LastTxnResult = ClientList;
						_data.DisplayDialog(false);
						_requestInProgress = false;
						_eft.Dispose();
					}
					else
					{
						_data.LastTxnResult = DictionaryFromType(r);
						string output = string.Empty;
						_data.LastTxnResult.TryGetValue("Success", out output);
						result = (output.Equals("True"));
						_data.DisplayDialog(false);
						_requestInProgress = false;
						_eft.Dispose();
					}
				}
				while (_requestInProgress);

				if (!_requestInProgress)
				{
					_data.Log($"Request: {request.ToString()} done!");
				}
			}
			catch (TaskCanceledException)
			{
				_data.ConnectedState = ConnectedStatus.Disconnected;
				ShowError("EFT-Client Timeout", "");
			}
			catch (ConnectionException cx)
			{
				_data.ConnectedState = ConnectedStatus.Disconnected;
				ShowError(cx.HResult.ToString(), cx.Message);
			}
			catch (Exception ex)
			{
				ShowError(ex.HResult.ToString(), ex.Message);
			}

			_requestInProgress = false;
			return result;
		}

		#endregion

		#region Connect

		public async Task<bool> Connect(string ip, int port, bool useSSL)
		{
			if (_data.ConnectedState == ConnectedStatus.Connected)
			{
				return true;
			}

			try
			{
				if (await _eft.ConnectAsync(ip, port, useSSL))
				{
					_data.ConnectedState = ConnectedStatus.Connected;
					//_checkStatusTimer.Enabled = true;
					return true;
				}
			}
			catch (Exception ex)
			{
				ShowError(ex.HResult.ToString(), ex.Message);
			}
			return false;
		}

		private async void _checkStatusTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (_requestInProgress == false)
			{
				_requestInProgress = true;
				if (await _eft.CheckConnectStateAsync() == false)
				{
					_data.Log("CheckConnectState failed. Connection has been dropped.");
					Disconnect();
				}
				_requestInProgress = false;
			}
		}

		public void Disconnect()
		{
			try
			{
				//_checkStatusTimer.Enabled = false;
				_data.ConnectedState = ConnectedStatus.Disconnected;
				_eft.Disconnect();
			}
			catch (Exception ex)
			{
				ShowError(ex.HResult.ToString(), ex.Message);
			}
		}

		#endregion

		#region Logon
		public async Task Logon(LogonType logonType, ReceiptCutModeType cutMode, ReceiptPrintModeType printMode, bool autoApproveDialogs)
		{
			await SendRequest<EFTLogonResponse>(new EFTLogonRequest()
			{
				LogonType = logonType,
				CutReceipt = cutMode,
				ReceiptAutoPrint = printMode
			}, autoApproveDialogs);
		}

		public async Task CloudLogon(string clientId, string password, string pairingCode)
		{
			await SendRequest<EFTCloudLogonResponse>(new EFTCloudLogonRequest()
			{
				ClientID = clientId,
				Password = password,
				PairingCode = pairingCode
			});
		}

		public async Task StartLogonTest(LogonType type, ReceiptCutModeType cutMode, ReceiptPrintModeType printMod)
		{
			_ct = new CancellationTokenSource();
			await SpawnLogon(type, cutMode, printMod, _ct.Token);
		}

		public void StopLogonTest()
		{
			_ct.Cancel();
			_ct.Dispose();
		}

		private async Task SpawnLogon(LogonType type, ReceiptCutModeType cutMode, ReceiptPrintModeType printMod, CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(10);
				await Logon(type, cutMode, printMod, true);
			}
		}

		#endregion

		#region ControlPanel
		public async Task DisplayControlPanel(ControlPanelType option, ReceiptCutModeType cutMode, ReceiptPrintModeType printMode)
		{
			try
			{
				await _eft.WriteRequestAsync(new EFTControlPanelRequest()
				{
					ControlPanelType = option,
					ReceiptCutMode = cutMode,
					ReceiptPrintMode = printMode,
					ReturnType = ControlPanelReturnType.Immediately
				});


				bool waiting = true;
				do
				{
					var timeoutToken = new CancellationTokenSource(new TimeSpan(0, 5, 0)).Token; // 5 minute timeout
					var r = await _eft.ReadResponseAsync(timeoutToken);

					if (r is EFTDisplayResponse)
					{
						if (_data.Settings.DemoDialogOption != DemoDialogMode.Hide)
						{
							EFTDisplayResponse res = r as EFTDisplayResponse;
							_data.DisplayDetails = res;
							_data.DisplayDialog(true);
						}
					}
					else if (r is EFTReceiptResponse || r is EFTReprintReceiptResponse)
					{
						string[] receiptText = (r is EFTReceiptResponse) ? (r as EFTReceiptResponse).ReceiptText : (r as EFTReprintReceiptResponse).ReceiptText;

						StringBuilder receipt = new StringBuilder();
						foreach (var s in receiptText)
						{
							if (s.Length > 0)
							{
								receipt.AppendLine(s);
							}
						}

						if (!string.IsNullOrEmpty(receipt.ToString()))
						{
							_data.Receipt = receipt.ToString();
						}
					}
					else
					{
						_data.LastTxnResult = DictionaryFromType(r);
						_data.DisplayDialog(false);
					}
				}
				while (waiting);

			}
			catch (Exception ex)
			{
				ShowError(ex.HResult.ToString(), ex.Message);
			}

		}
		#endregion

		#region Last Transaction
		public async Task GetLastTransaction()
		{
			await SendRequest<EFTGetLastTransactionResponse>(new EFTGetLastTransactionRequest(_data.OriginalTransactionReference) { });
		}

		public async Task LastReceipt(ReceiptCutModeType cutMode, ReceiptPrintModeType printMode, ReprintType type)
		{
			await SendRequest<EFTReprintReceiptResponse>(new EFTReprintReceiptRequest()
			{
				CutReceipt = cutMode,
				ReceiptAutoPrint = printMode,
				ReprintType = type,
				OriginalTxnRef = _data.OriginalTransactionReference,
				Merchant = _data.LastReceiptMerchantNumber
			});
		}
		#endregion

		#region Transaction



		public async Task DoTransaction(EFTTransactionRequest request)
		{
			bool result = await SendRequest<EFTTransactionResponse>(request);
			if (result)
			{
				_data.TransactionReference = string.Empty;
			}
		}
		#endregion

		#region ClientList

		public async Task DoClientList(EFTClientListRequest clientListRequest)
		{
			bool result = await SendRequest<EFTClientListResponse>(clientListRequest);
			if (result)
			{

			}

		}
		#endregion

		#region Set Dialog
		public async Task SetDialog(int x, int y, bool displayEvents, DialogPosition pos, string title, bool topMost, DialogType dialogType)
		{
			await SendRequest<SetDialogResponse>(new SetDialogRequest
			{
				DialogX = x,
				DialogY = y,
				DisableDisplayEvents = displayEvents,
				DialogPosition = pos,
				DialogTitle = title,
				EnableTopmost = topMost,
				DialogType = dialogType
			});
		}

		public async Task SetDialog(SetDialogRequest request)
		{
			await SendRequest<SetDialogResponse>(request);
		}

		#endregion

		#region Query Card
		public async Task QueryCard(PadField pad, QueryCardType cardType)
		{
			await SendRequest<EFTQueryCardResponse>(new EFTQueryCardRequest
			{
				QueryCardType = cardType,
				PurchaseAnalysisData = pad
			});
		}
		#endregion

		#region Status
		public async Task GetStatus(StatusType statusType)
		{
			await SendRequest<EFTStatusResponse>(new EFTStatusRequest
			{
				StatusType = statusType
			});
		}
		#endregion

		#region Config Merchant
		public async Task ConfigureMerchant(int aiic, string merchantId, int nii, string terminalId, int timeout)
		{
			await SendRequest<EFTConfigureMerchantResponse>(new EFTConfigureMerchantRequest
			{
				AIIC = aiic,
				Caid = merchantId,
				NII = nii,
				Catid = terminalId,
				Timeout = timeout
			});
		}

		public async Task ConfigureMerchant(EFTConfigureMerchantRequest request)
		{
			await SendRequest<EFTConfigureMerchantResponse>(request);
		}
		#endregion

		#region Settlement
		public async Task DoSettlement(SettlementType settlement, ReceiptCutModeType cutMode,
			PadField padInfo, ReceiptPrintModeType printMode, bool resetTotals)
		{
			await SendRequest<EFTSettlementResponse>(new EFTSettlementRequest
			{
				SettlementType = settlement,
				CutReceipt = cutMode,
				PurchaseAnalysisData = padInfo,
				ReceiptAutoPrint = printMode,
				ResetTotals = resetTotals
			});
		}
		#endregion

		#region Cheque Auth
		public async Task DoVerifyCheque(EFTChequeAuthRequest request)
		{
			await SendRequest<EFTChequeAuthResponse>(request);
		}
		#endregion

		#region Slave Mode
		public async Task DoSlaveMode(string cmd)
		{
			await SendRequest<Slave.EFTSlaveResponse>(new Slave.EFTSlaveRequest
			{
				RawCommand = cmd
			});
			await Task.CompletedTask;
		}
		#endregion

		#region SendKey
		public async Task SendKey(EFTPOSKey option, string data = "")
		{
			try
			{
				await _eft.WriteRequestAsync(new EFTSendKeyRequest
				{
					Data = data,
					Key = option
				});
			}
			catch (Exception ex)
			{
				ShowError(ex.HResult.ToString(), ex.Message);
			}
		}

		public async Task StartSendKeysTest(EFTPOSKey key)
		{
			_ct = new CancellationTokenSource();

			var progress = new Progress<string>((s) =>
			{
				_data.Log(s);
			});

			_data.Progress = progress;

			await Task.Run(() => SpawnSendKeys(key, _ct.Token, progress), _ct.Token);
		}

		public void StopSendKeysTest()
		{
			_ct.Cancel();
			_ct.Dispose();
		}

		private async Task SpawnSendKeys(EFTPOSKey key, CancellationToken token, IProgress<string> p)
		{
			try
			{
				while (!token.IsCancellationRequested)
				{
					await Task.Delay(100);
					await _eft.WriteRequestAsync(new EFTSendKeyRequest
					{
						Key = key
					});
					p.Report(_eftLogs);
				}
			}
			catch (Exception ex)
			{
				ShowError(ex.HResult.ToString(), ex.Message);
			}
		}
		#endregion

		#region PIN
		public async Task AuthPin()
		{
			await SendRequest<EFTTransactionResponse>(new EFTTransactionRequest
			{
				TxnType = TransactionType.AuthPIN
			});
		}

		public async Task ChangePin()
		{
			await SendRequest<EFTTransactionResponse>(new EFTTransactionRequest
			{
				TxnType = TransactionType.EnhancedPIN
			});
		}
		#endregion

	}
}
