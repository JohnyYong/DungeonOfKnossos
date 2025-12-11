using UnityEngine;
using UnityEngine.PlayerLoop;

public class RoomData : MonoBehaviour
{
    [Header("Exit and Lock Configuration")]
    public bool hasTopExit;
    public bool hasRightExit;
    public bool hasBottomExit;
    public bool hasLeftExit;

    public bool isTopLocked;
    public bool isRightLocked;
    public bool isBottomLocked;
    public bool isLeftLocked;

    [Header("Anchor Points (Attach Transforms Here)")]
    public Transform topAnchor;
    public Transform rightAnchor;
    public Transform bottomAnchor;
    public Transform leftAnchor;

    GameObject walls;
    GameObject doors;

    public RoomExitConfig GetRotatedConfig(int rotation)
    {
        // rotation = 0 (0°), 1 (90° CCW), 2 (180°), 3 (270° CCW)
        bool[] exits = new bool[4] { hasTopExit, hasRightExit, hasBottomExit, hasLeftExit };
        bool[] locks = new bool[4] { isTopLocked, isRightLocked, isBottomLocked, isLeftLocked };
        bool[] rotatedExits = new bool[4];
        bool[] rotatedLocks = new bool[4];

        for (int i = 0; i < 4; ++i)
        {
            int rotatedIndex = (i - rotation + 4) % 4;
            rotatedExits[i] = exits[rotatedIndex];
            rotatedLocks[i] = locks[rotatedIndex];
        }

        return new RoomExitConfig
        {
            top = rotatedExits[0],
            right = rotatedExits[1],
            bottom = rotatedExits[2],
            left = rotatedExits[3],
            topLocked = rotatedLocks[0],
            rightLocked = rotatedLocks[1],
            bottomLocked = rotatedLocks[2],
            leftLocked = rotatedLocks[3],
            rotation = rotation
        };
    }

    // Get anchor transform in direction string ("Top", "Right", etc.)
    public Transform GetAnchor(string direction)
    {
        switch (direction)
        {
            case "Top": return topAnchor;
            case "Right": return rightAnchor;
            case "Bottom": return bottomAnchor;
            case "Left": return leftAnchor;
            default: return null;
        }
    }

    void OnDrawGizmos()
    {
        string[] dirs = { "TopExit", "RightExit", "BottomExit", "LeftExit" };
        foreach (string dir in dirs)
        {
            var anchor = transform.Find(dir + "Anchor");
            if (anchor)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(anchor.position, 0.1f);
            }
            else
            {
                Debug.Log("No anchor");
            }
        }
    }

}

public struct RoomExitConfig
{
    public bool top, right, bottom, left;
    public bool topLocked, rightLocked, bottomLocked, leftLocked;
    public int rotation;
}
