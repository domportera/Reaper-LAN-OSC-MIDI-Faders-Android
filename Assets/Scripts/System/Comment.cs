using UnityEngine;

public class Comment : MonoBehaviour
{
    [TextArea(3,100)]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string notes = "Enter your comment here";

}
