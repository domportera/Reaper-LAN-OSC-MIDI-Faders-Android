using UnityEngine;

public class Comment : MonoBehaviour
{
    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string notes = "Enter your comment here";

}
