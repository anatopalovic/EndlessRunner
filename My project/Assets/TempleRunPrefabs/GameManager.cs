using System.Collections;
using LootLocker.Requests;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private UnityEvent playerConected;


    IEnumerator Start()
    {
        bool connected = false;
        LootLockerSDKManager.StartGuestSession(response => {
            if(!response.success)
            {
                Debug.Log("Error starting Lootlocker session.");
                return;
            }

            Debug.Log("Successfully started Lootlocker session");
            connected = true;
        });

        yield return new WaitUntil(() => connected);
        playerConected.Invoke();
    }
}
