using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Input;
using Debug = UnityEngine.Debug;

namespace Networking
{
	public sealed class MonoClient : MonoNetwork
	{
		[SerializeField] private string targetIP = "127.0.0.1";
		[SerializeField] private InputMaster InputMaster;

		[Header("Latency Display"), SerializeField] private float latencyUpdateInterval = .1f;
		[SerializeField] private TextMeshProUGUI tcpLatencyText;
		[SerializeField] private TextMeshProUGUI udpLatencyText;
		private StringBuilder stringBuilder = new StringBuilder();
		private int tcpLatency = 0;
		private int udpLatency = 0;

		private Vector2 movementInput;
		private Vector2 aimInput;

		private InputDataMessageUdp cachedUdpMessage;
		private InputDataMessageUdpStruct cachedUdpMessageAlt;
		private TcpMessage cachedTcpMessage;
		//private NetworkStream stream;
		private byte clientID = byte.MaxValue;
		private bool roundStarted = false;

		private void Start()
		{
			cachedUdpMessage = new InputDataMessageUdp();
			cachedUdpMessageAlt = new InputDataMessageUdpStruct();
			cachedTcpMessage = new TcpMessage();

			ConnectToServer(IPAddress.Parse(targetIP)); // DEBUG CALL
		}

		public void ConnectToServer(IPAddress targetIP)
		{
			SetupAsClient(true, true, targetIP);
			//StartCoroutine(UpdateLatencyDisplays());
		}

		private void PlayerAction(PlayerCharacter.ActionType action)
		{
			Debug.Log(action + " performed");
		}

		//private IEnumerator UpdateAndSendTcp()
		//{
		//	var message = new TcpMessage()
		//	{
		//		MessageType = (byte)MessageType.Gameplay,
		//		ClientID = clientID
		//	};

		//	var waiter = new WaitForSecondsRealtime(positionUpdateInterval);

		//	while(true)
		//	{
		//		//message.MillisecondTimestamp = GetTime;
		//		//message.PlayerPosition = localPlayer.transform.position;

		//		//SendTcpMessage(stream, message.ToArray());

		//		yield return waiter;
		//	}
		//}

		//private IEnumerator UpdateAndSendUdp()
		//{
		//	var message = new UdpMessage()
		//	{
		//		ClientID = clientID
		//	};

		//	var waiter = new WaitForSecondsRealtime(0f);
		//	while (true)
		//	{
		//		message.MillisecondTimestamp = GetTime;
		//		message.MovementInput = localPlayer.MovementInput;
		//		message.AimInput = localPlayer.AimInput;

		//		SendUdpMessage(message.ToArray());

		//		yield return waiter;
		//	}
		//}

		private IEnumerator UpdateLatencyDisplays()
		{
			var waiter = new WaitForSecondsRealtime(latencyUpdateInterval);
			while (true)
			{
				stringBuilder.Append("TCP: ").Append(Mathf.Max(0, tcpLatency)).Append(" ms");
				tcpLatencyText.text = stringBuilder.ToString();
				stringBuilder.Clear();

				stringBuilder.Append("UDP: ").Append(Mathf.Max(0, udpLatency)).Append(" ms");
				udpLatencyText.text = stringBuilder.ToString();
				stringBuilder.Clear();
				yield return waiter;
			}
		}


		protected override void OnTimerTick(object obj)
		{
			//cachedUdpMessage.MovementInput = movementInput;
			//cachedUdpMessage.AimInput = aimInput;
			//SendUdpMessage(cachedUdpMessage.ToArray());
			cachedUdpMessageAlt.Write(movementInput.x, movementInput.y, aimInput.x, aimInput.y);
			SendUdpMessage(cachedUdpMessageAlt.data);
		}

		protected override void UdpMessageReceived(IPEndPoint sender, byte[] messageBytes)
		{
			//var message = NetworkMessage.Parse<UdpMessage>(messageBytes);
			//udpLatency = GetTime - message.MillisecondTimestamp;
		}

		#region TCP
		protected override void TcpConnectionEstablished(NetworkStream stream)
		{
			//this.stream = stream;
		}

		protected override void TcpMessageReceived(NetworkStream sender, byte[] messageBytes)
		{
			var message = NetworkMessage.Parse<TcpMessage>(messageBytes);
			//tcpLatency = GetTime - message.MillisecondTimestamp;

			switch ((MessageType)message.MessageType)
			{
				case MessageType.Initialization:
					clientID = message.ClientID;
					break;

				case MessageType.Gameplay:

					break;

				case MessageType.Undefined:
				default:
					Debug.Log("Err");
					break;
			}
		}
		#endregion

		#region Client Input

		private void OnEnable()
		{
			InputMaster.Player.Movement.performed += UpdateMovementControlled;
			InputMaster.Player.Aim.performed += UpdateLookRotationControlled;
			InputMaster.Player.Shoot.performed += TriggerShotControlled;
			InputMaster.Player.Jump.performed += TriggerDash;
			InputMaster.Player.Parry.performed += TriggerParry;
			InputMaster.Player.LockAim.performed += TriggerAimLock;
			InputMaster.Player.Portal.performed += TriggerPortalOne;
		}

		private void OnDisable()
		{
			InputMaster.Player.Movement.performed -= UpdateMovementControlled;
			InputMaster.Player.Aim.performed -= UpdateLookRotationControlled;
			InputMaster.Player.Shoot.performed -= TriggerShotControlled;
			InputMaster.Player.Jump.performed -= TriggerDash;
			InputMaster.Player.Parry.performed -= TriggerParry;
			InputMaster.Player.LockAim.performed -= TriggerAimLock;
			InputMaster.Player.Portal.performed -= TriggerPortalOne;
		}

		private void UpdateMovementControlled(InputAction.CallbackContext ctx)
		{
			movementInput = ctx.ReadValue<Vector2>();
		}

		private void UpdateLookRotationControlled(InputAction.CallbackContext ctx)
		{
			aimInput = ctx.ReadValue<Vector2>();
		}

		private void TriggerShotControlled(InputAction.CallbackContext ctx)
		{

		}

		private void TriggerDash(InputAction.CallbackContext ctx)
		{

		}

		private void TriggerParry(InputAction.CallbackContext ctx)
		{

		}

		private void TriggerAimLock(InputAction.CallbackContext ctx)
		{

		}

		private void TriggerPortalOne(InputAction.CallbackContext ctx)
		{

		}
		#endregion

	}
}
