import { HubConnection } from "@microsoft/signalr";

export class VoiceManager {
    private peerConnection: RTCPeerConnection | null = null;
    private localStream: MediaStream | null = null;
    private signalR: HubConnection;

    constructor(signalRConnection: HubConnection) {
    this.signalR = signalRConnection;
  }

  public async joinSession(token: string){
    //Setup microphone
    this.localStream = await navigator.mediaDevices.getUserMedia({
        audio: { echoCancellation: true, noiseSuppression: true},
      
      
        video: false
    })

    
  }
   
}