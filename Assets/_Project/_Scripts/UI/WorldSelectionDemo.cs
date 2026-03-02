using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldSelectionDemo : MonoBehaviour
{
    public TMP_InputField worldIdInput;
    public Button joinButton;
    public Transform _worldSelectionPanel;

    private bool _isPanelOpen = false;

    private void Start()
    {
        joinButton.onClick.AddListener(JoinWorld);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }
    }

    private void JoinWorld()
    {
        int worldId = worldIdInput.text == "" ? 0 : int.Parse(worldIdInput.text);

        Player localPlayer = null;
        foreach (var p in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if (p.IsOwner)
            {
                localPlayer = p;
                break;
            }
        }

        if (localPlayer == null)
        {
            Debug.LogWarning("No local player found!");
            return;
        }

        localPlayer.RequestJoinWorld(worldId);
    }

    private void TogglePanel()
    {
        _isPanelOpen = !_isPanelOpen;
        _worldSelectionPanel.gameObject.SetActive(_isPanelOpen);
    }
}
