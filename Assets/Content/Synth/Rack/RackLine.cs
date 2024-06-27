using NaughtyAttributes;
using UnityEngine;

[ExecuteAlways]
public class RackLine : MonoBehaviour
{
    [Required] public GameObject socketPrefab;
    [Required] public Transform socketsParent;

    [Required] public Transform startPoint;

    public int numberOfSockets = 10;
    public float socketSpacing = 0.075f;

    // Start is called before the first frame update
    private void Start()
    {
        CreateSocketLine();
    }

    [Button("GenerateSocketLine")]
    private void CreateSocketLine()
    {
        if (!socketPrefab)
            return;

        for (var i = socketsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(socketsParent.GetChild(i).gameObject);
        }

        var positon = startPoint.position + startPoint.right * (socketSpacing / 2);

        SocketArrayInteractor previous = null;
        for (int i = 0; i < numberOfSockets; i++)
        {
            var socket = Instantiate(socketPrefab, positon, startPoint.rotation, socketsParent)
                .GetComponent<SocketArrayInteractor>();
            positon += startPoint.right * socketSpacing;

            if (previous)
            {
                socket.right = previous;
                previous.left = socket;
            }

            previous = socket;
        }
    }
}