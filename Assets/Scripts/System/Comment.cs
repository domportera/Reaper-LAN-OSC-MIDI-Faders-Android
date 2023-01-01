using UnityEngine;
using UnityEngine.Serialization;

public class Comment : MonoBehaviour
{
    [FormerlySerializedAs("notes")]
    [TextArea(3,100)]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "Enter your comment here";

}
