using NativeWebSocket;
using SocketIOClient;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
public class WebSocketManager : MonoBehaviour
{
    public SocketIOUnity socket;
    GameManager GM;

    void Start()
    {
        GM=GetComponent<GameManager>();
        socket = new SocketIOUnity("http://localhost:3000");
        socket.OnConnected += (sender, e) => {
            PuanGonder(GM.playerName, (int)GM.puan);
        };
        socket.Connect();
    }

    public void PuanGonder(string isim, int puan)
    {
        
        var veri = new { username = isim, score = puan };
        socket.Emit("updateScore", veri);
    }
    public void AktifiSil(string isim)
    {
        var veri = new { username = isim };
        socket.Emit("aktifisil", veri);        Debug.Log("Aktif Sil");

    }
}