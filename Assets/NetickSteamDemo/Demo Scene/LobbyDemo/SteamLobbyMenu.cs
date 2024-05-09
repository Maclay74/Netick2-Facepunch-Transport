using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.UI;
using Network = Netick.Unity.Network;

namespace Netick.Examples.Steam {
    public class SteamLobbyMenu : MonoBehaviour {
        public static SteamLobbyMenu instance;

        public GameObject SearchMenu;
        public GameObject LobbyMenu;
        public GameObject LobbyContent;
        public GameObject LobbyInfoPrefab;
        public Button StartServerButton;
        public Button ConnectToServerButton;
        public Button StopServerButton;

        bool locked;
        bool WasRunningLastFrame;
        void Awake() {
            if (instance == null) {
                instance = this;
                SteamLobbyExample.OnLobbyEnteredEvent += JoinedLobby;
                SteamLobbyExample.OnLobbyLeftEvent += LeftLobby;
                SteamLobbyExample.OnLobbySearchStart += ClearLobbyList;
                SteamLobbyExample.OnLobbySearchFinished += UpdateLobbyList;
                SteamLobbyExample.OnGameServerShutdown += ResetLobbyCamera;
            }
            else
                Destroy(gameObject);
        }
        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape))
                ToggleCursor();

            var IsRunning = Network.IsRunning;

            if (WasRunningLastFrame != IsRunning) {
                if (IsRunning) {
                    StartServerButton.interactable = false;
                    ConnectToServerButton.interactable = false;
                    StopServerButton.interactable = true;
                }
                else {
                    var IsOwner = SteamLobbyExample.CurrentLobby.IsOwnedBy(SteamClient.SteamId);
                    if (IsOwner) {
                        StartServerButton.interactable = true;
                        ConnectToServerButton.interactable = false;
                    }
                    else {
                        StartServerButton.interactable = false;
                        ConnectToServerButton.interactable = true;
                    }
                    StopServerButton.interactable = false;
                }
            }

            WasRunningLastFrame = IsRunning;
        }

        void ToggleCursor() {
            locked = !locked;
            if (locked) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
        }

        public void ClearLobbyList() {
            for (var i = 0; i < LobbyContent.transform.childCount; i++) {
                Destroy(LobbyContent.transform.GetChild(i).gameObject);
            }
        }

        public void UpdateLobbyList(List<Lobby> LobbyList) {
            foreach (var lobby in LobbyList) {
                var lobbyGO = Instantiate(LobbyInfoPrefab, LobbyContent.transform);
                lobbyGO.transform.GetChild(0).GetComponent<Text>().text = lobby.GetData("LobbyName");
                lobbyGO.GetComponent<Button>().onClick.AddListener(async () => {
                    await SteamLobbyExample.JoinLobby(lobby.Id);
                });
            }
        }

        public void JoinedLobby(Lobby lobby) {
            var IsOwner = lobby.IsOwnedBy(SteamClient.SteamId);
            if (IsOwner) {
                StartServerButton.interactable = true;
                ConnectToServerButton.interactable = false;
            }
            else {
                StartServerButton.interactable = false;
                ConnectToServerButton.interactable = true;
            }
            SearchMenu.SetActive(false);
            LobbyMenu.SetActive(true);
        }

        public void LeftLobby() {
            SearchMenu.SetActive(true);
            LobbyMenu.SetActive(false);
        }

        public void ResetLobbyCamera() {
            FindObjectOfType<Camera>().transform.SetParent(null);
        }
    }
}
